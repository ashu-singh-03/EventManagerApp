using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.DTOs
{
    public class AccessPointDto
    {
        public int AccessPointId { get; set; }
        public int? EventId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; } = true;
    }
}
