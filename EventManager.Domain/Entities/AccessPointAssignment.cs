using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Domain.Entities
{
    public class AccessPointAssignment
    {
        public int AccessPointId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public bool IsAssigned { get; set; }
    }
}
