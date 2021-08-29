using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace hw_azure_functions.Functions.Entities
{
    class TotalTimeWorkedEntity : TableEntity
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public int MinutesWorked { get; set; }
    }
}
