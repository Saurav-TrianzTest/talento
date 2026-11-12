using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class EmployeeSkillsReport
    {
        public string ProjectCode { get; set; }
        public int EmployeeID { get; set; }
        public long? Sno { get; set; }
        public string EmployeeName { get; set; }
        public string Practice { get; set; }
        public string ParentOrganization { get; set; }
        public int Grade { get; set; }
        public string Location { get; set; }
        public string ProjectName { get; set; }
        public string BillingStatus { get; set; }
        public string Supervisor { get; set; }
        public string SKillstatus { get; set; }
        public string Competency { get; set; }
        public string Skillset { get; set; }
        public string Expertiselevel { get; set; }
        public string LastUsed { get; set; }
        public string MasterSkills { get; set; }
        public string SubSkill1 { get; set; }
        public string SubSkill2 { get; set; }
        public string SubSkill4 { get; set; }
        public string SubSkill3 { get; set; }
        public string SubSkill5 { get; set; }
        public string SecondarySkills { get; set; }
        public string SecSubSkill1 { get; set; }
        public string SecSubSkill2 { get; set; }
        public string SecSubSkill3 { get; set; }
        public string SecSubSkill4 { get; set; }
        public string SecSubSkill5 { get; set; }
        public string Status { get; set; }
        public string ResumeUploaded { get; set; }
        public string BusinessGroup { get; set; }
        public Nullable< System.DateTime> LastModifiedDate { get; set; }

    }
}