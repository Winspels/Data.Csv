using System;
using System.Data;
using System.IO;
using System.Text;

namespace Winspels.Data.Csv
{
	public sealed class CsvWriter: IDisposable
	{
		public CsvWriter(string fileName) : this(fileName, ',', Encoding.Default)
		{
		}

		public CsvWriter(TextWriter outputStream, char delimiter)
		{
			this.outputStream = null;
			fileName = null;
			firstColumn = true;
			Delimiter = ',';
			TextQualifier = '"';
			UseTextQualifier = true;
			recordDelimiter = '\0';
			useCustomRecordDelimiter = false;
			Comment = '#';
			encoding = null;
			ForceQualifier = false;
			EscapeMode = EscapeMode.Doubled;
			initialized = false;
			disposed = false;
			if (outputStream == null)
			{
				throw new ArgumentNullException("outputStream", "Output stream can not be null.");
			}
			this.outputStream = outputStream;
			Delimiter = delimiter;
			initialized = true;
		}

		public CsvWriter(Stream outputStream, char delimiter, Encoding encoding) : this(new StreamWriter(outputStream, encoding), delimiter)
		{
		}

		public CsvWriter(string fileName, char delimiter, Encoding encoding)
		{
			outputStream = null;
			this.fileName = null;
			firstColumn = true;
			Delimiter = ',';
			TextQualifier = '"';
			UseTextQualifier = true;
			recordDelimiter = '\0';
			useCustomRecordDelimiter = false;
			Comment = '#';
			this.encoding = null;
			ForceQualifier = false;
			EscapeMode = EscapeMode.Doubled;
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
			this.fileName = fileName;
			Delimiter = delimiter;
			this.encoding = encoding;
		}

