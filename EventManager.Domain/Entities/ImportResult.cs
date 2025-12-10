using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Domain.Entities
{
    public class ImportResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int TotalRecords { get; set; }
        public int ImportedRecords { get; set; }
        public int FailedRecords { get; set; }
        public string ErrorFilePath { get; set; }
    }
}
