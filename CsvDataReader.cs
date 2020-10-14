using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;

namespace Winspels.Data.Csv
{
	public sealed class CsvDataReader: IDataReader, IDataRecord, IDisposable
	{
		public event ReadRecordEventHandler ReadRecord;

		public CsvDataReader(TextReader inputStream)
		{
			csvReader = null;
			columnIndexByName = new Hashtable();
			columns = new ColumnCollection();
			HasHeaders = false;
			disposed = false;
			initialized = false;
			csvReader = new CsvReader(inputStream);
		}

		public CsvDataReader(string fileName)
		{
			csvReader = null;
			columnIndexByName = new Hashtable();
			columns = new ColumnCollection();
			HasHeaders = false;
			disposed = false;
			initialized = false;
			csvReader = new CsvReader(fileName);
		}

		public CsvDataReader(Stream inputStream, Encoding encoding)
		{
			csvReader = null;
			columnIndexByName = new Hashtable();
			columns = new ColumnCollection();
			HasHeaders = false;
			disposed = false;
			initialized = false;
			csvReader = new CsvReader(inputStream, encoding);
		}

		public CsvDataReader(TextReader inputStream, char delimiter)
		{
			csvReader = null;
			columnIndexByName = new Hashtable();
			columns = new ColumnCollection();
			HasHeaders = false;
			disposed = false;
			initialized = false;
			csvReader = new CsvReader(inputStream, delimiter);
		}

		public CsvDataReader(string fileName, char delimiter)
		{
			csvReader = null;
			columnIndexByName = new Hashtable();
			columns = new ColumnCollection();
			HasHeaders = false;
			disposed = false;
			initialized = false;
			csvReader = new CsvReader(fileName, delimiter);
		}

		public CsvDataReader(Stream inputStream, char delimiter, Encoding encoding)
		{
			csvReader = null;
			columnIndexByName = new Hashtable();
			columns = new ColumnCollection();
			HasHeaders = false;
			disposed = false;
			initialized = false;
			csvReader = new CsvReader(inputStream, delimiter, encoding);
		}

		public CsvDataReader(string fileName, char delimiter, Encoding encoding)
		{
			csvReader = null;
			columnIndexByName = new Hashtable();
			columns = new ColumnCollection();
			HasHeaders = false;
			disposed = false;
			initialized = false;
			csvReader = new CsvReader(fileName, delimiter, encoding);
		}

		private void CheckColumnIndex(int i)
		{
			if ((i < 0) || (i >= columns.Count))
			{
				throw new IndexOutOfRangeException("No column was found at index " + i.ToString("###,##0") + " in column collection of length " + columns.Count.ToString("###,##0") + ".");
			}
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
				if (columns.Count == 0)
				{
					throw new InvalidOperationException("At least one column must exist in the column collection. Columns may be added to the collection using the Columns property and its Add method.");
				}
				values = new string[columns.Count];
				names = new string[columns.Count];
				if (HasHeaders && csvReader.ReadHeaders())
				{
					for (int num1 = 0; (num1 < names.Length) && (num1 < csvReader.HeaderCount); num1++)
					{
						names[num1] = csvReader.GetHeader(num1);
					}
				}
				for (int num2 = 0; num2 < columns.Count; num2++)
				{
					string text1 = columns[num2].Name;
					if (text1 != null)
					{
						names[num2] = text1;
					}
					else
					{
						text1 = names[num2];
					}
					if (text1 != null)
					{
						columnIndexByName[text1] = num2;
					}
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
					columns = null;
					values = null;
					names = null;
					columnIndexByName = null;
				}
				((IDisposable)csvReader).Dispose();
				csvReader = null;
				disposed = true;
			}
		}

		~CsvDataReader()
		{
			Dispose(false);
		}

		private bool OnReadRecord()
		{
			bool flag1 = false;
			if (ReadRecord != null)
			{
				var args1 = new ReadRecordEventArgs(values);
				ReadRecord(args1);
				flag1 = args1.SkipRecord;
			}
			return flag1;
		}

		public static CsvDataReader Parse(string data)
		{
			return data == null ? throw new ArgumentNullException("data", "Data can not be null.") : new CsvDataReader(new StringReader(data));
		}

		DataTable IDataReader.GetSchemaTable()
		{
			return null;
		}

		bool IDataReader.NextResult()
		{
			return false;
		}

