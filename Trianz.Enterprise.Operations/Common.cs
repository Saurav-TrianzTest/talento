using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using System.Threading.Tasks;
using Trianz.Enterprise.Operations.Models;
using System.Web.Mvc;
using System.IO;
using System.Data.SqlClient;
using System.Net;
using System.Security.Principal;

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
				TrianzOperationsEntities _db = new TrianzOperationsEntities();
               
                string UName = HttpContext.Current?.User?.Identity?.Name ?? "";
                if (UName.IndexOf("\\") > 0)
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
            //EmpID = 103553;
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
            //int EmpID = 0;
			TrianzOperationsEntities _db = new TrianzOperationsEntities();
			string eMail = "";
			try
			{
				var userClaims = (HttpContext.Current?.User?.Identity as System.Security.Claims.ClaimsIdentity);
				string EmpName = userClaims?.FindFirst("name")?.Value;
				Common.WriteErrorLog("name : " + EmpName);
				eMail = userClaims?.FindFirst("preferred_username")?.Value;
				Common.WriteErrorLog("email : " + eMail);
                //var empLoggedin = (from emp in _db.Employees where emp.Email == eMail select emp).FirstOrDefault();
                //EmpID = empLoggedin.EmployeeId;
                //return Json("error", JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
			{
				Common.WriteErrorLog(ex.Message + ex.StackTrace);
			}
            //EmpID = 103553;

            //eMail = "TSathishkumar.K@trianz.com";
            return eMail;// EmpID;
            //return View("error");
            //
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
                sw.WriteLine(DateTime.UtcNow.ToString() + ": " + ex.Source.ToString().Trim() + "; " + ex.Message.ToString().Trim());
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
                sw.WriteLine(DateTime.UtcNow.ToString() + ": " + Message);
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
        //public static void WriteErrorLog(string Message)
        //{
        //    StreamWriter sw = null;
        //    try
        //    {
        //        sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\Audit.txt", true);
        //        sw.WriteLine(DateTime.UtcNow.ToString() + ": " + Message);
        //        sw.Flush();
        //        sw.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        string message =
        //      "EXCEPTION TYPE:" + ex.GetType() + Environment.NewLine +
        //      "EXCEPTION MESSAGE: " + ex.Message + Environment.NewLine +
        //      "STACK TRACE: " + ex.StackTrace + Environment.NewLine;
        //        System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(ex, true);
        //        Common.WriteErrorLog("Line: " + trace.GetFrame(0).GetFileLineNumber());
        //        if (ex.InnerException != null)
        //        {
        //            message += "---BEGIN InnerException--- " + Environment.NewLine +
        //                       "Exception type " + ex.InnerException.GetType() + Environment.NewLine +
        //                       "Exception message: " + ex.InnerException.Message + Environment.NewLine +
        //                       "Stack trace: " + ex.InnerException.StackTrace + Environment.NewLine +
        //                       "---END Inner Exception";
        //        }
        //        Common.WriteErrorLog(message);
        //    }
        //}


    }

    public static byte[] DownloadFromS3(string key)
    {
        try
        {
            var s3Client = new AmazonS3Client();
            var bucketName = System.Configuration.ConfigurationManager.AppSettings["AWS:S3:Bucket"] ?? "TrianzEnterpriseOperations";
            var response = s3Client.GetObject(new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            });
            using (var memoryStream = new MemoryStream())
            {
                response.ResponseStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public static Stream DownloadFromS3Stream(string key)
    {
        var s3Client = new AmazonS3Client();
        var bucketName = System.Configuration.ConfigurationManager.AppSettings["AWS:S3:Bucket"] ?? "TrianzEnterpriseOperations";
        var response = s3Client.GetObject(new GetObjectRequest
        {
            BucketName = bucketName,
            Key = key
        });
        return response.ResponseStream;
    }

    public static string GetS3TempFilePath(string key)
    {
        var bytes = DownloadFromS3(key);
        if (bytes == null) return null;
        var tempPath = Path.Combine(Path.GetTempPath(), key.Replace("/", "_"));
        File.WriteAllBytes(tempPath, bytes);
        return tempPath;
    }
}