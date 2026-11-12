using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.Models
{
    public class ITSResourceDashboard
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Designation { get; set; }
        public int grade { get; set; }
        public string EmployeeType { get; set; }
        public Nullable<System.DateTime> DOj { get; set; }
        public string Email { get; set; }
        public string Location { get; set; }
        public string LocationType { get; set; }
        public string Practice { get; set; }
        public string RevenuePractice { get; set; }
        public string Parentorg { get; set; }
        public string AccountName { get; set; }
        public string ProjectPractice { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string billtype { get; set; }
        public string AssignmentLocation { get; set; }
        public decimal headcount { get; set; }
        public int Utilization { get; set; }
        public string BillingStatus { get; set; }
        public Nullable<System.DateTime> rollondate { get; set; }
        public Nullable<System.DateTime> rolloffdate { get; set; }
        public string ProjectManagerName { get; set; }
        public string ProjectManageremail { get; set; }
        public string DeliveryManager { get; set; }
        public string Supervisorname { get; set; }
        public decimal NonBillableCountDays { get; set; }
        public string Ageing { get; set; }
        public decimal DurationidTrainz { get; set; }
        public string visaType { get; set; }
        public string Noticeperiod { get; set; }
        public string Competancy { get; set; }
        public string Skill { get; set; }
        public string Lastused { get; set; }
        public string Expertiselevel { get; set; }
        public string Category { get; set; }
        public string Benchstatus { get; set; }
        public string Customer { get; set; }
        public string Remarks { get; set; }
        public string Organisation { get; set; }
        public string BusinessGroup { get; set; }
        public string OpsStatus { get; set; }
        public Nullable<System.DateTime> ExpectedBillingDate { get; set; }
        public Nullable<System.DateTime> MappedDate { get; set; }
        public string ReservedProjectCode { get; set; }
        public string PrimaryPractice { get; set; }
        public string SecondaryPractice { get; set; }

        public string UniqueId { get; set; }
        public string OnsiteOROffshore { get; set; }
        public int NonBillableDays { get; set; }
        public string OrganisationGRoup { get; set; }


        public string GradeCode { get; set; }
        public string ProjectSubType { get; set; }

        public string OrgGroup { get; set; }
        public string OrgSubGroup { get; set; }


        public string Role { get; set; }
        public Nullable<System.DateTime> SOWEndDate { get; set; }
        public string SOWStatus { get; set; }
        public Nullable<System.DateTime> RelievingDate { get; set; }
        public string Skills { get; set; }
        public string CostCenter { get; set; }

        public string PracticeA { get; set; }
        //public string OrgSubGroup { get; set; }
        //public string OrgSubGroup { get; set; }

    }
}