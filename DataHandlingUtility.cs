using ClosedXML.Excel;
using System.Data;

namespace SixtyLibrary
{
    public static class DataHandlingUtility
    {
        /// <summary>
        /// Only works with xlsx files.
        /// Test requires.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="sheetName"></param>
        public static (DataTable? dataTable, string? errorMessage) DataTableFromExcel(string filePath, string sheetName, bool isFirstRowHeader)
        {
            // Check if file exists
            if (!File.Exists(filePath))
            {
                return (null, $"File '{filePath}' not found.");
            }

            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(sheetName);

                    // Check if Sheet exists
                    if (worksheet == null)
                    {
                        return (null, $"Sheet '{sheetName}' not found in the workbook.");
                    }

                    DataTable dataTable = new DataTable();
                    var rows = worksheet.RowsUsed();

                    if (isFirstRowHeader)
                    {
                        var headerRow = rows.First();
                        foreach (var cell in headerRow.Cells())
                        {
                            // Use cell value as column name. If null, assign a default name
                            dataTable.Columns.Add(cell.Value.ToString() ?? $"Column{cell.Address.ColumnNumber}");
                        }

                        // Skip the header row for data rows
                        rows = (IXLRows)rows.Skip(1);
                    }
                    else
                    {
                        // If no header, create columns with default names
                        var firstDataRow = rows.First();
                        foreach (var cell in firstDataRow.Cells())
                        {
                            dataTable.Columns.Add($"Column{cell.Address.ColumnNumber}");
                        }
                    }

                    // Populate data rows
                    foreach (var row in rows)
                    {
                        var dataRow = dataTable.NewRow();
                        int columnIndex = 0;
                        foreach (var cell in row.Cells())
                        {
                            dataRow[columnIndex++] = cell.Value.ToString();
                        }
                        dataTable.Rows.Add(dataRow);
                    }
                    // Success
                    return (dataTable, null);
                }
            }
            catch (Exception ex)
            {
                return (null, $"Error: {ex.Message}");
            }
        }
    }
}