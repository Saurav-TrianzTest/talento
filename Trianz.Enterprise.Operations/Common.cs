using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Trianz.Enterprise.Operations.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Security.Principal;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Trianz.Enterprise.Operations.General
{
    public class Common
    {
        #region Get LoggedIn User
        /// <summary>
        /// this function Gets Logged In User Details.
        /// </summary>
        /// <param name="Message"></param>
        public static int GetLoggedInUserID()
        {
            int EmpID = 0;
            try
            {
                var options = new DbContextOptionsBuilder<TrianzOperationsEntities>()
                    .UseSqlServer(ConfigurationManager.ConnectionStrings["TrianzOperationsEntities"].ConnectionString)
                    .Options;
                TrianzOperationsEntities _db = new TrianzOperationsEntities(options);

                string UName = ClaimsPrincipal.Current?.Identity?.Name;
                if (UName != null && UName.IndexOf("\\") > 0)
                {
                    UName = UName.Substring(7);
                }
                var EmployeeID = (from emp in _db.Employees
                                  where emp.UserName.ToLower() == UName.ToLower() && emp.IsActive == true
                                  select new
                                  {
                                      EmployeeId = emp.EmployeeId
                                  }).FirstOrDefault();
                if (EmployeeID != null)
                {
                    EmpID = Convert.ToInt32(EmployeeID.EmployeeId);
                }
                else
                {
                    Common.WriteErrorLog("User not found in Employee Table" + UName);
                }

            }
            catch (Exception ex)
            {
                Common.WriteErrorLog(ex.Message + ex.StackTrace);
            }
            return EmpID;
        }
        #endregion

        #region Get AzureLoggedIn User
        /// <summary>
        /// this function Gets Azure Logged In User Details.
        /// </summary>
        /// <param name="Message"></param>
        public static string GetAzureLoggedInUserID()
        {
            TrianzOperationsEntities _db = new TrianzOperationsEntities(new DbContextOptions<TrianzOperationsEntities>());
            string eMail = "";
            try
            {
                var userClaims = ClaimsPrincipal.Current?.Identity as ClaimsIdentity;
                string EmpName = userClaims?.FindFirst("name")?.Value;
                Common.WriteErrorLog("name : " + EmpName);
                eMail = userClaims?.FindFirst("preferred_username")?.Value;
                Common.WriteErrorLog("email : " + eMail);
            }
            catch (Exception ex)
            {
                Common.WriteErrorLog(ex.Message + ex.StackTrace);
            }
            return eMail;
        }
        #endregion

        #region Log Error
        /// <summary>
        /// this function write message to Error Log.
        /// </summary>
        /// <param name="Message"></param>
        public static void WriteErrorLog(Exception ex)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\TalentoLog.txt", true);
                sw.WriteLine(DateTime.Now.ToString() + ": " + ex.Source.ToString().Trim() + "; " + ex.Message.ToString().Trim());
                sw.Flush();
                sw.Close();
            }
            catch (Exception e)
            {
                Common.WriteErrorLog(e.Message + e.StackTrace);
            }
        }
        #endregion

        #region Log Error
        /// <summary>
        /// this function write Message to log file.
        /// </summary>
        /// <param name="Message"></param>
        public static void WriteErrorLog(string Message)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\TalentoLog.txt", true);
                sw.WriteLine(DateTime.Now.ToString() + ": " + Message);
                sw.Flush();
                sw.Close();
            }
            catch (Exception ex)
            {
                string message =
             "EXCEPTION TYPE:" + ex.GetType() + Environment.NewLine +
             "EXCEPTION MESSAGE: " + ex.Message + Environment.NewLine +
             "STACK TRACE: " + ex.StackTrace + Environment.NewLine;
                System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(ex, true);
                Common.WriteErrorLog("Line: " + trace.GetFrame(0).GetFileLineNumber());
                if (ex.InnerException != null)
                {
                    message += "---BEGIN InnerException--- " + Environment.NewLine +
                               "Exception type " + ex.InnerException.GetType() + Environment.NewLine +
                               "Exception message: " + ex.InnerException.Message + Environment.NewLine +
                               "Stack trace: " + ex.InnerException.StackTrace + Environment.NewLine +
                               "---END Inner Exception";
                }
                Common.WriteErrorLog(message);
            }
        }
        #endregion

        public static void WriteExceptionErrorLog(Exception exception)
        {
            try
            {
                string message =
                            "EXCEPTION TYPE:" + exception.GetType() + Environment.NewLine +
                            "EXCEPTION MESSAGE: " + exception.Message + Environment.NewLine +
                            "STACK TRACE: " + exception.StackTrace + Environment.NewLine;
                System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(exception, true);
                Common.WriteErrorLog("Line: " + trace.GetFrame(0).GetFileLineNumber());
                if (exception.InnerException != null)
                {
                    message += "---BEGIN InnerException--- " + Environment.NewLine +
                               "Exception type " + exception.InnerException.GetType() + Environment.NewLine +
                               "Exception message: " + exception.InnerException.Message + Environment.NewLine +
                               "Stack trace: " + exception.InnerException.StackTrace + Environment.NewLine +
                               "---END Inner Exception";
                }
                Common.WriteErrorLog(message);
            }
            catch (Exception ex)
            {
                Common.WriteErrorLog(ex.ToString());
            }
        }
    }
}
