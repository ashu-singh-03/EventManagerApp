using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Linq;

namespace EventManager.WebUI.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly IParticipantService _service;
        private readonly IEventClaimService _eventClaimService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<ParticipantController> _logger;

        public ParticipantController(
            IParticipantService service,
            IEventClaimService eventClaimService,
            IWebHostEnvironment hostingEnvironment,
            ILogger<ParticipantController> logger)
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
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error downloading template: {ex.Message}");
            }
        }

        [HttpPost]
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

                // Get the uploads folder path from WebHostEnvironment
                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads");

                // Ensure folder exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                    _logger.LogInformation($"Created uploads folder: {uploadsFolder}");
                }

                // Pass the uploads folder path to the service
                var result = await _service.ImportParticipantsFromExcelAsync(file, eventId, createdBy, uploadsFolder);

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
                    // Get the filename only, not the full path
                    string errorFileUrl = null;
                    if (!string.IsNullOrEmpty(result.ErrorFilePath))
                    {
                        var fileName = Path.GetFileName(result.ErrorFilePath);
                        // FIX: Include "Participant" controller name in the URL
                        errorFileUrl = Url.Action("DownloadErrorFile", "Participant",
                            new { fileName = fileName }, Request.Scheme);

                        // Log for debugging
                        _logger.LogInformation($"Generated download URL: {errorFileUrl}");
                    }

                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message,
                        totalRecords = result.TotalRecords,
                        failedRecords = result.FailedRecords,
                        errorFileUrl = errorFileUrl
                    });
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error importing Excel");
                return StatusCode(500, new { success = false, message = $"Import failed: {ex.Message}" });
            }
        }

        [HttpGet("DownloadErrorFile")]
        public IActionResult DownloadErrorFile([FromQuery] string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    return NotFound("Error file not specified.");

                // Get the uploads folder path
                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads");
                var filePath = Path.Combine(uploadsFolder, fileName);

                if (!System.IO.File.Exists(filePath))
                    return NotFound($"Error file not found: {fileName}");

                var fileBytes = System.IO.File.ReadAllBytes(filePath);

                // Delete the error file after downloading
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning(ex, "Could not delete error file after download");
                }

                return File(fileBytes,
                           "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                           fileName);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error downloading error file");
                return BadRequest(new { success = false, message = "Unable to download error file" });
            }
        }
    }
}