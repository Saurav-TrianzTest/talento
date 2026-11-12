using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Trianz.Enterprise.Operations.Filters;
using Trianz.Enterprise.Operations.Models;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Trianz.Enterprise.Operations.General;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class BillinglossController : Controller
    {
        TrianzOperationsEntities db = new TrianzOperationsEntities();
        // GET: Billingloss
        public ActionResult Index()
        {
            return View();         
        }
   
        public ActionResult GetHRRFsByFilters(string strreqstatus, string strCrtticality, DateTime? fromdate, DateTime? todate)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
            try
            {
                BillingLossList listofdata = new BillingLossList();
                DateTime? Fromdate = null;
                DateTime? Todate = null;
                if (fromdate != null)
                {
                    Fromdate = Convert.ToDateTime(fromdate);
                }
                if (todate != null)
                {
                    Todate = Convert.ToDateTime(todate);
                }
                int? EmpId = Convert.ToInt32(Session["EmployeeId"]);
               // strRequest = strRequest != null ? (strRequest.ToLower() == "myrequest") ? "" : strRequest.ToUpper() : string.Empty;
                    var lstHRRF = db.Database.SqlQuery<sp_GetBillingsLossList_Result>("exec [dbo].[sp_GetBillingsLossList]  @Criticality, @FromDate, @ToDate,@RequestStatus",          
                    new System.Data.SqlClient.SqlParameter("Criticality", strCrtticality),
                    new System.Data.SqlClient.SqlParameter("FromDate", Fromdate ?? (object)DBNull.Value),
                    new System.Data.SqlClient.SqlParameter("ToDate", Todate ?? (object)DBNull.Value),
                    new System.Data.SqlClient.SqlParameter("RequestStatus", strreqstatus)).ToList<sp_GetBillingsLossList_Result>();

                    List<BillingLoss> billinglosslist = new List<BillingLoss>();              
                    DateTime date2 = DateTime.UtcNow;
                    decimal? sumofaccountstotal = 0;
                    decimal? sumofpractisetotal =0;
                    
                    var practicedata = lstHRRF.GroupBy(x => x.Practice).Select(x => x.ToList()).ToList();
                    decimal? sumofpracleakagerevenue = 0;
                    List<BillingLoss> practiselistdata = new List<BillingLoss>();
                    foreach (var category in practicedata)
                    {
                        BillingLoss data = new BillingLoss();
                        data.Practice = category[0].Practice;                    
                        foreach (var item in category)
                        {
                            var x = item.LeakageRevenue;
                            if (x != null)
                            {
                                sumofpracleakagerevenue = sumofpracleakagerevenue + x ;                            
                            }                           
                        }
                        data.LeakageRevenue = sumofpracleakagerevenue;
                        sumofpracleakagerevenue = 0 ;
                       practiselistdata.Add(data);
                    }
                    sumofpractisetotal = practiselistdata.Sum(item => item.LeakageRevenue);
                    //var practice = lstHRRF.GroupBy(x => x.Practice).Select(x => x.FirstOrDefault()).ToList();

                    //List<BillingLoss> practiselist = new List<BillingLoss>();
                    //foreach (var item in practice)
                    //{
                    //    BillingLoss data = new BillingLoss();
                    //    data.Practice = item.Practice;
                    //    data.LeakageRevenue = item.LeakageRevenue;
                    //    practiselist.Add(data);
                    //}
                    var accounttypes = lstHRRF.GroupBy(x => x.AccountName).Select(x => x.ToList()).ToList();
                    decimal? sumofaccountsleakagerevenue = 0;
                    List<BillingLoss> accountslistdata = new List<BillingLoss>();
                    foreach (var account in accounttypes)
                    {
                        BillingLoss dataacounts = new BillingLoss();
                        dataacounts.AccountName = account[0].AccountName;
                        foreach (var item in account)
                        {
                            var x = item.LeakageRevenue;
                            if (x != null)
                            {
                                sumofaccountsleakagerevenue = sumofaccountsleakagerevenue + x;
                            }
                        }
                        dataacounts.LeakageRevenue = sumofaccountsleakagerevenue;
                        sumofaccountsleakagerevenue = 0;
                        accountslistdata.Add(dataacounts);
                    }

                    sumofaccountstotal = accountslistdata.Sum(x => x.LeakageRevenue);
                    //if (accounttypes != null)
                    //{
                    //    sumofaccountstotal = accounttypes.Where(item => item.LeakageRevenue != null).Sum(item => item.LeakageRevenue);
                    //}
                    //List<BillingLoss> accountlist = new List<BillingLoss>();
                    //foreach (var item in accounttypes)
                    //{
                    //    BillingLoss data = new BillingLoss();
                    //    data.AccountName = item.AccountName;
                    //    data.LeakageRevenue = item.LeakageRevenue;
                    //    accountlist.Add(data);
                    //}
                    foreach (var item in lstHRRF)
                    {
                    BillingLoss data = new BillingLoss();
                    if (item.BillingDate != null)
                    {
                     date2 = item.BillingDate ?? item.BillingDate.Value;
                    }
                   data.HRRFID = item.HRRFID;
                   data.HRRFNumber=item.HRRFNumber;
                   data.Practice = item.Practice;
                   data.ProjectName=item.ProjectName;
                   data.AccountName=item.AccountName;
                   data.RequestStatus=item.RequestStatus;
                   data.BillingDate=item.BillingDate ;
                   data.BILLRATE=item.BILLRATE;
                   data.Criticality=item.Criticality;
                   data.LeakageDays=item.LeakageDays;      
                   data.LeakageRevenue=item.LeakageRevenue;
                   data.CurrentDate = DateTime.UtcNow;
                   billinglosslist.Add(data);
              }
                    
                  
                    var js = new BillingLossList();
                    js.BillingLossData = billinglosslist;
                    js.PractiseList = practiselistdata;
                    js.AccountNameList = accountslistdata;
                    js.CountPractiseList = sumofpractisetotal;
                    js.CountAccountNameList = sumofaccountstotal;
                return PartialView("_BillingLossShowDetails", js);
            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
          }

            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }

       
    }
}