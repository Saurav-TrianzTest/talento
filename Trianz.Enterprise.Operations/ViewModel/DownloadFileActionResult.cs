using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Trianz.Enterprise.Operations.ViewModel
{
    public class DownloadFileActionResult : ActionResult 
    {
        public object ExcelGridView { get; set; }
        public string fileName { get; set; }


        public DownloadFileActionResult(object gv, string pFileName)
        {
            ExcelGridView = gv;
            fileName = pFileName;
        }


        public override void ExecuteResult(ControllerContext context)
        {
            //Create a response stream to create and write the Excel file
            HttpContext curContext = context.HttpContext;
            curContext.Response.Clear();
            curContext.Response.AddHeader("content-disposition", "attachment;filename=" + fileName);
            curContext.Response.Charset = "";
            curContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            curContext.Response.ContentType = "application/vnd.ms-excel";

            //Convert the rendering of the gridview to a string representation 
            StringBuilder sb = new StringBuilder();
            sb.Append("<table>");
            // Build HTML table from data source
            if (ExcelGridView is IEnumerable<object>)
            {
                foreach (var item in (IEnumerable<object>)ExcelGridView)
                {
                    sb.Append("<tr>");
                    var props = item.GetType().GetProperties();
                    foreach (var prop in props)
                    {
                        sb.AppendFormat("<td>{0}</td>", HttpUtility.HtmlEncode(prop.GetValue(item)));
                    }
                    sb.Append("</tr>");
                }
            }
            sb.Append("</table>");

            //Write the stream back to the response
            curContext.Response.Write(sb.ToString());
            curContext.Response.End();

        }
    }
}