		bool IDataReader.Read()
		{
			CheckDisposed();
			CheckInit();
			bool flag1 = true;
			bool flag2 = true;
			while (flag1 && flag2)
			{
				flag1 = csvReader.ReadRecord();
				if (flag1)
				{
					for (int num1 = 0; num1 < values.Length; num1++)
					{
						values[num1] = csvReader[num1];
					}
					flag2 = OnReadRecord();
				}
			}
			return flag1;
		}

		public bool GetBoolean(int i)
		{
			return (bool)GetValue(i);
		}

		public byte GetByte(int i)
		{
			return (byte)GetValue(i);
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			throw new NotSupportedException("GetBytes is not currently supported by the IDataRecord implementation supplied by Csv.CsvDataReader.");
		}

		public char GetChar(int i)
		{
			return (char)GetValue(i);
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			throw new NotSupportedException("GetChars is not currently supported by the IDataRecord implementation supplied by Csv.CsvDataReader.");
		}

		public IDataReader GetData(int i)
		{
			return i == 0 ? this : null;
		}

		public string GetDataTypeName(int i)
		{
			CheckDisposed();
			CheckColumnIndex(i);
			return columns[i].DataTypeName;
		}

		public DateTime GetDateTime(int i)
		{
			return (DateTime)GetValue(i);
		}

		public decimal GetDecimal(int i)
		{
			return (decimal)GetValue(i);
		}

		public double GetDouble(int i)
		{
			return (double)GetValue(i);
		}

		public Type GetFieldType(int i)
		{
			CheckDisposed();
			CheckColumnIndex(i);
			return columns[i].FieldType;
		}

		public float GetFloat(int i)
		{
			return (float)GetValue(i);
		}

		public Guid GetGuid(int i)
		{
			return (Guid)GetValue(i);
		}

		public short GetInt16(int i)
		{
			return (short)GetValue(i);
		}

		public int GetInt32(int i)
		{
			return (int)GetValue(i);
		}

		public long GetInt64(int i)
		{
			return (long)GetValue(i);
		}

		public string GetName(int i)
		{
			CheckDisposed();
			CheckInit();
			CheckColumnIndex(i);
			return names[i];
		}

		public int GetOrdinal(string name)
		{
			CheckDisposed();
			CheckInit();
			object obj1 = columnIndexByName[name];
			return obj1 != null ? (int)obj1 : -1;
		}

		public string GetString(int i)
		{
			return GetValue(i).ToString();
		}

		public object GetValue(int i)
		{
			CheckDisposed();
			CheckColumnIndex(i);
			string text1 = values[i];
			var column1 = columns[i];
			var provider1 = column1.FormatProvider;
			var type1 = column1.DbType;
			if ((type1 != DbType.String) && (text1 != null))
			{
				text1 = text1.Trim();
			}
			object obj1 = null;
			if ((text1 != null) && ((text1.Length > 0) || ((type1 == DbType.String) && (text1.Length == 0) && csvReader.IsQualified(i))))
			{
				try
				{
					switch (type1)
					{
						case DbType.Byte:
						return Byte.Parse(text1, NumberStyles.Any, provider1);

						case DbType.Boolean:
						switch (text1.ToUpper())
						{
							case "TRUE":
							case "T":
							case "YES":
							case "Y":
							case "1":
							case "-1":
							return true;

							case "FALSE":
							case "F":
							case "NO":
							case "N":
							case "0":
							return false;
						}
						break;

						case DbType.Currency:
						case DbType.Date:
						case DbType.Object:
						case DbType.SByte:
						return obj1;

						case DbType.DateTime:
						{
							string text2 = column1.Format;
							return text2 != null ? DateTime.ParseExact(text1, text2, provider1) : (object)DateTime.Parse(text1, provider1);
						}
						case DbType.Decimal:
						return Decimal.Parse(text1, NumberStyles.Any, provider1);

						case DbType.Double:
						return Double.Parse(text1, NumberStyles.Any, provider1);

						case DbType.Guid:
						return new Guid(text1);

						case DbType.Int16:
						return Int16.Parse(text1, NumberStyles.Any, provider1);

						case DbType.Int32:
						return Int32.Parse(text1, NumberStyles.Any, provider1);

						case DbType.Int64:
						return Int64.Parse(text1, NumberStyles.Any, provider1);

						case DbType.Single:
						return Single.Parse(text1, NumberStyles.Any, provider1);

						case DbType.String:
						return text1;

						default:
						return obj1;
					}
					obj1 = Boolean.Parse(text1);
				}
				catch (Exception exception1)
				{
					ThrowHelpfulException(i, exception1, column1.FieldType);
				}
				return obj1;
			}
			return column1.DefaultValue;
		}

