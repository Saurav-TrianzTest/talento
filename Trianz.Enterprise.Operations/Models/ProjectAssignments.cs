using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    [Serializable]
    public class ProjectAssignments
    {
        public int Assignment_Id { get; set; }
        public string ProjectName { get; set; }
        public int ProjectID { get; set; }
        public Nullable<int> EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<int> Utilization { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string Assigned_By { get; set; }
        public Nullable<System.DateTime> Assigned_Date { get; set; }
        public string ProjectCode { get; set; }
        
        // additional fields
        public int ProjectManagerID { get; set; }
        public int? Utilisation { get; set; }
        public string Practice { get; set; }
        public string AssignmentStatus { get; set; }
        public string BillingType { get; set; }
        public string BillingStatus { get; set; }

        //Newly added fields
        public Nullable<System.DateTime> ReleaseDate { get; set; }
        public string ClientorProject { get; set; }
        public string ReasonForRelease { get; set; }
        public string IsDiscResourceOnProject { get; set; }
        public string SkillsGoodAt { get; set; }
        public string DetailedFeedBack { get; set; }
        public string ResourcePerformance { get; set; }
       
    }

    public class ProjectExporttoExcel
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string ProjectName { get; set; }
        public string ProjectCode { get; set; }
        public string Assigned_By { get; set; }
        public int Utilisation { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string BillingType { get; set; }
        public string BillingStatus { get; set; }
        public string Practice { get; set; }
        public bool IsActive { get; set; }
        public string AssignmentStatus { get; set; }

    }
}