using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class ProjectResourceUtilizationReport
    {
        public string EmployeeName { get; set; }
        public  int   EmployeeId { get; set; }
        public DateTime ActualStartDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CurrentDate { get; set; }
        public int TotalWorkingDays { get; set; }
        public int TotalWorkingHours { get; set; }
        public Decimal Utilization { get; set; }

        public bool IsActive { get; set; }
        public int ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public int HeadCount { get; set; }
        //public int TotalHours { get; set; }
    }


    //public class ProjectResourceUtilizationReport
    //{
    //    public int ProjectCode { get; set; }
    //    public string ProjectName { get; set; }
    //    public DateTime StartDate { get; set; }
    //    public DateTime EndDate { get; set; }
    //    public int HeadCount { get; set; }
    //    public int TotalHours { get; set; }

    //    //public int TotalWorkingDays { get; set; }
    //    //public int TotalWorkingHours { get; set; }
    //    //public Decimal Utilization { get; set; }

    //    //public bool IsActive { get; set; }
    //}
}
