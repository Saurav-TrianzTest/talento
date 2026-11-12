using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations.ViewModel
{
    public class BenchResourceViewModel
    {
        public int Assignment_ID { get; set; }
        public int EmployeeId { get; set; }
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public string Practice { get; set; }
        public string PracticeA { get; set; }
        public string Resumestatus { get; set; }
        public string SPOC { get; set; }
        public string PotentialAccount { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string LocationType { get; set; }
        public string Location { get; set; }
        public string AssignmentStatus { get; set; }
        public string Comments { get; set; }
        public bool IsActive { get; set; }
        public string ChangedBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Utilization { get; set; }
        public int? Rating { get; set; }
        public string NonBillableCountDays { get; set; }
        public string ProjectManager { get; set; }
        public string Supervisor { get; set; }
        public string SuperviserName { get; set; }
        public string Ageing { get; set; }
        public int? Grade { get; set; }
        public string Customer { get; set; }
        public string Category { get; set; }
        public string Bechstatus { get; set; }
        public string Remarks { get; set; }
        public DateTime? ExpectedBillingDate { get; set; }
        public DateTime? Assigned_Date { get; set; }
        public DateTime? MappedDate { get; set; }
        public DateTime? ActionReminder { get; set; }
        public string HRRFNumber { get; set; }
        public string SkillCategory { get; set; }
        public string SkillSet { get; set; }
        public string PARENT_ORG { get; set; }
        public string ReservedProjectCode { get; set; }
        public string BusinessGroup { get; set; }
        public decimal Headcount { get; set; }
        public DateTime? DateOfJoin { get; set; }
        public string BenchSkills { get; set; }
        public string UpdatedBy { get; set; }
        public string PPTResumestatus { set; get; }
        public Nullable<System.DateTime> ResumePdfUpdatedDate { get; set; }
        public Nullable<System.DateTime> ResumeUpdatedDate { get; set; }
      
        public string ReservedProjCode { get; set; }
        public string ReservedProjName { get; set; }

          public string ReservedOppCode { set; get; }
          public string ReservedProjCodeName { get; set; }
          public string ReservedOppName { set; get; }

        //public string ReservedOppName { get; set; }


    }


    public class BenchResourceDataViewModel
    {
        public int Assignment_ID { get; set; }
        public int EmployeeId { get; set; }
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public string Practice { get; set; }
        public string SPOC { get; set; }
        public string PotentialAccount { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string LocationType { get; set; }
        public string Location { get; set; }
        public string AssignmentStatus { get; set; }
        public string Comments { get; set; }
        public bool IsActive { get; set; }
        public string ChangedBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Utilization { get; set; }
        public int? Rating { get; set; }
        public string NonBillableCountDays { get; set; }
        public string ProjectManager { get; set; }
        public string Supervisor { get; set; }
        public string SuperviserName { get; set; }
        public string Ageing { get; set; }
        public int? Grade { get; set; }
        public string Customer { get; set; }
        public string Category { get; set; }
        public string Bechstatus { get; set; }
        public string Remarks { get; set; }
        public DateTime? ExpectedBillingDate { get; set; }
        public DateTime? MappedDate { get; set; }
        public DateTime? ActionReminder { get; set; }
        public string HRRFNumber { get; set; }
        public string SkillCategory { get; set; }
        public string SkillSet { get; set; }
        public string PARENT_ORG { get; set; }
        public string ReservedProjectCode { get; set; }
        public string BusinessGroup { get; set; }
        public decimal Headcount { get; set; }
        public DateTime? DateOfJoin { get; set; }
        public string BenchSkills { get; set; }

    }
    
    public class HRRFReportDetails
    {
        public long HRRFID { get; set; }
        public string HRRFNumber { get; set; }
        public string BU { get; set; }
        public string PracticeA { set; get; }
        public string LocationType { set; get; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> Ageing { get; set; }
        public string AgeingBucket { get; set; }
        public string Purpose { get; set; }
        public string BillingStatus { get; set; }
        public string ProjectNameWithCode { get; set; }
        public string OpportunityCode { get; set; }
        public string OpportunityName { get; set; }
        public string AccountName { get; set; }
        public Nullable<System.DateTime> HRRFSubmitedDate { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public Nullable<System.DateTime> AssignmentStartDate { get; set; }
        public string UpdatedBy { get; set; }
        public string TRRemarks { set; get; }
        public string RequestType { get; set; }
        public string Recruitername { get; set; }
        public string PrimarySkillSet { get; set; }
        public string JobDescription { get; set; }
        public Nullable<int> Grade { get; set; }
        public string RequestStatus { get; set; }
        public string FulfillmentRemarks { get; set; }
        public string CurrentStatus { get; set; }
        public string LocationName { get; set; }
        public string TALeadName { get; set; }
        public string ResourceName { get; set; }
        public string DemandType { get; set; }
        public string RequestReason { get; set; }
        public string RoleRequired { get; set; }
        public Nullable<int> HRRFCreatedById { get; set; }
        public string HRRFCreatedByName { get; set; }
        public Nullable<System.DateTime> DOJ { get; set; }
        public string FirstLevelTechnicalPanel { get; set; }
        public Nullable<System.DateTime> BillingDate { get; set; }
        public string Remarks { get; set; }
        public Nullable<int> ReplacementEmpID { get; set; }
        public string ReplacementName { get; set; }
        public string Skill2 { get; set; }
        public string Impact { get; set; }
        public string Criticality { get; set; }
        public string SkillCluster { get; set; }
        public string CSkill { get; set; }
        public string SkillCode { get; set; }
        public string Practice { get; set; }
        public Nullable<System.DateTime> DHApprovaerDate { get; set; }
        public string ClientInterview { get; set; }
        public string TECHPANEL { get; set; }
        public string SECONDTECHPANEL { get; set; }
        public string Stage { get; set; }
        public string TRCandidate { get; set; }
        public string RoleGroup { get; set; }
        public string Certifications { get; set; }
        public Nullable<decimal> Billrate { get; set; }
        public Nullable<double> Maxsal { get; set; }
        public string OrganizationGroup { get; set; }
        public string ProjectName { get; set; }
        public string ProjectCode { get; set; }
        public string Talead { get; set; }
        public string Rectr { get; set; }
        public string BoSpoc { get; set; }
        public string OrganizationSubGroup { get; set; }
        public string ReasonRaisingTR { get; set; }
        public string Raisedby { set; get; }
        public string ValidatedBy { get; set; }
        public string TRDOJ { get; set; }
        public string EmploymentType { set; get; }
        
    }

    public class ProjectReleaseDetails
    {


        public int Assignment_ID { get; set; }
        public int EmployeeId { get; set; }
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public string Practice { get; set; }
         
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string LocationType { get; set; }
        public string Location { get; set; }
        public string AssignmentStatus { get; set; }
        public string Comments { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Utilization { get; set; }
       
      
        public string ProjectManager { get; set; }
        public string Supervisor { get; set; }
        public string SuperviserName { get; set; }
       
        public int? Grade { get; set; }
        public string Designation { get; set; }
        public string PracticeA { get; set; }

        public Nullable<System.DateTime> ReleaseDate { get; set; }
        public string ClientorProject { get; set; }
        public string ReasonForRelease { get; set; }
        public string SkillsGoodAt { get; set; }
        public string DetailedFeedBack { get; set; }
        public string ResourcePerformance { get; set; }
        public Nullable<int> ReleasedBy { get; set; }
        public Nullable<System.DateTime> ReleaseAuditDate { get; set; }
        public string IsDiscResourceOnProject { get; set; }
        public string OrganisationGroup { get; set; }
        public string OrganisationSubGroup { get; set; }
        public string Skills { get; set; }
    }

    public class FutureReleasesData
    {
        public int EmployeeId { get; set; }
        public string OrganizationGroup { get; set; }
        public string ProjectName { get; set; }
        public string PracticeA { set; get; }
        public string LocationType { set; get; }
        public int Grade { set; get; }
        public string SOWStatus { set; get; }
        public string EmployeeName { set; get; }
        public string Practice { set; get; }
        public string ProjectCode { set; get; }
        public string DeliveryManager { set; get; }
        public string ProjectManager { set; get; }
        public string Supervisor { set; get; }
        public string ResumeupdatedDate { set; get; }
        public int ResumeFlag { set; get; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Skills { get; set; }
       
        public DateTime RollOffDate { set; get; }
        public string OrganizationSubGroup { get; set; }       
    }
}