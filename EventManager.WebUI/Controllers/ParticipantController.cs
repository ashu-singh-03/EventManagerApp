using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EventManager.WebUI.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly IParticipantService _service;
        private readonly IEventClaimService _eventClaimService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<ParticipantController> _logger;

        public ParticipantController(IParticipantService service, IEventClaimService eventClaimService, IWebHostEnvironment hostingEnvironment, ILogger<ParticipantController> logger)
        {
            _service = service;
            _eventClaimService = eventClaimService;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        // LOAD PAGE (HTML)
        public async Task<IActionResult> Index()
        {
            int eventId = _eventClaimService.GetEventIdFromClaim();
            if (eventId == 0) return BadRequest("EventId claim not set.");

            var participants = await _service.GetParticipantsByEventAsync(eventId);
            return View(participants);
        }

        // RETURN PARTICIPANTS AS JSON (for table AJAX)
        [HttpGet]
        public async Task<IActionResult> LoadParticipants()
        {
            int eventId = _eventClaimService.GetEventIdFromClaim();
            if (eventId == 0) return BadRequest("EventId claim not set.");

            var participants = await _service.GetParticipantsByEventAsync(eventId);
            return Json(participants);
        }

        // GET ONE PARTICIPANT
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var participant = await _service.GetParticipantByIdAsync(id);
            if (participant == null)
                return NotFound();

            return Json(participant);
        }

        // SAVE (INSERT/UPDATE)
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] ParticipantDto dto)
        {
            dto.EventId = _eventClaimService.GetEventIdFromClaim();
            if (dto.EventId <= 0)
                return BadRequest(new { error = "EventId must be greater than 0" });

            if (!ModelState.IsValid)
            {
                // Extract validation errors and return them in a consistent format
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    message = "Validation failed",
                    errors = errors
                });
            }

            await _service.SaveParticipantAsync(dto);

            return Ok(new { message = "Participant saved successfully" });
        }

        // DELETE
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteParticipantAsync(id);

            return Ok(new { message = "Deleted successfully" });
        }

        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            try
            {
                // Path to your Excel template file
                var filePath = Path.Combine(_hostingEnvironment.WebRootPath,
                                           "ExcelTemplates",
                                           "Attendee_Template.xlsx");

                // Check if file exists
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Template file not found. Please place Attendee_Template.xlsx in wwwroot/ExcelTemplates folder.");
                }

                // Read the file
                var fileBytes = System.IO.File.ReadAllBytes(filePath);

                // Return file for download
                return File(fileBytes,
                           "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                           "Attendee_Template.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error downloading template: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ImportExcel([FromForm] IFormFile file)
        {
            try
            {
                string createdBy = User.Identity?.Name ?? "System";

                // Get eventId from claims instead of form parameter
                var eventId = _eventClaimService.GetEventIdFromClaim();

                if (eventId <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid event or no event selected." });
                }

                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "No file uploaded" });

                var result = await _service.ImportParticipantsFromExcelAsync(file, eventId, createdBy);

                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        totalRecords = result.TotalRecords,
                        importedRecords = result.ImportedRecords
                    });
                }
                else
                {
                    // Always include errorFileUrl (empty if no file)
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message,
                        totalRecords = result.TotalRecords,
                        failedRecords = result.FailedRecords,
                        errorFileUrl = !string.IsNullOrEmpty(result.ErrorFilePath)
                            ? $"/api/participants/download-error?filePath={Uri.EscapeDataString(result.ErrorFilePath)}"
                            : null
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing Excel");
                return StatusCode(500, new { success = false, message = $"Import failed: {ex.Message}" });
            }
        }

        [HttpGet("download-error")]
        [Authorize]
        public IActionResult DownloadErrorFile([FromQuery] string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return NotFound("Error file not specified.");

                // Decode the file path (it was URL encoded)
                filePath = Uri.UnescapeDataString(filePath);

                if (!System.IO.File.Exists(filePath))
                    return NotFound("Error file not found or already downloaded.");

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var fileName = Path.GetFileName(filePath);

                // Delete the error file after downloading
                System.IO.File.Delete(filePath);

                return File(fileBytes,
                           "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                           fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading error file");
                return BadRequest(new { success = false, message = "Unable to download error file" });
            }
        }

        //[HttpGet("download-template")]
        //public IActionResult DownloadTemplate()
        //{
        //    try
        //    {
        //        // Create template in memory
        //        using var memoryStream = new MemoryStream();
        //        using (var package = new OfficeOpenXml.ExcelPackage(memoryStream))
        //        {
        //            var worksheet = package.Workbook.Worksheets.Add("Participants");

        //            // Add headers
        //            worksheet.Cells[1, 1].Value = "First Name";
        //            worksheet.Cells[1, 2].Value = "Last Name";
        //            worksheet.Cells[1, 3].Value = "Email";
        //            worksheet.Cells[1, 4].Value = "Phone";
        //            worksheet.Cells[1, 5].Value = "Company";
        //            worksheet.Cells[1, 6].Value = "Department";
        //            worksheet.Cells[1, 7].Value = "Notes";

        //            // Format headers
        //            for (int i = 1; i <= 7; i++)
        //            {
        //                worksheet.Cells[1, i].Style.Font.Bold = true;
        //            }

        //            package.Save();
        //        }

        //        memoryStream.Position = 0;
        //        return File(memoryStream,
        //                   "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //                   "Participant_Template.xlsx");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error creating template");
        //        return StatusCode(500, new { success = false, message = "Error creating template" });
        //    }
        //}
    }
}