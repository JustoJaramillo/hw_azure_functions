using System;

namespace hw_azure_functions.Common.Models
{
    public class WorkingHoursEntry
    {
        public int EmployeeId { get; set; }
        public DateTime RecordDate { get; set; }
        public bool RecordType { get; set; }
        public bool Consolidated { get; set; }
    }
}
