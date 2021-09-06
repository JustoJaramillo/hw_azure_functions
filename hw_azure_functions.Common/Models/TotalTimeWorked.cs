using System;

namespace hw_azure_functions.Common.Models
{
    public class TotalTimeWorked
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public int MinutesWorked { get; set; }
    }
}
