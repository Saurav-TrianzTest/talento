using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class EmployeeInfoDetails
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }

        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string JobName { get; set; }
     
        public string LocationType { get; set; }
        public string Location { get; set; }
        public string ReservationStatus { get; set; }
        public string AssignmentStatus { get; set; }

        public string EmployeeType { get; set; }
        public string Designation { get; set; }

        public string PrimarySkills { get; set; }

        public string ProjectName { get; set; }
        public string ProjectCode { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }

        public Nullable<System.DateTime> EndDate { get; set; }
        public string BillingType { get; set; }

        public string Skills { get; set; }

        public string SkillName { get; set; }
        //public byte[] Resumes { get; set; }

        public int ResumeFlag { get; set; }
        public int SkillsCount { get; set; }

        public int Utilization { get; set; }
        public string BillingStatus { get; set; }
        public string Grade { get; set; }
        public decimal Percentage { get; set; }


        public string AccountName { get; set; }




    }
}