		public int GetValues(object[] values)
		{
			int num1 = Math.Min(this.values.Length, values.Length);
			for (int num2 = 0; num2 < num1; num2++)
			{
				values[num2] = GetValue(num2);
			}
			return num1;
		}

		public bool IsDBNull(int i)
		{
			CheckDisposed();
			CheckColumnIndex(i);
			string text1 = values[i];
			var type1 = columns[i].DbType;
			if ((type1 != DbType.String) && (text1 != null))
			{
				text1 = text1.Trim();
			}
			if (text1 == null)
			{
				return true;
			}
			if (text1.Length == 0)
			{
				if (type1 != DbType.String)
				{
					return true;
				}
				if (type1 == DbType.String)
				{
					return !csvReader.IsQualified(i);
				}
			}
			return false;
		}

		void IDisposable.Dispose()
		{
			if (!disposed)
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
		}

		private void ThrowHelpfulException(int columnIndex, Exception e, Type type)
		{
			if (e is FormatException)
			{
				throw new FormatException("Value \"" + values[columnIndex] + "\" can not be converted to type " + type.Name + "/" + columns[columnIndex].DataTypeName + " in column " + columnIndex.ToString("###,##0") + " in record " + csvReader.CurrentRecord.ToString("###,##0") + ".");
			}
			if (e is OverflowException)
			{
				throw new OverflowException("Overflow occurred while trying to convert value \"" + values[columnIndex] + "\" to type " + type.Name + "/" + columns[columnIndex].DataTypeName + " in column " + columnIndex.ToString("###,##0") + " in record " + csvReader.CurrentRecord.ToString("###,##0") + ".");
			}
		}


		public ColumnCollection Columns
		{
			get
			{
				CheckDisposed();
				return columns;
			}
			set => columns = value;
		}

		public char Comment
		{
			get => csvReader.Comment;
			set => csvReader.Comment = value;
		}

		public char Delimiter
		{
			get => csvReader.Delimiter;
			set => csvReader.Delimiter = value;
		}

		public EscapeMode EscapeMode
		{
			get => csvReader.EscapeMode;
			set => csvReader.EscapeMode = value;
		}

		public bool HasHeaders { get; set; }

		public char RecordDelimiter
		{
			get => csvReader.RecordDelimiter;
			set => csvReader.RecordDelimiter = value;
		}

		public bool SafetySwitch
		{
			get => csvReader.SafetySwitch;
			set => csvReader.SafetySwitch = value;
		}

		public bool SkipEmptyRecords
		{
			get => csvReader.SkipEmptyRecords;
			set => csvReader.SkipEmptyRecords = value;
		}

		int IDataReader.Depth => 0;

		bool IDataReader.IsClosed => disposed;

		int IDataReader.RecordsAffected => -1;

		public int FieldCount
		{
			get
			{
				CheckDisposed();
				return columns.Count;
			}
		}

		public object this[string name]
		{
			get
			{
				CheckDisposed();
				int num1 = GetOrdinal(name);
				return GetValue(num1);
			}
		}

		public object this[int i] => GetValue(i);

		public char TextQualifier
		{
			get => csvReader.TextQualifier;
			set => csvReader.TextQualifier = value;
		}

		public bool TrimWhitespace
		{
			get => csvReader.TrimWhitespace;
			set => csvReader.TrimWhitespace = value;
		}

		public bool UseComments
		{
			get => csvReader.UseComments;
			set => csvReader.UseComments = value;
		}

		public bool UseTextQualifier
		{
			get => csvReader.UseTextQualifier;
			set => csvReader.UseTextQualifier = value;
		}


		private Hashtable columnIndexByName;
		private ColumnCollection columns;
		private CsvReader csvReader;
		private bool disposed;
		private bool initialized;
		private string[] names;
		private string[] values;


		public sealed class Column
		{
			static Column()
			{
				typeMappings = new Hashtable();
				nameMappings = new Hashtable();
				dbTypeMappings = new Hashtable();
				LoadFieldTypeMappings();
				LoadNameMappings();
				LoadDbTypeMappings();
			}

			public Column(string dataType)
			{
				FieldType = typeof(string);
				DataTypeName = "";
				DbType = DbType.String;
				FormatProvider = CultureInfo.CurrentCulture;
				DefaultValue = DBNull.Value;
				format = null;
				Name = null;
				if (dataType == null)
				{
					throw new ArgumentNullException("dataType", "Data type can not be null.");
				}
				VerifyLookup(dataType);
				string text1 = dataType.ToUpper();
				FieldType = LookupTypeMapping(text1);
				DataTypeName = LookupNameMapping(text1);
				DbType = LookupDbTypeMapping(text1);
			}

