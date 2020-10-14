using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Winspels.Data.Csv
{
	public sealed class CsvReader: IDisposable, IEnumerator, IEnumerable
	{
		public CsvReader(TextReader inputStream) : this(inputStream, ',')
		{
		}

		public CsvReader(string fileName) : this(fileName, ',')
		{
		}

		public CsvReader(Stream inputStream, Encoding encoding) : this(new StreamReader(inputStream, encoding, true))
		{
		}

		public CsvReader(TextReader inputStream, char delimiter)
		{
			this.inputStream = null;
			fileName = null;
			encoding = null;
			TextQualifier = '"';
			TrimWhitespace = true;
			UseTextQualifier = true;
			detectBom = false;
			Delimiter = ',';
			recordDelimiter = '\0';
			useCustomRecordDelimiter = false;
			Comment = '#';
			UseComments = false;
			EscapeMode = EscapeMode.Doubled;
			SafetySwitch = true;
			SkipEmptyRecords = true;
			headers = null;
			HeaderCount = 0;
			headerIndexByName = new Hashtable();
			values = null;
			columnBuffer = new char[100];
			columnBufferSize = 100;
			usedColumnLength = 0;
			columnStart = 0;
			dataBuffer = new char[0x400];
			bufferPosition = 0;
			bufferCount = 0;
			maxColumnCount = 10;
			columns = new ColumnChunk[10];
			ColumnCount = 0;
			currentRecord = 0;
			startedColumn = false;
			startedWithQualifier = false;
			hasMoreData = true;
			lastLetter = '\0';
			hasReadNextLine = false;
			readingHeaders = false;
			skippingRecord = false;
			initialized = false;
			disposed = false;
			if (inputStream == null)
			{
				throw new ArgumentNullException("inputStream", "Input stream can not be null.");
			}
			this.inputStream = inputStream;
			Delimiter = delimiter;
			initialized = true;
		}

		public CsvReader(string fileName, char delimiter) : this(fileName, delimiter, Encoding.Default)
		{
			detectBom = true;
		}

		public CsvReader(Stream inputStream, char delimiter, Encoding encoding) : this(new StreamReader(inputStream, encoding, true), delimiter)
		{
		}

		public CsvReader(string fileName, char delimiter, Encoding encoding)
		{
			inputStream = null;
			this.fileName = null;
			this.encoding = null;
			TextQualifier = '"';
			TrimWhitespace = true;
			UseTextQualifier = true;
			detectBom = false;
			Delimiter = ',';
			recordDelimiter = '\0';
			useCustomRecordDelimiter = false;
			Comment = '#';
			UseComments = false;
			EscapeMode = EscapeMode.Doubled;
			SafetySwitch = true;
			SkipEmptyRecords = true;
			headers = null;
			HeaderCount = 0;
			headerIndexByName = new Hashtable();
			values = null;
			columnBuffer = new char[100];
			columnBufferSize = 100;
			usedColumnLength = 0;
			columnStart = 0;
			dataBuffer = new char[0x400];
			bufferPosition = 0;
			bufferCount = 0;
			maxColumnCount = 10;
			columns = new ColumnChunk[10];
			ColumnCount = 0;
			currentRecord = 0;
			startedColumn = false;
			startedWithQualifier = false;
			hasMoreData = true;
			lastLetter = '\0';
			hasReadNextLine = false;
			readingHeaders = false;
			skippingRecord = false;
			initialized = false;
			disposed = false;
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName", "File name can not be null.");
			}
			if (encoding == null)
			{
				throw new ArgumentNullException("encoding", "Encoding can not be null.");
			}
			if (!File.Exists(fileName))
			{
				throw new FileNotFoundException("File does not exist.", fileName);
			}
			this.fileName = fileName;
			Delimiter = delimiter;
			this.encoding = encoding;
		}

		private void AddLetter(char letter)
		{
			if (!skippingRecord)
			{
				if (((usedColumnLength - columnStart) >= 0x186a0) && SafetySwitch)
				{
					Close();
					throw new IOException("Maximum column length of 100,000 exceeded in column " + ColumnCount.ToString("###,##0") + " in record " + currentRecord.ToString("###,##0") + ". Set the SafetySwitch property to false if you're expecting column lengths greater than 100,000 characters to avoid this error.");
				}
				if (usedColumnLength == columnBufferSize)
				{
					int num1 = columnBufferSize + Math.Max(1, columnBufferSize * 1 / 2);
					char[] chArray1 = new char[num1];
					Array.Copy(columnBuffer, 0, chArray1, 0, columnBufferSize);
					columnBuffer = chArray1;
					columnBufferSize = num1;
				}
				columnBuffer[usedColumnLength++] = letter;
			}
		}

		private void CheckDataLength()
		{
			if (!initialized)
			{
				if (fileName != null)
				{
					inputStream = new StreamReader(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 0x1000, false), encoding, detectBom);
				}
				encoding = null;
				initialized = true;
			}
			if (bufferPosition == bufferCount)
			{
				try
				{
					bufferCount = inputStream.Read(dataBuffer, 0, 0x400);
				}
				catch
				{
					Close();
					throw;
				}
				bufferPosition = 0;
				if (bufferCount == 0)
				{
					hasMoreData = false;
				}
			}
		}

		private void CheckDisposed()
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().FullName, "This object has been previously disposed. Methods on this object can no longer be called.");
			}
		}

		private void ClearColumns()
		{
			values = null;
			ColumnCount = 0;
			columnStart = 0;
			usedColumnLength = 0;
		}

		public void Close()
		{
			Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					encoding = null;
					headers = null;
					headerIndexByName = null;
					columnBuffer = null;
					dataBuffer = null;
					columns = null;
				}
				if (initialized)
				{
					inputStream.Dispose();
				}
				inputStream = null;
				disposed = true;
			}
		}

		private void EndColumn()
		{
			startedColumn = false;
			if (!skippingRecord)
			{
				if ((ColumnCount >= 0x186a0) && SafetySwitch)
				{
					Close();
					throw new IOException("Maximum column count of 100,000 exceeded in record " + currentRecord.ToString("###,##0") + ". Set the SafetySwitch property to false if you're expecting more than 100,000 columns per record to avoid this error.");
				}
				if (ColumnCount == maxColumnCount)
				{
					int num1 = maxColumnCount + Math.Max(1, maxColumnCount * 1 / 2);
					var chunkArray1 = new ColumnChunk[num1];
					Array.Copy(columns, 0, chunkArray1, 0, maxColumnCount);
					columns = chunkArray1;
					maxColumnCount = num1;
				}
				if ((usedColumnLength - columnStart) > 0)
				{
					if (TrimWhitespace && !startedWithQualifier)
					{
						int num2 = usedColumnLength - 1;
						if (columnStart < num2)
						{
							while ((num2 > columnStart) && ((columnBuffer[num2] == ' ') || (columnBuffer[num2] == '\t')))
							{
								num2--;
							}
						}
						columns[ColumnCount] = new ColumnChunk(columnStart, num2 - columnStart + 1, startedWithQualifier);
					}
					else
					{
						columns[ColumnCount] = new ColumnChunk(columnStart, usedColumnLength - columnStart, startedWithQualifier);
					}
					columnStart = usedColumnLength;
				}
				else
				{
					columns[ColumnCount] = new ColumnChunk(columnStart, 0, startedWithQualifier);
				}
				ColumnCount++;
			}
		}

		private void EndRecord()
		{
			if (!skippingRecord)
			{
				hasReadNextLine = true;
				if (readingHeaders)
				{
					HeaderCount = ColumnCount;
					headers = new string[HeaderCount];
					for (int num1 = 0; num1 < HeaderCount; num1++)
					{
						string text1 = this[num1];
						headers[num1] = text1;
						headerIndexByName[text1] = num1;
					}
				}
				else
				{
					currentRecord++;
				}
			}
			else
			{
				hasReadNextLine = true;
			}
		}

		~CsvReader()
		{
			Dispose(false);
		}

		public string GetHeader(int columnIndex)
		{
			CheckDisposed();
			return (columnIndex > -1) && (columnIndex < HeaderCount) ? headers[columnIndex] : "";
		}

		public int GetIndex(string headerName)
		{
			CheckDisposed();
			object obj1 = headerIndexByName[headerName];
			return obj1 != null ? (int)obj1 : -1;
		}

		public int GetLength(int columnIndex)
		{
			CheckDisposed();
			return (columnIndex < ColumnCount) && (columnIndex > -1) ? columns[columnIndex].Length : 0;
		}

		private static char HexToDec(char hex)
		{
			return hex >= 'a' ? (char)(hex - 'a' + '\n') : hex >= 'A' ? (char)(hex - 'A' + '\n') : (char)(hex - '0');
		}

		public bool IsQualified(int columnIndex)
		{
			CheckDisposed();
			return (columnIndex < ColumnCount) && (columnIndex > -1) && columns[columnIndex].Qualified;
		}

		public static CsvReader Parse(string data)
		{
			if (data == null)
				throw new ArgumentNullException("data", "Data can not be null.");

			return new CsvReader(new StringReader(data));
		}

		public bool ReadHeaders()
		{
			readingHeaders = true;
			bool flag1 = ReadRecord();
			readingHeaders = false;
			ClearColumns();
			return flag1;
		}

		public bool ReadRecord()
		{
			CheckDisposed();
			ClearColumns();
			hasReadNextLine = false;
			if (hasMoreData)
			{
				do
				{
					while (!hasReadNextLine && (bufferPosition < bufferCount))
					{
						startedWithQualifier = false;
						char ch1 = dataBuffer[bufferPosition++];
						if (UseTextQualifier && (ch1 == TextQualifier))
						{
							lastLetter = ch1;
							startedColumn = true;
							startedWithQualifier = true;
							bool flag1 = false;
							char ch2 = TextQualifier;
							if (EscapeMode == EscapeMode.Backslash)
							{
								ch2 = '\\';
							}
							bool flag2 = false;
							bool flag3 = false;
							bool flag4 = false;
							var escape1 = ComplexEscape.Unicode;
							int num1 = 0;
							char ch3 = '\0';
							do
							{
								while (startedColumn && (bufferPosition < bufferCount))
								{
									ch1 = dataBuffer[bufferPosition++];
									if (flag2)
									{
										if (ch1 == Delimiter)
										{
											EndColumn();
											goto Label_040E;
										}
										if ((!useCustomRecordDelimiter && ((ch1 == '\r') || (ch1 == '\n'))) || (useCustomRecordDelimiter && (ch1 == recordDelimiter)))
										{
											EndColumn();
											EndRecord();
										}
										goto Label_040E;
									}
									if (!flag4)
									{
										goto Label_01B9;
									}
									num1++;
									switch (escape1)
									{
										case ComplexEscape.Unicode:
										ch3 *= '\x0010';
										ch3 += HexToDec(ch1);
										if (num1 == 4)
										{
											flag4 = false;
										}
										goto Label_01A5;

										case ComplexEscape.Octal:
										ch3 *= '\b';
										ch3 += (char)(ch1 - '0');
										if (num1 == 3)
										{
											flag4 = false;
										}
										goto Label_01A5;

										case ComplexEscape.Decimal:
										ch3 *= '\n';
										ch3 += (char)(ch1 - '0');
										if (num1 == 3)
										{
											flag4 = false;
										}
										goto Label_01A5;

										case ComplexEscape.Hex:
										break;

										default:
										goto Label_01A5;
									}
									ch3 *= '\x0010';
									ch3 += HexToDec(ch1);
									if (num1 == 2)
									{
										flag4 = false;
									}
								Label_01A5:
									if (!flag4)
									{
										AddLetter(ch3);
									}
									goto Label_040E;
								Label_01B9:
									if (ch1 == TextQualifier)
									{
										if (flag3)
										{
											flag3 = false;
											flag1 = false;
											AddLetter(TextQualifier);
											goto Label_040E;
										}
										if (EscapeMode == EscapeMode.Doubled)
										{
											flag3 = true;
										}
										flag1 = true;
										goto Label_040E;
									}
									if ((EscapeMode != EscapeMode.Backslash) || !flag3)
									{
										goto Label_03B3;
									}
									switch (ch1)
									{
										case 'a':
										AddLetter('\a');
										goto Label_03AE;

										case 'b':
										AddLetter('\b');
										goto Label_03AE;

										case 'c':
										case 'p':
										case 'q':
										case 's':
										case 'w':
										goto Label_03A7;

										case 'd':
										case 'o':
										case 'u':
										case 'x':
										case 'U':
										case 'X':
										case 'D':
										case 'O':
										switch (ch1)
										{
											case 'x':
											case 'X':
											goto Label_038F;

											case 'd':
											case 'D':
											goto Label_0399;

											case 'o':
											case 'O':
											goto Label_0394;
										}
										goto Label_039C;

										case 'e':
										AddLetter('\x001b');
										goto Label_03AE;

										case 'f':
										AddLetter('\f');
										goto Label_03AE;

										case 'n':
										AddLetter('\n');
										goto Label_03AE;

										case 'r':
										AddLetter('\r');
										goto Label_03AE;

										case 't':
										AddLetter('\t');
										goto Label_03AE;

										case 'v':
										AddLetter('\v');
										goto Label_03AE;

										case '0':
										case '1':
										case '2':
										case '3':
										case '4':
										case '5':
										case '6':
										case '7':
										escape1 = ComplexEscape.Octal;
										flag4 = true;
										num1 = 1;
										ch3 = (char)(ch1 - '0');
										goto Label_03AE;

										default:
										goto Label_03A7;
									}
								Label_038F:
									escape1 = ComplexEscape.Hex;
									goto Label_039C;
								Label_0394:
									escape1 = ComplexEscape.Octal;
									goto Label_039C;
								Label_0399:
									escape1 = ComplexEscape.Decimal;
								Label_039C:
									flag4 = true;
									num1 = 0;
									ch3 = '\0';
									goto Label_03AE;
								Label_03A7:
									AddLetter(ch1);
								Label_03AE:
									flag3 = false;
									goto Label_040E;
								Label_03B3:
									if (ch1 == ch2)
									{
										flag3 = true;
									}
									else if (flag1)
									{
										if (ch1 == Delimiter)
										{
											EndColumn();
										}
										else if ((!useCustomRecordDelimiter && ((ch1 == '\r') || (ch1 == '\n'))) || (useCustomRecordDelimiter && (ch1 == recordDelimiter)))
										{
											EndColumn();
											EndRecord();
										}
										else
										{
											flag2 = true;
										}
										flag1 = false;
									}
									else
									{
										AddLetter(ch1);
									}
								Label_040E:
									lastLetter = ch1;
								}
								CheckDataLength();
							}
							while (hasMoreData && startedColumn);
							continue;
						}
						if (ch1 == Delimiter)
						{
							lastLetter = ch1;
							EndColumn();
							continue;
						}
						if (useCustomRecordDelimiter && (ch1 == recordDelimiter))
						{
							if (startedColumn || (ColumnCount > 0) || !SkipEmptyRecords)
							{
								EndColumn();
								EndRecord();
							}
							lastLetter = ch1;
							continue;
						}
						if (!useCustomRecordDelimiter && ((ch1 == '\r') || (ch1 == '\n')))
						{
							if (startedColumn || (ColumnCount > 0) || (!SkipEmptyRecords && ((ch1 == '\r') || (lastLetter != '\r'))))
							{
								EndColumn();
								EndRecord();
							}
							lastLetter = ch1;
							continue;
						}
						if (UseComments && (ColumnCount == 0) && (ch1 == Comment))
						{
							lastLetter = ch1;
							SkipLine();
							continue;
						}
						if (TrimWhitespace && ((ch1 == ' ') || (ch1 == '\t')))
						{
							startedColumn = true;
							continue;
						}
						startedColumn = true;
						bool flag5 = false;
						bool flag6 = false;
						var escape2 = ComplexEscape.Unicode;
						int num2 = 0;
						char ch4 = '\0';
						bool flag7 = true;
					Label_055E:
						if (!flag7)
						{
							ch1 = dataBuffer[bufferPosition++];
						}
						if (!UseTextQualifier && (EscapeMode == EscapeMode.Backslash) && (ch1 == '\\'))
						{
							if (flag5)
							{
								AddLetter('\\');
								flag5 = false;
								goto Label_0879;
							}
							flag5 = true;
							goto Label_0879;
						}
						if (!flag6)
						{
							goto Label_0660;
						}
						num2++;
						switch (escape2)
						{
							case ComplexEscape.Unicode:
							ch4 *= '\x0010';
							ch4 += HexToDec(ch1);
							if (num2 == 4)
							{
								flag6 = false;
							}
							goto Label_064C;

							case ComplexEscape.Octal:
							ch4 *= '\b';
							ch4 += (char)(ch1 - '0');
							if (num2 == 3)
							{
								flag6 = false;
							}
							goto Label_064C;

							case ComplexEscape.Decimal:
							ch4 *= '\n';
							ch4 += (char)(ch1 - '0');
							if (num2 == 3)
							{
								flag6 = false;
							}
							goto Label_064C;

							case ComplexEscape.Hex:
							break;

							default:
							goto Label_064C;
						}
						ch4 *= '\x0010';
						ch4 += HexToDec(ch1);
						if (num2 == 2)
						{
							flag6 = false;
						}
					Label_064C:
						if (!flag6)
						{
							AddLetter(ch4);
						}
						goto Label_0879;
					Label_0660:
						if (UseTextQualifier || (EscapeMode != EscapeMode.Backslash) || !flag5)
						{
							goto Label_0830;
						}
						switch (ch1)
						{
							case 'a':
							AddLetter('\a');
							goto Label_082B;

							case 'b':
							AddLetter('\b');
							goto Label_082B;

							case 'c':
							case 'p':
							case 'q':
							case 's':
							case 'w':
							goto Label_0824;

							case 'd':
							case 'o':
							case 'u':
							case 'x':
							case 'U':
							case 'X':
							case 'D':
							case 'O':
							switch (ch1)
							{
								case 'x':
								case 'X':
								goto Label_080C;

								case 'd':
								case 'D':
								goto Label_0816;

								case 'o':
								case 'O':
								goto Label_0811;
							}
							goto Label_0819;

							case 'e':
							AddLetter('\x001b');
							goto Label_082B;

							case 'f':
							AddLetter('\f');
							goto Label_082B;

							case 'n':
							AddLetter('\n');
							goto Label_082B;

							case 'r':
							AddLetter('\r');
							goto Label_082B;

							case 't':
							AddLetter('\t');
							goto Label_082B;

							case 'v':
							AddLetter('\v');
							goto Label_082B;

							case '0':
							case '1':
							case '2':
							case '3':
							case '4':
							case '5':
							case '6':
							case '7':
							escape2 = ComplexEscape.Octal;
							flag6 = true;
							num2 = 1;
							ch4 = (char)(ch1 - '0');
							goto Label_082B;

							default:
							goto Label_0824;
						}
					Label_080C:
						escape2 = ComplexEscape.Hex;
						goto Label_0819;
					Label_0811:
						escape2 = ComplexEscape.Octal;
						goto Label_0819;
					Label_0816:
						escape2 = ComplexEscape.Decimal;
					Label_0819:
						flag6 = true;
						num2 = 0;
						ch4 = '\0';
						goto Label_082B;
					Label_0824:
						AddLetter(ch1);
					Label_082B:
						flag5 = false;
						goto Label_0879;
					Label_0830:
						if (ch1 == Delimiter)
						{
							EndColumn();
						}
						else if ((!useCustomRecordDelimiter && ((ch1 == '\r') || (ch1 == '\n'))) || (useCustomRecordDelimiter && (ch1 == recordDelimiter)))
						{
							EndColumn();
							EndRecord();
						}
						else
						{
							AddLetter(ch1);
						}
					Label_0879:
						lastLetter = ch1;
						flag7 = false;
						if (startedColumn && (bufferPosition < bufferCount))
						{
							goto Label_055E;
						}
						CheckDataLength();
						if (hasMoreData && startedColumn)
						{
							goto Label_055E;
						}
					}
					CheckDataLength();
				}
				while (hasMoreData && !hasReadNextLine);
				if (startedColumn || (lastLetter == Delimiter))
				{
					EndColumn();
					EndRecord();
				}
			}
			return hasReadNextLine;
		}

		public DataTable ReadToEnd()
		{
			return ReadToEnd(true);
		}

		public DataTable ReadToEnd(bool readHeaders)
		{
			return ReadToEnd(readHeaders, 0);
		}

		public DataTable ReadToEnd(bool readHeaders, ulong maxRecords)
		{
			var table1 = new DataTable();
			table1.BeginLoadData();
			if (readHeaders)
			{
				ReadHeaders();
				bool flag1 = true;
				for (int num1 = 0; num1 < HeaderCount; num1++)
				{
					if (flag1)
					{
						string text1 = headers[num1];
						if (table1.Columns.Contains(text1))
						{
							for (int num2 = 0; num2 < num1; num2++)
							{
								table1.Columns[num2].ColumnName = "Column" + (num2 + 1);
							}
							table1.Columns.Add("Column" + (num1 + 1));
							flag1 = false;
						}
						else
						{
							table1.Columns.Add(text1);
						}
					}
					else
					{
						table1.Columns.Add("Column" + (num1 + 1));
					}
				}
			}
			int num3 = HeaderCount;
			bool flag2 = maxRecords > 0;
			var collection1 = table1.Rows;
			while ((!flag2 || (currentRecord < maxRecords)) && ReadRecord())
			{
				if (ColumnCount > num3)
				{
					for (int num4 = num3; num4 < ColumnCount; num4++)
					{
						table1.Columns.Add("Column" + (num4 + 1));
					}
					num3 = ColumnCount;
				}
				collection1.Add(Values);
			}
			table1.EndLoadData();
			return table1;
		}

		public bool SkipLine()
		{
			CheckDisposed();
			ClearColumns();
			bool flag1 = false;
			if (hasMoreData)
			{
				bool flag2 = false;
				do
				{
					while (!flag2 && (bufferPosition < bufferCount))
					{
						flag1 = true;
						char ch1 = dataBuffer[bufferPosition++];
						switch (ch1)
						{
							case '\r':
							case '\n':
							flag2 = true;
							break;
						}
						lastLetter = ch1;
					}
					CheckDataLength();
				}
				while (hasMoreData && !flag2);
			}
			return flag1;
		}

		public bool SkipRecord()
		{
			CheckDisposed();
			bool flag1 = false;
			if (hasMoreData)
			{
				skippingRecord = true;
				flag1 = ReadRecord();
				skippingRecord = false;
			}
			return flag1;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this;
		}

		bool IEnumerator.MoveNext()
		{
			return ReadRecord();
		}

		void IEnumerator.Reset()
		{
			throw new NotSupportedException("Reset is not currently supported by the IEnumerable implementation supplied by Csv.CsvReader.");
		}

		void IDisposable.Dispose()
		{
			if (!disposed)
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
		}


		public int ColumnCount { get; private set; }

		public char Comment { get; set; }

		public ulong CurrentRecord => currentRecord - 1;

		public char Delimiter { get; set; }

		public EscapeMode EscapeMode { get; set; }

		public int HeaderCount { get; private set; }

		public string[] Headers
		{
			get
			{
				CheckDisposed();
				return headers;
			}
			set
			{
				headers = value;
				headerIndexByName.Clear();
				HeaderCount = headers != null ? headers.Length : 0;
				for (int num1 = 0; num1 < HeaderCount; num1++)
				{
					headerIndexByName[headers[num1]] = num1;
				}
			}
		}

		public string this[string headerName]
		{
			get
			{
				CheckDisposed();
				return this[GetIndex(headerName)];
			}
		}

		public string this[int columnIndex]
		{
			get
			{
				string text1;
				CheckDisposed();
				if ((columnIndex > -1) && (columnIndex < ColumnCount))
				{
					var chunk1 = columns[columnIndex];
					text1 = chunk1.Length == 0 ? "" : new string(columnBuffer, chunk1.Start, chunk1.Length);
				}
				else
				{
					text1 = "";
				}
				return text1;
			}
		}

		public char RecordDelimiter
		{
			get => useCustomRecordDelimiter ? recordDelimiter : '\0';
			set
			{
				useCustomRecordDelimiter = true;
				recordDelimiter = value;
			}
		}

		public bool SafetySwitch { get; set; }

		public bool SkipEmptyRecords { get; set; }

		object IEnumerator.Current => Values;

		public char TextQualifier { get; set; }

		public bool TrimWhitespace { get; set; }

		public bool UseComments { get; set; }

		public bool UseTextQualifier { get; set; }

		public string[] Values
		{
			get
			{
				CheckDisposed();
				if ((values == null) && initialized)
				{
					values = new string[ColumnCount];
					for (int num1 = 0; num1 < ColumnCount; num1++)
					{
						values[num1] = this[num1];
					}
				}
				return values;
			}
		}


		private const char Alert = '\a';
		private const char Backslash = '\\';
		private const char Backspace = '\b';
		private int bufferCount;
		private int bufferPosition;
		private char[] columnBuffer;
		private int columnBufferSize;
		private ColumnChunk[] columns;
		private int columnStart;
		private const char Comma = ',';
		private const char Cr = '\r';
		private ulong currentRecord;
		private char[] dataBuffer;
		private readonly bool detectBom;
		private bool disposed;
		private Encoding encoding;
		private const char Escape = '\x001b';
		private readonly string fileName;
		private const char FormFeed = '\f';
		private bool hasMoreData;
		private bool hasReadNextLine;
		private Hashtable headerIndexByName;
		private string[] headers;
		private const int InitialColumnBufferSize = 100;
		private bool initialized;
		private const int InitialMaxColumnCount = 10;
		private TextReader inputStream;
		private char lastLetter;
		private const char Lf = '\n';
		private const int MaxBufferSize = 0x400;
		private int maxColumnCount;
		private const int MaxFileBufferSize = 0x1000;
		private const char NullChar = '\0';
		private const char Pound = '#';
		private const char Quote = '"';
		private bool readingHeaders;
		private char recordDelimiter;
		private bool skippingRecord;
		private const char Space = ' ';
		private bool startedColumn;
		private bool startedWithQualifier;
		private const char Tab = '\t';
		private bool useCustomRecordDelimiter;
		private int usedColumnLength;
		private string[] values;
		private const char VerticalTab = '\v';


		[StructLayout(LayoutKind.Sequential)]
		private struct ColumnChunk
		{
			public readonly int Start;
			public readonly int Length;
			public readonly bool Qualified;
			public ColumnChunk(int start, int length, bool qualified)
			{
				Start = start;
				Length = length;
				Qualified = qualified;
			}

		}

		private enum ComplexEscape
		{
			Unicode,
			Octal,
			Decimal,
			Hex
		}
	}
}

