using System;
using System.Collections.Generic;
using System.Linq;



namespace Trianz.Enterprise.Operations.Models
{
    public class EmployeeSkills_NewDetails
    {
        public int EmployeeSkillId { get; set; }
        public int EmployeeId { get; set; }
        public int SkillId { get; set; }
        public string Skillname { get; set; }
        public string CompetincyName { get; set; }
        public string Expertiselevel { get; set; }
        public string TechnologyLastUsed { get; set; }
        public string TechnologyLastUsedmonth { get; set; }
        public string TechnologyLastUsedyear { get; set; }
        public string SkillStatus { get; set; }
        public string ApproverName { get; set; }
        public Nullable<int> ApproverId { get; set; }
        public Nullable<System.DateTime> ApprovalDate { get; set; }
        public System.DateTime LastModifiedDate { get; set; }
        public string EmployeeSkillHistrory { get; set; }
        public string EMployeeName { get; set; }
        public string Remarks { get; set; }
        public Nullable<bool> IsPrimary { get; set; }
        public Nullable<bool> IsReviewed { get; set; }
        public int LeadID { get; set; }
        public Nullable<bool> Reviewed { get; set; }
        public Nullable<System.DateTime> ApprovalSubmissionDate { get; set; }
    }
}