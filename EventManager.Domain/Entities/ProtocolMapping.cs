using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Domain.Entities
{
    public class ProtocolMapping
    {
        public int? Id { get; set; }
        public int ProjectId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ImportFilePath { get; set; }
        public string ErrorFilePath { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
    }

}
