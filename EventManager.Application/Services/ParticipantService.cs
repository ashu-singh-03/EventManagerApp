using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using EventManager.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq; // Add this
using System.Threading.Tasks; // Add this
using System.Text;
using EventManager.Application.Utilities;

namespace EventManager.Application.Services
{
    public class ParticipantService : IParticipantService
    {
        private readonly IParticipantRepository _repository;
        private readonly ExcelHelper _excelHelper;
        private readonly ILogger<ParticipantService> _logger;
        public ParticipantService(IParticipantRepository repository, ExcelHelper excelHelper, ILogger<ParticipantService> logger)
        {
            _repository = repository;
            _excelHelper = excelHelper;
            _logger = logger;
        }

        public async Task<IEnumerable<ParticipantDto>> GetParticipantsByEventAsync(int eventId)
        {
            var participants = await _repository.GetParticipantsByEventAsync(eventId);
            return participants.Select(p => new ParticipantDto
            {
                ParticipantId = p.ParticipantId,
                EventId = p.EventId,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Email = p.Email,
                Phone = p.Phone,
                Company = p.Company,
                Department = p.Department,
                Notes = p.Notes,
                participants_code= p.participants_code,
            }).ToList();

        }


        public async Task<ParticipantDto> GetParticipantByIdAsync(int participantId)
        {
            var participant = await _repository.GetParticipantByIdAsync(participantId);
            if (participant == null) return null;

            return new ParticipantDto
            {
                ParticipantId = participant.ParticipantId,
                EventId = participant.EventId,
                FirstName = participant.FirstName,
                LastName = participant.LastName,
                Email = participant.Email,
                Phone = participant.Phone,
                Company = participant.Company,
                Department = participant.Department,
                Notes = participant.Notes
            };
        }


        public async Task SaveParticipantAsync(ParticipantDto dto)
        {
            var participant = new Participant
            {
                ParticipantId = dto.ParticipantId,
                EventId = dto.EventId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                Company = dto.Company,
                Department = dto.Department,
                Notes = dto.Notes,
                QrCodeHash = Guid.NewGuid().ToString()
            };

            await _repository.SaveParticipantAsync(participant);
        }

