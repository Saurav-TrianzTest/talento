using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trianz.Enterprise.Operations.Models
{
    public class UiConstants
    {
        static string strGetLookupService = System.Configuration.ConfigurationManager.AppSettings["GetLookupService"].ToString();
        static string strGetEmployeeService = System.Configuration.ConfigurationManager.AppSettings["GetEmployeeService"].ToString();
        static string strEmployeeURL = System.Configuration.ConfigurationManager.AppSettings["EmployeeURL"].ToString();
        static string strNumberingCliURL = System.Configuration.ConfigurationManager.AppSettings["NumberingCliURL"].ToString();
        static string strHRRFCliUrl = System.Configuration.ConfigurationManager.AppSettings["HRRFCliUrl"].ToString();
        static string strHRRFNumberingCliUrl = System.Configuration.ConfigurationManager.AppSettings["HRRFNumberingCliUrl"].ToString();
        static string strNotificationCliUrl = System.Configuration.ConfigurationManager.AppSettings["NotificationCliUrl"].ToString();
        static string strGetOpportunityByNameorCode = System.Configuration.ConfigurationManager.AppSettings["GetOpportunityByNameorCode"].ToString();



        public static string GetMasterLookupData = "Enterprise/Lookup/{lookuptype}";

        public static string GetLookupService = strGetLookupService;
        public static string GetAllLookUpMethods = "/Enterprise/Lookup";
        public static string GetBasedOnLookupType = "/Enterprise/Lookup/{LOOKUPTYPE}";
        public static string GetApplicationSetting = "/Enterprise/Application/Setting";
        public static string GetApplicationIdSetting = "/Enterprise/Application/{APPLICATION}/Setting";
        public static string GetEmployeeService = strGetEmployeeService;//"http://172.16.0.217/EnterpriseServices/EmployeeService/EmployeeService.svc";
        public static string GetGetEmployees = "/Enterprise/Employee";
        public static string GetEmployeeByEmail = "/Enterprise/Employee/Email/{EMAILID}";
        public static string GetEmployeeByID = "/Enterprise/Employee/{ID}";
        public static string EmployeeURL = strEmployeeURL;//"http://172.16.0.217/EnterpriseServices/EmployeeService/EmployeeService.svc/enterprise/employee/101148";
        public static string NumberingCliURL = strNumberingCliURL;//"http://172.16.0.217/EnterpriseServices/NumberingService/NumberingService.svc";
        public static string HRRFCliUrl = strHRRFCliUrl;//"http://172.16.0.217/EnterpriseServices/HRRFService/HRRFService.svc";
       // public static string HRRFCliUrl = "http://localhost:27902/HRRFService.svc";
        public static string GetHRRFDetails = "/Enterprise/HRRF";
        public static string GetHRRFByHRRFNumber = "/Enterprise/HRRF/{HRRFNo}";
        public static string InsertHRRFDetails = "/Enterprise/HRRFs";
        public static string UpdateHRRFDetails = "/Enterprise/UpdateHRRF";

        public static string HRRFNumberingCliUrl = strHRRFNumberingCliUrl;//"http://172.16.0.217/EnterpriseServices/NumberingService/NumberingService.svc";
        public static string HRRFNumberingCode = "/Enterprise/NumberingSystem?CompanyCode={CompanyCode}&BranchCode={BranchCode}&TypeCode={TypeCode}";

        public static string NotificationCliUrl = strNotificationCliUrl;// "http://172.16.0.217/EnterpriseServices/NotificationService/NotificationService.svc";
        public static string GetNotifications = "/Enterprise/Notification";
        public static string GetNotificationByID = "/Enterprise/Notification/{id}";
        public static string GetNotificationsPendingForApproval = "/Enterprise/Notification/Approval/{managerid}";
        public static string GetPendingNotificationsByApplication = "Enterprise/Notification/Application/?Manager={managerid}&Application={applicationCode}";

        public static string GetOpportunityByNameorCode = strGetOpportunityByNameorCode;//"https://crm.trianz.com/";
    }
}