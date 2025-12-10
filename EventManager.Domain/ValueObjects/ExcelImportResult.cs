using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace EventManager.Domain.ValueObjects
{
    public class ExcelImportResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public DataTable ValidatedData { get; set; }
        public int RecordsProcessed { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