		private void CheckDisposed()
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().FullName, "This object has been previously disposed. Methods on this object can no longer be called.");
			}
		}

		private void CheckInit()
		{
			if (!initialized)
			{
				if (fileName != null)
				{
					outputStream = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 0x2800, false), encoding);
				}
				initialized = true;
			}
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
				}
				if (initialized)
				{
					outputStream.Dispose();
				}
				outputStream = null;
				disposed = true;
			}
		}

		public void EndRecord()
		{
			CheckDisposed();
			CheckInit();
			if (useCustomRecordDelimiter)
			{
				outputStream.Write(recordDelimiter);
			}
			else
			{
				outputStream.WriteLine();
			}
			firstColumn = true;
		}

		~CsvWriter()
		{
			Dispose(false);
		}

		public void Flush()
		{
			outputStream.Flush();
		}

		void IDisposable.Dispose()
		{
			if (!disposed)
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
		}

		public void Write(string content)
		{
			Write(content, false);
		}

		public void Write(string content, bool preserveSpaces)
		{
			char ch3;
			CheckDisposed();
			CheckInit();
			if (content == null)
			{
				content = "";
			}
			if (!firstColumn)
			{
				outputStream.Write(Delimiter);
			}
			bool flag1 = ForceQualifier;
			if (!preserveSpaces && (content.Length > 0))
			{
				content = content.Trim(new char[] { ' ', '\t' });
			}
			if (!flag1 && UseTextQualifier && ((!useCustomRecordDelimiter && (content.IndexOfAny(new char[] { '\n', '\r', TextQualifier, Delimiter }) > -1)) || (useCustomRecordDelimiter && (content.IndexOfAny(new char[] { recordDelimiter, TextQualifier, Delimiter }) > -1)) || (firstColumn && (content.Length > 0) && (content[0] == Comment)) || (firstColumn && (content.Length == 0))))
			{
				flag1 = true;
			}
			if (UseTextQualifier && !flag1 && (content.Length > 0) && preserveSpaces)
			{
				switch (content[0])
				{
					case ' ':
					case '\t':
					flag1 = true;
					break;
				}
				if (!flag1 && (content.Length > 1))
				{
					switch (content[^1])
					{
						case ' ':
						case '\t':
						flag1 = true;
						break;
					}
				}
			}
			if (flag1)
			{
				outputStream.Write(TextQualifier);
				if (EscapeMode == EscapeMode.Backslash)
				{
					if (content.IndexOf('\\') > -1)
					{
						ch3 = '\\';
						content = content.Replace(ch3.ToString(), ch3.ToString() + '\\');
					}
					if (content.IndexOf(TextQualifier) > -1)
					{
						content = content.Replace(TextQualifier.ToString(), '\\'.ToString() + TextQualifier);
					}
				}
				else if (content.IndexOf(TextQualifier) > -1)
				{
					content = content.Replace(TextQualifier.ToString(), TextQualifier.ToString() + TextQualifier);
				}
			}
			else if (EscapeMode == EscapeMode.Backslash)
			{
				if (content.IndexOf('\\') > -1)
				{
					ch3 = '\\';
					content = content.Replace(ch3.ToString(), ch3.ToString() + '\\');
				}
				if (content.IndexOf(Delimiter) > -1)
				{
					content = content.Replace(Delimiter.ToString(), '\\'.ToString() + Delimiter);
				}
				if (useCustomRecordDelimiter)
				{
					if (content.IndexOf(recordDelimiter) > -1)
					{
						content = content.Replace(recordDelimiter.ToString(), '\\'.ToString() + recordDelimiter);
					}
				}
				else
				{
					if (content.IndexOf('\r') > -1)
					{
						ch3 = '\\';
						content = content.Replace(ch3.ToString(), ch3.ToString() + '\r');
					}
					if (content.IndexOf('\n') > -1)
					{
						ch3 = '\\';
						content = content.Replace(ch3.ToString(), ch3.ToString() + '\n');
					}
				}
				if (firstColumn && (content.Length > 0) && (content[0] == Comment))
				{
					content = content.Length > 1 ? '\\'.ToString() + Comment + content.Substring(1) : '\\'.ToString() + Comment;
				}
			}
			outputStream.Write(content);
			if (flag1)
			{
				outputStream.Write(TextQualifier);
			}
			firstColumn = false;
		}

		public void WriteAll(DataTable data)
		{
			WriteAll(data, true);
		}

		public void WriteAll(DataTable data, bool writeHeaders)
		{
			if (writeHeaders)
			{
				foreach (DataColumn column1 in data.Columns)
				{
					Write(column1.Caption);
				}
				EndRecord();
			}
			int num1 = data.Columns.Count;
			foreach (DataRow row1 in data.Rows)
			{
				for (int num2 = 0; num2 < num1; num2++)
				{
					Write(row1[num2].ToString());
				}
				EndRecord();
			}
			outputStream.Flush();
		}

		public void WriteComment(string commentText)
		{
			CheckDisposed();
			CheckInit();
			outputStream.Write(Comment);
			outputStream.Write(commentText);
			if (useCustomRecordDelimiter)
			{
				outputStream.Write(recordDelimiter);
			}
			else
			{
				outputStream.WriteLine();
			}
			firstColumn = true;
		}

		public void WriteRecord(string[] values)
		{
			WriteRecord(values, false);
		}

		public void WriteRecord(string[] values, bool preserveSpaces)
		{
			if ((values != null) && (values.Length > 0))
			{
				foreach (string text1 in values)
				{
					Write(text1, preserveSpaces);
				}
				EndRecord();
			}
		}


		public char Comment { get; set; }

		public char Delimiter { get; set; }

		public EscapeMode EscapeMode { get; set; }

		public bool ForceQualifier { get; set; }

		public char RecordDelimiter
		{
			get => useCustomRecordDelimiter ? recordDelimiter : '\0';
			set
			{
				useCustomRecordDelimiter = true;
				recordDelimiter = value;
			}
		}

		public char TextQualifier { get; set; }

		public bool UseTextQualifier { get; set; }


		private const char Backslash = '\\';
		private const char Comma = ',';
		private const char Cr = '\r';
		private bool disposed;
		private Encoding encoding;
		private readonly string fileName;
		private bool firstColumn;
		private bool initialized;
		private const char Lf = '\n';
		private const int MaxFileBufferSize = 0x2800;
		private const char NullChar = '\0';
		private TextWriter outputStream;
		private const char Pound = '#';
		private const char Quote = '"';
		private char recordDelimiter;
		private const char Space = ' ';
		private const char Tab = '\t';
		private bool useCustomRecordDelimiter;
	}
}

