using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace hw_azure_functions.Functions.Entities
{
    public class TotalTimeWorkedEntity : TableEntity
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public int MinutesWorked { get; set; }
    }
}