			public Column(string dataType, string columnName)
				: this(dataType)
			{
				Name = columnName;
			}

			private static void LoadDbTypeMappings()
			{
				dbTypeMappings["STRING"] = DbType.String;
				dbTypeMappings["TEXT"] = DbType.String;
				dbTypeMappings["NTEXT"] = DbType.String;
				dbTypeMappings["CHAR"] = DbType.String;
				dbTypeMappings["NCHAR"] = DbType.String;
				dbTypeMappings["VARCHAR"] = DbType.String;
				dbTypeMappings["NVARCHAR"] = DbType.String;
				dbTypeMappings["XML"] = DbType.String;
				dbTypeMappings["DATETIME"] = DbType.DateTime;
				dbTypeMappings["SMALLDATETIME"] = DbType.DateTime;
				dbTypeMappings["GUID"] = DbType.Guid;
				dbTypeMappings["UNIQUE"] = DbType.Guid;
				dbTypeMappings["UNIQUEIDENTIFIER"] = DbType.Guid;
				dbTypeMappings["IDENTIFIER"] = DbType.Guid;
				dbTypeMappings["DOUBLE"] = DbType.Double;
				dbTypeMappings["FLOAT"] = DbType.Double;
				dbTypeMappings["NUMERIC"] = DbType.Decimal;
				dbTypeMappings["DECIMAL"] = DbType.Decimal;
				dbTypeMappings["MONEY"] = DbType.Decimal;
				dbTypeMappings["SMALLMONEY"] = DbType.Decimal;
				dbTypeMappings["CURRENCY"] = DbType.Decimal;
				dbTypeMappings["BOOLEAN"] = DbType.Boolean;
				dbTypeMappings["BOOL"] = DbType.Boolean;
				dbTypeMappings["BIT"] = DbType.Boolean;
				dbTypeMappings["INT"] = DbType.Int32;
				dbTypeMappings["INTEGER"] = DbType.Int32;
				dbTypeMappings["INT32"] = DbType.Int32;
				dbTypeMappings["SHORT"] = DbType.Int32;
				dbTypeMappings["LONG"] = DbType.Int64;
				dbTypeMappings["BIGINT"] = DbType.Int64;
				dbTypeMappings["INT64"] = DbType.Int64;
				dbTypeMappings["SINGLE"] = DbType.Single;
				dbTypeMappings["REAL"] = DbType.Single;
				dbTypeMappings["SMALLINT"] = DbType.Int16;
				dbTypeMappings["INT16"] = DbType.Int16;
				dbTypeMappings["TINYINT"] = DbType.Byte;
				dbTypeMappings["BYTE"] = DbType.Byte;
			}

			private static void LoadFieldTypeMappings()
			{
				typeMappings["STRING"] = typeof(string);
				typeMappings["TEXT"] = typeof(string);
				typeMappings["NTEXT"] = typeof(string);
				typeMappings["CHAR"] = typeof(string);
				typeMappings["NCHAR"] = typeof(string);
				typeMappings["VARCHAR"] = typeof(string);
				typeMappings["NVARCHAR"] = typeof(string);
				typeMappings["XML"] = typeof(string);
				typeMappings["DATETIME"] = typeof(DateTime);
				typeMappings["SMALLDATETIME"] = typeof(DateTime);
				typeMappings["GUID"] = typeof(Guid);
				typeMappings["UNIQUE"] = typeof(Guid);
				typeMappings["UNIQUEIDENTIFIER"] = typeof(Guid);
				typeMappings["IDENTIFIER"] = typeof(Guid);
				typeMappings["DOUBLE"] = typeof(double);
				typeMappings["FLOAT"] = typeof(double);
				typeMappings["NUMERIC"] = typeof(decimal);
				typeMappings["DECIMAL"] = typeof(decimal);
				typeMappings["MONEY"] = typeof(decimal);
				typeMappings["SMALLMONEY"] = typeof(decimal);
				typeMappings["CURRENCY"] = typeof(decimal);
				typeMappings["BOOLEAN"] = typeof(bool);
				typeMappings["BOOL"] = typeof(bool);
				typeMappings["BIT"] = typeof(bool);
				typeMappings["INT32"] = typeof(int);
				typeMappings["INT"] = typeof(int);
				typeMappings["INTEGER"] = typeof(int);
				typeMappings["SHORT"] = typeof(int);
				typeMappings["INT64"] = typeof(long);
				typeMappings["LONG"] = typeof(long);
				typeMappings["BIGINT"] = typeof(long);
				typeMappings["SINGLE"] = typeof(float);
				typeMappings["REAL"] = typeof(float);
				typeMappings["INT16"] = typeof(short);
				typeMappings["SMALLINT"] = typeof(short);
				typeMappings["TINYINT"] = typeof(byte);
				typeMappings["BYTE"] = typeof(byte);
			}

