using EventManager.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace EventManager.WebUI.Controllers
{
    public class QrConfigurationController : Controller
    {
        private static List<ScanLogViewModel> _scanLog = new List<ScanLogViewModel>();
        private static int _scanCounter = 1;
        private static int _totalScans = 0;
        private static int _validScans = 0;
        private static int _invalidScans = 0;
        private static int _duplicateScans = 0;
        private readonly IEventClaimService _eventClaimService;

        public QrConfigurationController(IEventClaimService eventClaimService)
        {
            _eventClaimService = eventClaimService;

        }
        [HttpGet]
        public IActionResult Index()
        {
            int eventId = _eventClaimService.GetEventIdFromClaim();
            if (eventId == 0) return BadRequest("Invalid event");

            // Set the EventId in ViewBag
            ViewBag.EventId = eventId;

            return View();
        }


        public IActionResult GetScanLog()
        {
            // If no data, add sample data
            if (_scanLog.Count == 0)
            {
                _scanLog = GetSampleScans();
            }

            return PartialView("_ScanLogPartial", _scanLog);
        }

        [HttpPost]
        public JsonResult ProcessScan([FromBody] ScanRequest request)
        {
            try
            {
                _totalScans++;

                // Generate random ticket data
                var random = new Random();
                var names = new[] { "John Smith", "Sarah Johnson", "Mike Brown", "Emma Wilson", "David Lee" };
                var events = new[] { "Summer Fest 2024", "Tech Conference", "Music Festival" };
                var statuses = new[] { "valid", "invalid", "duplicate" };

                var status = random.Next(0, 10) > 2 ? "valid" : (random.Next(0, 5) > 2 ? "invalid" : "duplicate");

                // Update stats
                if (status == "valid") _validScans++;
                else if (status == "invalid") _invalidScans++;
                else _duplicateScans++;

                // Create scan log entry
                var scan = new ScanLogViewModel
                {
                    Id = _scanCounter++,
                    TicketId = "#TKT-" + random.Next(100000, 999999),
                    HolderName = names[random.Next(names.Length)],
                    ScanTime = DateTime.Now.ToString("HH:mm:ss"),
                    Status = status,
                    AccessPoint = request.AccessPoint ?? "Main Entrance"
                };

                // Add to beginning of list
                _scanLog.Insert(0, scan);

                // Keep only last 50 scans
                if (_scanLog.Count > 50)
                {
                    _scanLog = _scanLog.Take(50).ToList();
                }

                return Json(new
                {
                    success = true,
                    ticketId = scan.TicketId,
                    holderName = scan.HolderName,
                    status = scan.Status,
                    scanTime = scan.ScanTime,
                    accessPoint = scan.AccessPoint
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public JsonResult GetStats()
        {
            return Json(new
            {
                totalScans = _totalScans,
                validScans = _validScans,
                invalidScans = _invalidScans,
                duplicateScans = _duplicateScans
            });
        }

        private List<ScanLogViewModel> GetSampleScans()
        {
            return new List<ScanLogViewModel>
            {
                new ScanLogViewModel
                {
                    Id = _scanCounter++,
                    TicketId = "#TKT-123456",
                    HolderName = "John Smith",
                    ScanTime = DateTime.Now.AddMinutes(-30).ToString("HH:mm:ss"),
                    Status = "valid",
                    AccessPoint = "Main Entrance"
                },
                new ScanLogViewModel
                {
                    Id = _scanCounter++,
                    TicketId = "#TKT-789012",
                    HolderName = "Sarah Johnson",
                    ScanTime = DateTime.Now.AddMinutes(-25).ToString("HH:mm:ss"),
                    Status = "valid",
                    AccessPoint = "VIP Lounge"
                },
                new ScanLogViewModel
                {
                    Id = _scanCounter++,
                    TicketId = "#TKT-345678",
                    HolderName = "Mike Brown",
                    ScanTime = DateTime.Now.AddMinutes(-15).ToString("HH:mm:ss"),
                    Status = "invalid",
                    AccessPoint = "Main Entrance"
                },
                new ScanLogViewModel
                {
                    Id = _scanCounter++,
                    TicketId = "#TKT-901234",
                    HolderName = "Emma Wilson",
                    ScanTime = DateTime.Now.AddMinutes(-10).ToString("HH:mm:ss"),
                    Status = "valid",
                    AccessPoint = "Food Counter"
                },
                new ScanLogViewModel
                {
                    Id = _scanCounter++,
                    TicketId = "#TKT-567890",
                    HolderName = "David Lee",
                    ScanTime = DateTime.Now.AddMinutes(-5).ToString("HH:mm:ss"),
                    Status = "duplicate",
                    AccessPoint = "Backstage"
                }
            };
        }
    }

    public class ScanLogViewModel
    {
        public int Id { get; set; }
        public string TicketId { get; set; }
        public string HolderName { get; set; }
        public string ScanTime { get; set; }
        public string Status { get; set; }
        public string AccessPoint { get; set; }
    }

    public class ScanRequest
    {
        public string QrCode { get; set; }
        public string AccessPoint { get; set; }
    }
}