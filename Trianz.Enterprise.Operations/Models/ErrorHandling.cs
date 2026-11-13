using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using Trianz.Enterprise.Operations.General;
using Trianz.Enterprise.Operations.Models;

namespace Trianz.Enterprise.Operations
{
    public class ErrorHandling
    {
        public long ErrorId { get; set; }
        public string Application { get; set; }
        public string Host { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
        public string User { get; set; }
        public int StatusCode { get; set; }
        public System.DateTime TimeUtc { get; set; }
        TrianzOperationsEntities Db = new TrianzOperationsEntities();

        
        public void Error(Exception ex)
        {
            Trianz.Enterprise.Operations.Models.Error_Info elc = new Trianz.Enterprise.Operations.Models.Error_Info();
            elc.Application = Environment.GetEnvironmentVariable("ÄpplicationName") ?? "";
            elc.Host = Dns.GetHostName();
            elc.Type = "Error";
            elc.Source = ex.Source != null ? ex.Source.ToString() : "";
            elc.Message = ex.Message != null ? ex.Message.ToString() : "";
            elc.StacTrace = ex.StackTrace != null ? ex.StackTrace.ToString() : "";
            //elc.User = Convert.ToString(HttpContext.Current.User.Identity.Name.Split('\\')[1]);
            elc.User = Common.GetAzureLoggedInUserID().Split('@')[0];
			elc.StatusCode = Convert.ToInt32(HttpStatusCode.BadRequest);
            elc.TimeUtc = DateTime.Now;
            Db.Error_Info.Add(elc);
            Db.SaveChanges();
        }
    }
}