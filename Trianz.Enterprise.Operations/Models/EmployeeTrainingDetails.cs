using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class EmployeeTrainingDetails
    {
        public String ProgramName { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public int EmployeeID { get; set; }
        public string Location { get; set; }
        public string Practice { get; set; }
        public string AttendanceStatus { get; set; }
        public string TotalHours { get; set; }
        public string Source { get; set; }
        public string Category { get; set; }
        public string Mode { get; set; }

    }
}