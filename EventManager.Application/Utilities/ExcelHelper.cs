using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System.Data;

namespace EventManager.Application.Utilities
{
    public class ExcelHelper
    {
        private readonly ILogger<ExcelHelper> _logger;

        public ExcelHelper(ILogger<ExcelHelper> logger)
        {
            _logger = logger;
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<string> SaveExcelFile(IFormFile file, string uploadsFolder)
        {
            try
            {
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Excel file");
                throw;
            }
        }

        public DataTable ReadExcel(string filePath, int sheetIndex = 0)
        {
            try
            {
                DataTable dt = new DataTable("ExcelData");

                FileInfo fileInfo = new FileInfo(filePath);

                if (!fileInfo.Exists || fileInfo.Length == 0)
                {
                    _logger.LogWarning($"File does not exist or is empty: {filePath}");
                    return dt;
                }

                using (var package = new ExcelPackage(fileInfo))
                {
                    if (sheetIndex >= package.Workbook.Worksheets.Count)
                    {
                        _logger.LogWarning($"Sheet index {sheetIndex} not found. Total sheets: {package.Workbook.Worksheets.Count}");
                        return dt;
                    }

                    var worksheet = package.Workbook.Worksheets[sheetIndex];

                    if (worksheet.Dimension == null)
                    {
                        _logger.LogWarning($"Worksheet {worksheet.Name} has no data (Dimension is null)");
                        return dt;
                    }

                    _logger.LogInformation($"Reading Excel: {worksheet.Name}, Range: {worksheet.Dimension.Address}");

                    // Add columns from first row
                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        string columnName = worksheet.Cells[1, col].Text?.Trim();
                        if (string.IsNullOrEmpty(columnName))
                            columnName = $"Column{col}";

                        dt.Columns.Add(columnName, typeof(string));
                    }

                    // Add rows (always add; no hasAnyData check)
                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        DataRow dataRow = dt.NewRow();

                        for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                        {
                            string cellValue = worksheet.Cells[row, col].Text?.Trim();
                            dataRow[col - 1] = cellValue ?? "";
                        }

                        dt.Rows.Add(dataRow);   // ← FIXED
                    }

                    _logger.LogInformation($"Excel read complete. Found {dt.Rows.Count} rows.");
                    return dt;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reading Excel file: {filePath}");
                return new DataTable();
            }
        }
        public string GenerateErrorExcel(DataTable errorData, string originalFilePath)
        {
            try
            {
                _logger.LogInformation($"Generating error Excel with {errorData.Rows.Count} error rows");

                var directory = Path.GetDirectoryName(originalFilePath);
                var fileName = Path.GetFileNameWithoutExtension(originalFilePath);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var errorFilePath = Path.Combine(directory, $"{fileName}_errors_{timestamp}.xlsx");

                _logger.LogInformation($"Creating error file at: {errorFilePath}");

                // Ensure directory exists
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogInformation($"Created directory: {directory}");
                }

                using (var package = new ExcelPackage())
                {
                    var workSheet = package.Workbook.Worksheets.Add("Errors");

                    // Add header
                    workSheet.Cells["A1"].Value = "Error Report - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    workSheet.Cells["A1:D1"].Merge = true;
                    workSheet.Cells["A1"].Style.Font.Bold = true;
                    workSheet.Cells["A1"].Style.Font.Size = 14;
                    workSheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    // Load data starting from row 3 (row 2 is empty for spacing)
                    workSheet.Cells["A3"].LoadFromDataTable(errorData, true);

                    // Auto-fit columns for better readability
                    workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();

                    // Save the file
                    package.SaveAs(new FileInfo(errorFilePath));
                }

                _logger.LogInformation($"Error Excel successfully generated: {errorFilePath}");

                // Verify file was created
                if (File.Exists(errorFilePath))
                {
                    var fileInfo = new FileInfo(errorFilePath);
                    _logger.LogInformation($"Error file size: {fileInfo.Length} bytes");
                    return errorFilePath;
                }
                else
                {
                    _logger.LogError($"Error file was not created: {errorFilePath}");
                    throw new Exception($"Failed to create error file at: {errorFilePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating error Excel");
                throw new Exception($"Failed to generate error Excel: {ex.Message}", ex);
            }
        }
        public void DeleteFile(string filePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete file: {FilePath}", filePath);
            }
        }

        public bool ValidateExcelFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var allowedExtensions = new[] { ".xlsx", ".xls" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
                return false;

            return file.Length <= (10 * 1024 * 1024); // 10MB
        }
    }
}