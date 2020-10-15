# Data.Csv

## Installation

```
Install-Package Winspels.Data.Csv -Version 1.0.0
```

## Samples

**Reading from CSV file**

  
```
  private DataTable GetDataFromCsv(Stream dataStream)
  {
    DataTable data;
    using (var streamReader = new StreamReader(dataStream))
    {
      using (var reader = new CsvReader(streamReader, ','))
      {
        data = reader.ReadToEnd(false);
      }
    }

    return data;
  }
```
