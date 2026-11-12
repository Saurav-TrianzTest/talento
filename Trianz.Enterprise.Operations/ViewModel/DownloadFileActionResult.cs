using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Trianz.Enterprise.Operations.ViewModel
{
    public class DownloadFileActionResult : ActionResult
    {
        public string HtmlContent { get; set; }
        public string fileName { get; set; }

        public DownloadFileActionResult(string htmlContent, string pFileName)
        {
            HtmlContent = htmlContent;
            fileName = pFileName;
        }

        public override void ExecuteResult(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.Clear();
            response.Headers.Add("content-disposition", "attachment;filename=" + fileName);
            response.ContentType = "application/vnd.ms-excel";

            byte[] byteArray = Encoding.ASCII.GetBytes(HtmlContent);
            response.Body.Write(byteArray, 0, byteArray.Length);
        }
    }
}
