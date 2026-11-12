using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using Trianz.Enterprise.Operations.Models;
using System.Configuration;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class DemandTrendController : Controller
    {
        //ServiceAgent AgentService = new ServiceAgent();

        private TrianzOperationsEntities db = new TrianzOperationsEntities();

        // GET: DemandTrend
        public ActionResult Index()
        {
            return View();
        }
       
        public ActionResult DemandTrend()
        {
            //Below single statement is added by Sarath, for security reason to access RoleMaster page.
            TempData["IsRoleMasterPageAccess"] = null;

            List<HRRF> allHRRFS = new List<HRRF>();
           
            using (TrianzOperationsEntities dc = new TrianzOperationsEntities())
            {
                allHRRFS = dc.HRRFs.ToList();
            }

            var TotalDemands = (from DemandCount in db.HRRFs.AsEnumerable()
                                    
                                    select DemandCount).Count();
            var InternalDemands = (from InternalDemand in db.HRRFs.AsEnumerable()
                                    where InternalDemand.RequestStatus == ConfigurationManager.AppSettings["Internal"]
                                   select InternalDemand).Count();
            var ExternalContractDemands = (from ExternalContract in db.HRRFs.AsEnumerable()
                                    where ExternalContract.RequestStatus == ConfigurationManager.AppSettings["ExternalCon"]
                                           select ExternalContract).Count();
            var ExternalContracttoHireDemands = (from ExternalContracttoHire in db.HRRFs.AsEnumerable()
                                    where ExternalContracttoHire.RequestStatus == ConfigurationManager.AppSettings["ExternalConHire"]
                                                 select ExternalContracttoHire).Count();
            var ExternalRecruitDemands = (from ExternalRecruit in db.HRRFs.AsEnumerable()
                                          where ExternalRecruit.RequestStatus == ConfigurationManager.AppSettings["ExternalRecruit"]
                                          select ExternalRecruit).Count();
            var todaydate = DateTime.UtcNow.Year;

            // List<HRRF> MonthlyDemand = new List<HRRF>();
            List<MasterLookUp> MasterLookUps = db.MasterLookUps.ToList();

            
            var RequestStatus = from requestStatus in MasterLookUps.Where(x => x.LookupType == "RequestStatus")
                                                        select new
                                                        {
                                                            LookupName = requestStatus.LookupName
                                                            
                                                       };


            var MonthlyFullFilledDemand = from c in db.HRRFs.AsEnumerable()
                                          where todaydate == Convert.ToDateTime(c.HRRFCreatedDate).Year
                                          
                                          group c by new
                                          {

                                              months = Convert.ToDateTime(c.HRRFCreatedDate).ToString("MMM"),
                                              
                                              // month=c.HRRFSubmitedDate

                                          }
                            into d
                                          orderby d.Key.months
                                          select new
                                          {
                                              Month = d.Key.months.ToString(),
                                              

                                              // month=Convert.ToDateTime(d.Key.months).ToString("MMM"),
                                              FullFilledCount = d.Where(c => c.RequestStatus == ConfigurationManager.AppSettings["Fulfilled"]).Count(),
                                              QualifiedCount = d.Where(c => c.RequestStatus == ConfigurationManager.AppSettings["Qualified"]).Count(),
                                              SubmittedCount = d.Where(c => c.RequestStatus == ConfigurationManager.AppSettings["Submitted"]).Count(),
                                              DraftCount = d.Where(c => c.RequestStatus == ConfigurationManager.AppSettings["Draft"]).Count(),
                                          };

            List<DemandData.monthdemandtest> listmonth = new List<DemandData.monthdemandtest>();
            List<DemandData.RecordList> Listrecords = new List<DemandData.RecordList>();
            foreach (var hr in MonthlyFullFilledDemand)
            {
                DemandData.monthdemandtest objmonthdemand = new DemandData.monthdemandtest();
                
                    objmonthdemand.Month = hr.Month;
                    objmonthdemand.FullFilledRequestCount = hr.FullFilledCount;
                    objmonthdemand.SubmittedRequestCount = hr.SubmittedCount;
                    objmonthdemand.QualifiedRequestCount = hr.QualifiedCount;
                    objmonthdemand.DraftRequestCount = hr.DraftCount;

                //objmonthdemand.Request = hr.FullFilledRequest;

                listmonth.Add(objmonthdemand);
            }
            
         
            //var monthlyDemands = MonthlyDemand.ToList();
            TempData["DemandTrend"] = DemandData.GetPieChartData(TotalDemands, InternalDemands, ExternalContractDemands, ExternalContracttoHireDemands, ExternalRecruitDemands);
            return View(DemandData.GetLineAreaChartDataMonth(listmonth));
            //return View(DemandData.GetData());
        }
    }
}