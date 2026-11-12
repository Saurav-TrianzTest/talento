using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class ClusterLead
    {
        public int SKILLCLSTRID { get; set; }
        [Required(ErrorMessage = "Skill Code is Requirde")]
        [Display(Name = "Skill Code")]
        public string SKILLCODE { get; set; }
        public int PRTCSKILLID { get; set; }
        public string SKILLCLUSTER { get; set; }
        [Display(Name = "Cluster Skill")]
        public string CSKILLS { get; set; }
        public string EmployeeName { get; set; }
        public string ClusterLeadName { get; set; }
        public Int32 EmployeeId { get; set; }
        public string PracticeName { get; set; }
        public string ClusterName { set; get; }
        public string ClusterLeadEmailId { set; get; }
        public int TeamMemberID { get; set; }
        public string TeamMemberName { get; set; }
        public string Role { get; set; }
        public string TeamMemberEmailID { get; set; }
        public string ProjectName { get; set; }
        public string ProjectCode { get; set; }
        public string AccountName { get; set; }
        public string Style { get; set; }
        public string SupervisorName { get; set; }
        public Int64 PracticeId { get; set; }
        public Int64 ClusterId { get; set; }

        public string PrimaryCluster { get; set; }
       public Int64 ClusterMemberCount { get; set; }
        public Int64 ClusterLeadCount { get; set; }
        public Int64 PracticeMemberCount { get; set; }
        public Int64 PracticeLeadCount { get; set; }

        public string SkillSet { get; set; }

        public string SkillIds { get; set; }

        public int ReviewFlag { get; set; }
        public int IsResume { get; set; }

    }
}