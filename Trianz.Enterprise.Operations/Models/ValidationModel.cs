using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;



namespace Trianz.Enterprise.Operations.Models
{
    public class ValidationModel
    {
        
        // editview models
        public List<TRHistory> TRHistorys { get; set; }
        public IEnumerable<ProposeAssociate> ProposeAssociates { get; set; }
        public IEnumerable<HRRF> HRRFs { get; set; }
        public IEnumerable<Controllers.HRRFData> HRRFData { get; set; }
        public IEnumerable<Controllers.SkillData> HRRFExpertiseLevel { get; set; }
        public IEnumerable<HRRFSkillsDetails> HRRFSkills_Expertise { get; set; }
        public IEnumerable<EmployeeSkills_NewDetails> EmployeeSkillInfo { get; set; }
        public IEnumerable<Controllers.hrrfdetails> HRRFDet { get; set; }
        public IEnumerable<TRMagicData> TRMagicDetails { get; set; }
        public IEnumerable<ProposedResourcesList> PropsedList { get; set; }
        public IEnumerable<ExternalFulfillment> ExternalFulfillments { get; set; }
        public IEnumerable<MasterLookUp> MasterLookUps { get; set; }
        public List<HRRFSkill> HRRFSkills { get; set; }
        public IEnumerable<EmpoloyeeAssigmentDetails> EMPProjectAssignmentDetails { get; set; }



        public IEnumerable<ExternalHire> ExternalHireDetails { get; set; }
        //public List<ExternalVM> ExternalHireDetail { get; set; }



        //custom View Models 
        public List<BenchEmployees> BenchEmployee { get; set; }
        public List<TRMagicData> TRMagicLogic { get; set; }
        public List<HRRF> HRRFParent { get; set; }
        public HRRF HRRFParentChild { get; set; }
        public List<HRRF> HRRFChild { get; set; } // will uncomment once i have method ready for HRRF model binding with hrrfskills
        //public List<HRRFSkills> HRRFSkillss { get; set; }
        public List<TRHistory> TRHistoryss { get; set; }



        public List<ProjectAssignments> ProjectAssignments { get; set; }




        public List<Controllers.ExtFulfillment> ExternalFulfullmentHireDetails { get; set; }



        public List<Controllers.ExtFulfillment> ExternalhiredetailsRemarks { get; set; }
        public List<ITSResourceDashboard> ITSDashboard { get; set; }



        public Dictionary<string, List<ITDashBoardDetailsSummary>> ITDashBoardDetails { get; set; }



        public Dictionary<string, List<ITDashBoardDetailsSummary>> PreviousWeekDashBoardDetails { get; set; }
        public Dictionary<string, List<ITDashBoardDetailsSummary>> CurrentWeekDashBoardDetails { get; set; }
        public Dictionary<string, List<ITDashBoardDetailsSummary>> NextWeekITDashBoardDetails { get; set; }
        public List<ClusterLead> ClusterLeadlst { get; set; }
        public List<EmployeeLocation> EmployeeLocationlst { get; set; }



        public List<SkillMaster> SkillDetails { get; set; }



        public List<EmployeeSkills_NewDetails> EmployeeSkills_NewDetailslst { get; set; }
        public List<SkillMasterinfo> lstskills { set; get; }
        public List<EmployeeSkills_NewDetails> lstselectskills { set; get; }

      
        public List<EmployeeInfoDetails> EmpSkillInfo { get; set; }

        public List<Employee> Empsearch { get; set; }

        public IList<ProjectAssignment> ProjectAssignmentinfo { get; set; }
        public List<Training> Traininginfo { get; set; }

        //public List<TR_Remakrinfo> TR_Remakrinfo { get; set; }

        public List<HRRFRemarksinfo> HRRFRemarksinfo { get; set; }

        public List<HRRFStatusHistoryDetails> HRRFStatusHistoryDetails { get; set; }

        public List<EmployeeDesignation> EmployeeDesignationlst { get; set; }



        public List<EmployeeCountry> EmployeeCountrieslst { get; set; }

        public List<RecruiterLedDetails> RecruiterLedlst { get; set; }

        //public List<ProfilesInformation> ProfilesDataList { get; set; }


    }
    public class EmpoloyeeAssigmentDetails
    {
        public string ProjectName { get; set; }
        public Nullable<int> EmployeeId { get; set; }
        public string Name { get; set; }
        public string startdate { get; set; }
        public string Enddate { get; set; }
        public Nullable<int> Utilization { get; set; }
        public string Status { get; set; }
        public string Assigned_By { get; set; }
        public string ProjectCode { get; set; }
        public string BillingStatus { get; set; }
        public string ProjectManager { get; set; }
        public string DeliveryManager { get; set; }
        public string Category { get; set; }
    }
    public class HRRFSkillsDetails
    {
        public int SkillId { get; set; }
        public string IsMandatoy { get; set; }
        public string ExpertiseLevel { get; set; }
        public string SkillCategory { get; set; }
        public string Skillset { get; set; }



    }




}