			private static void LoadNameMappings()
			{
				nameMappings["STRING"] = "string";
				nameMappings["TEXT"] = "text";
				nameMappings["NTEXT"] = "ntext";
				nameMappings["CHAR"] = "char";
				nameMappings["NCHAR"] = "nchar";
				nameMappings["VARCHAR"] = "varchar";
				nameMappings["NVARCHAR"] = "nvarchar";
				nameMappings["XML"] = "xml";
				nameMappings["DATETIME"] = "datetime";
				nameMappings["SMALLDATETIME"] = "smalldatetime";
				nameMappings["GUID"] = "uniqueidentifier";
				nameMappings["UNIQUE"] = "uniqueidentifier";
				nameMappings["UNIQUEIDENTIFIER"] = "uniqueidentifier";
				nameMappings["IDENTIFIER"] = "uniqueidentifier";
				nameMappings["DOUBLE"] = "double";
				nameMappings["FLOAT"] = "float";
				nameMappings["NUMERIC"] = "decimal";
				nameMappings["DECIMAL"] = "decimal";
				nameMappings["MONEY"] = "money";
				nameMappings["SMALLMONEY"] = "smallmoney";
				nameMappings["CURRENCY"] = "money";
				nameMappings["BOOLEAN"] = "bit";
				nameMappings["BOOL"] = "bit";
				nameMappings["BIT"] = "bit";
				nameMappings["INT"] = "int";
				nameMappings["INTEGER"] = "int";
				nameMappings["INT32"] = "int";
				nameMappings["SHORT"] = "short";
				nameMappings["LONG"] = "long";
				nameMappings["INT64"] = "long";
				nameMappings["BIGINT"] = "bigint";
				nameMappings["SINGLE"] = "single";
				nameMappings["REAL"] = "real";
				nameMappings["SMALLINT"] = "smallint";
				nameMappings["INT16"] = "smallint";
				nameMappings["TINYINT"] = "tinyint";
				nameMappings["BYTE"] = "byte";
			}

			private static DbType LookupDbTypeMapping(string dataType)
			{
				return (DbType)dbTypeMappings[dataType];
			}

			private static string LookupNameMapping(string dataType)
			{
				return nameMappings[dataType] as string;
			}

			private static Type LookupTypeMapping(string dataType)
			{
				return typeMappings[dataType] as Type;
			}

			private static void VerifyLookup(string dataType)
			{
				string text1 = dataType.ToUpper();
				if (!dbTypeMappings.ContainsKey(text1) || !nameMappings.ContainsKey(text1) || !typeMappings.ContainsKey(text1))
				{
					throw new ArgumentException("Could not find matching data column type for passed in type \"" + dataType + "\".", "dataType");
				}
			}


			public object DefaultValue { get; set; }

			public string Format
			{
				get => format;
				set
				{
					if ((value != null) && (value.Length > 0))
					{
						if (DbType == DbType.DateTime)
						{
							format = value;
							return;
						}
						throw new NotSupportedException("Format string is currently only supported for DateTime columns.");
					}
					format = null;
				}
			}

			public IFormatProvider FormatProvider { get; set; }

			public string Name { get; set; }


			internal readonly string DataTypeName;
			internal readonly DbType DbType;
			private static readonly Hashtable dbTypeMappings;
			internal readonly Type FieldType;
			private string format;
			private static readonly Hashtable nameMappings;
			private static readonly Hashtable typeMappings;
		}

		public sealed class ColumnCollection: CollectionBase
		{
			public void Add(Column column)
			{
				List.Add(column);
			}

			public void Add(string dataType)
			{
				List.Add(new Column(dataType));
			}

			public void Add(string dataType, string columnName)
			{
				List.Add(new Column(dataType, columnName));
			}

			public void Remove(Column column)
			{
				List.Remove(column);
			}


			public Column this[int i]
			{
				get => (Column)List[i];
				set => List[i] = value;
			}

		}

		public sealed class ReadRecordEventArgs
		{
			internal ReadRecordEventArgs(string[] Values)
			{
				SkipRecord = false;
				this.Values = Values;
			}


			public bool SkipRecord { get; set; }

			public string[] Values { get; set; }
		}

		public delegate void ReadRecordEventHandler(ReadRecordEventArgs e);

	}
}