        public async Task DeleteParticipantAsync(int participantId)
        {
            await _repository.DeleteParticipantAsync(participantId);
        }
        public async Task<ImportResult> ImportParticipantsFromExcelAsync(
            IFormFile excelFile,
            int eventId,
            string createdBy)
        {
            var result = new ImportResult();
            string tempFilePath = "";
            string errorFilePath = "";

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

            try
            {
                // 1. Validate file
                if (!_excelHelper.ValidateExcelFile(excelFile))
                {
                    result.Message = "Invalid file. Only .xlsx or .xls files up to 10MB allowed.";
                    result.TotalRecords = 0;
                    result.FailedRecords = 0;
                    _logger.LogWarning($"File validation failed: {result.Message}");
                    return result;
                }

                // 2. Save file
                tempFilePath = await _excelHelper.SaveExcelFile(excelFile, uploadsFolder);
                _logger.LogInformation($"File saved to: {tempFilePath}");

                // 3. Read Excel
                var dtImport = _excelHelper.ReadExcel(tempFilePath, 0);
                _logger.LogInformation($"Excel read: {dtImport?.Rows.Count ?? 0} rows found");

                if (dtImport == null || dtImport.Rows.Count == 0)
                {
                    result.Message = "Excel file is empty.";
                    result.TotalRecords = 0;
                    result.FailedRecords = 0;
                    _logger.LogWarning($"Empty Excel file: {result.Message}");
                    return result;
                }

                // 4. Process Excel data
                var processedData = ProcessExcelData(dtImport, eventId, createdBy);
                int originalRowCount = dtImport.Rows.Count;
                int processedRowCount = processedData.Rows.Count;

                _logger.LogInformation($"Processing: Original={originalRowCount}, Processed={processedRowCount}");

                result.TotalRecords = originalRowCount;

                if (processedRowCount == 0)
                {
                    result.IsSuccess = false;
                    result.FailedRecords = originalRowCount;
                    result.Message = $"No valid data found in Excel. {originalRowCount} rows failed processing.";
                    _logger.LogWarning($"No valid data: {result.Message}");
                    return result;
                }

                // 5. Delete existing temp data
                await _repository.DeleteTempParticipantsAsync(eventId, createdBy);

                // 6. Bulk insert
                await _repository.BulkInsertToTempTableAsync(processedData);
                _logger.LogInformation($"Inserted {processedRowCount} rows to temp table");

                // 7. Validate
                var validationErrors = await _repository.ValidateTempParticipantsAsync(eventId, createdBy);
                _logger.LogInformation($"Validation: {validationErrors?.Rows.Count ?? 0} errors found");

                if (validationErrors?.Rows.Count == 0)
                {
                    var importedCount = await _repository.ImportTempToMainAsync(eventId, createdBy);

                    result.IsSuccess = true;
                    result.ImportedRecords = importedCount;
                    result.Message = $"Successfully imported {importedCount} out of {originalRowCount} participants.";

                    _logger.LogInformation($"SUCCESS: {result.Message}");
                }
                else
                {
                    errorFilePath = _excelHelper.GenerateErrorExcel(validationErrors, tempFilePath);
                    _logger.LogInformation($"Error Excel generated at: {errorFilePath}");

                    result.IsSuccess = false;
                    result.FailedRecords = validationErrors.Rows.Count;
                    result.TotalRecords = processedRowCount; // Valid rows that were processed
                    result.ErrorFilePath = errorFilePath;

                    // Better error message
                    int successfulRows = processedRowCount - validationErrors.Rows.Count;
                    if (successfulRows > 0)
                    {
                        result.Message = $"{validationErrors.Rows.Count} validation errors found. {successfulRows} rows imported successfully. Please check error file.";
                    }
                    else
                    {
                        result.Message = $"{validationErrors.Rows.Count} validation errors found. All rows failed validation. Please check error file.";
                    }

                    _logger.LogWarning($"VALIDATION ERRORS: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing participants from Excel");
                result.Message = $"Import failed: {ex.Message}";
                result.TotalRecords = 0;
                result.FailedRecords = 0;
            }
            finally
            {
                // Always delete the temporary uploaded file
                if (!string.IsNullOrEmpty(tempFilePath) && File.Exists(tempFilePath))
                {
                    _excelHelper.DeleteFile(tempFilePath);
                    _logger.LogInformation($"Deleted temp file: {tempFilePath}");
                }

                // ONLY delete error file if import was SUCCESSFUL
                // If failed, we need to keep it for download
                if (result.IsSuccess && !string.IsNullOrEmpty(errorFilePath) && File.Exists(errorFilePath))
                {
                    _excelHelper.DeleteFile(errorFilePath);
                    _logger.LogInformation($"Deleted error file: {errorFilePath}");
                }
            }

            // FINAL DEBUG LOG
            _logger.LogInformation($"FINAL RESULT: IsSuccess={result.IsSuccess}, Message='{result.Message}', TotalRecords={result.TotalRecords}, FailedRecords={result.FailedRecords}, ErrorFilePath={(string.IsNullOrEmpty(result.ErrorFilePath) ? "null" : result.ErrorFilePath)}");

            return result;
        }
        private DataTable ProcessExcelData(DataTable dtImport, int eventId, string createdBy)
        {
            _logger.LogInformation($"ProcessExcelData started with {dtImport.Rows.Count} rows");

            // Clean column names (convert to lowercase with underscores)
            foreach (DataColumn col in dtImport.Columns)
            {
                string originalName = col.ColumnName;
                string cleanName = CleanColumnName(originalName);
                _logger.LogInformation($"Column renamed: '{originalName}' -> '{cleanName}'");
                col.ColumnName = cleanName;
            }

            // Add required columns if missing
            if (!dtImport.Columns.Contains("event_id"))
            {
                dtImport.Columns.Add("event_id", typeof(int));
                _logger.LogInformation("Added 'event_id' column");
                foreach (DataRow row in dtImport.Rows)
                    row["event_id"] = eventId;
            }

            if (!dtImport.Columns.Contains("created_by"))
            {
                dtImport.Columns.Add("created_by", typeof(string));
                _logger.LogInformation("Added 'created_by' column");
                foreach (DataRow row in dtImport.Rows)
                    row["created_by"] = createdBy;
            }

            string[] requiredColumns = { "first_name", "last_name", "email", "phone", "company", "department", "notes" };
            foreach (var column in requiredColumns)
            {
                if (!dtImport.Columns.Contains(column))
                {
                    dtImport.Columns.Add(column, typeof(string));
                    _logger.LogInformation($"Added missing column: '{column}'");
                }
            }

            _logger.LogInformation($"ProcessExcelData completed. Output rows: {dtImport.Rows.Count}");
            return dtImport;
        }
        private string CleanColumnName(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                return "column";

            return columnName
                .ToLower()
                .Replace(" ", "_")
                .Replace(".", "")
                .Replace("-", "_")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("/", "_");
        }
    }
}