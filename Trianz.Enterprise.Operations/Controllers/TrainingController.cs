using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Trianz.Enterprise.Operations.Models;
using Trianz.Enterprise.Operations.ViewModel;
using Microsoft.Extensions.Hosting;

namespace Trianz.Enterprise.Operations.Controllers
{

    public class TrainingController : Controller
    {
    private readonly IWebHostEnvironment Env;
        TrianzOperationsEntities db = new TrianzOperationsEntities();
        // GET: Training
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult UploadTrainingDocument()
        {

            try
            {


                return View();

            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                Exception raise = dbEx;
                // EF Core DbUpdateException does not have EntityValidationErrors - skip validation error iteration
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(raise);
                throw raise;
            }
        }







        //public ActionResult InsertTrainingData(IFormCollection form, IFormFile file)
        //{

        //    ValidateUploadTemplate validateUploadTemplate = new ValidateUploadTemplate();
        //    string isUpdatedReport = "false";
        //    string isValidTemplate = "true";
        //    DataTable uploadReportDT = new DataTable();
        //    DataSet ds = new DataSet();
        //    try
        //    {
        //        if (HttpContext.Request.Form.Files["file"].Length > 0)
        //        {
        //            string fileExtension =
        //                                 System.IO.Path.GetExtension(HttpContext.Request.Form.Files["file"].FileName);

        //            if (fileExtension == ".xls" || fileExtension == ".xlsx")
        //            {
        //                //string fileLocation = Path.Combine(Env.ContentRootPath, "Content/") + HttpContext.Request.Form.Files["file"].FileName;
        //                string fileLocation = Path.Combine(Env.ContentRootPath, "Content/") + string.Format(@"{0}.xlsx","Training"+DateTime.Now.Ticks);
        //                if (System.IO.File.Exists(fileLocation))
        //                {

        //                    System.IO.File.Delete(fileLocation);

        //                }
        //                using (var stream = System.IO.File.Create(fileLocation))
        //                { HttpContext.Request.Form.Files["file"].CopyTo(stream); }

        //                var File = new FileInfo(fileLocation);


        //                using (ExcelPackage package = new ExcelPackage(File))
        //                {
        //                    System.IO.File.Delete(fileLocation);
        //                    ExcelWorkbook workBook = package.Workbook;
        //                    if (workBook != null)
        //                    {
        //                        if (workBook.Worksheets.Count > 0)
        //                        {
        //                            ExcelWorksheet workSheet = workBook.Worksheets.First();

        //                            uploadReportDT = ToDataTable(workSheet);
        //                            ViewBag.exceldata = uploadReportDT;


        //                        }

        //                    }

        //                }

        //            }
        //            bool validtemplate = true;


        //            if (uploadReportDT.Columns.Count >= 18)
        //            {

        //                if ("PROGRAMNAME" != uploadReportDT.Columns[0].ColumnName.ToString().Trim().ToUpper())
        //                {
        //                    validtemplate = false;

        //                }
        //                if ("STARTDATE" != uploadReportDT.Columns[1].ColumnName.ToString().Trim().ToUpper())
        //                {
        //                    validtemplate = false;

        //                }
        //                if ("ENDDATE" != uploadReportDT.Columns[2].ColumnName.ToString().Trim().ToUpper())
        //                {
        //                    validtemplate = false;

        //                }
        //                if ("EMP.CODE" != uploadReportDT.Columns[3].ColumnName.ToString().Trim().ToUpper())
        //                {
        //                    validtemplate = false;

        //                }
        //                if ("PARTICIPANTNAME" != uploadReportDT.Columns[4].ColumnName.ToString().Trim().ToUpper())
        //                {
        //                    validtemplate = false;

        //                }
        //                if ("LOCATION" != uploadReportDT.Columns[5].ColumnName.ToString().Trim().ToUpper())
        //                {
        //                    validtemplate = false;

        //                }
        //                if ("GRADE" != uploadReportDT.Columns[6].ColumnName.ToString().Trim().ToUpper())
        //                {
        //                    validtemplate = false;

        //                }
        //                if ("PRACTICE" != uploadReportDT.Columns[7].ColumnName.ToString().Trim().ToUpper())
        //                {
        //                    validtemplate = false;

        //                }
        //                if ("PROJECT" != uploadReportDT.Columns[8].ColumnName.ToString().Trim().ToUpper())
        //                {

        //                    validtemplate = false;

        //                }
        //                if ("MANAGERNAME" != uploadReportDT.Columns[9].ColumnName.ToString().Trim().ToUpper())
        //                {

        //                    validtemplate = false;

        //                }
        //                if ("ATTENDANCESTATUS" != uploadReportDT.Columns[10].ColumnName.ToString().Trim().ToUpper())
        //                {

        //                    validtemplate = false;

        //                }
        //                if ("TRAININGHRS" != uploadReportDT.Columns[11].ColumnName.ToString().Trim().ToUpper())
        //                {
        //                    validtemplate = false;

        //                }
        //                if ("TRAININGDAYS" != uploadReportDT.Columns[12].ColumnName.ToString().Trim().ToUpper())
        //                {
        //                    validtemplate = false;

        //                }
        //                if ("TOTALHOURS" != uploadReportDT.Columns[13].ColumnName.ToString().Trim().ToUpper())
        //                {
        //                    validtemplate = false;

        //                }
        //                if ("TRAINERNAME" != uploadReportDT.Columns[14].ColumnName.ToString().Trim().ToUpper())
        //                {
        //                    validtemplate = false;

        //                }
        //                if ("SOURCE" != uploadReportDT.Columns[15].ColumnName.ToString().Trim().ToUpper())
        //                {
        //                    validtemplate = false;

        //                }
        //                if ("CATEGORY" != uploadReportDT.Columns[16].ColumnName.ToString().Trim().ToUpper())
        //                {
        //                    validtemplate = false;

        //                }
        //                if ("MODE" != uploadReportDT.Columns[17].ColumnName.ToString().Trim().ToUpper())
        //                {
        //                    validtemplate = false;

        //                }
        //                if ("FEEDBACKRATING" != uploadReportDT.Columns[18].ColumnName.ToString().Trim().ToUpper())
        //                {
        //                    validtemplate = false;

        //                }

        //            }
        //            if (validtemplate)
        //            {
        //                using (TrianzOperationsEntities db = new TrianzOperationsEntities())
        //                {
        //                    string sXMLDataRport;
        //                    StringWriter oStringWriter = new StringWriter();

        //                    uploadReportDT.WriteXml(oStringWriter, XmlWriteMode.IgnoreSchema);
        //                    sXMLDataRport = oStringWriter.ToString();
        //                    var xmlSchemaData = new SqlParameter("@xml", sXMLDataRport);
        //                    var dataReuslt = db.Set<List<int>>().FromSqlRaw("exec sp_TrainingReport @xml", xmlSchemaData).ToList();
        //                    if (dataReuslt != null)
        //                    {
        //                        if (dataReuslt.Count > 0)

        //                        isUpdatedReport = "true";
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                isValidTemplate = "false";
        //            }

        //            validateUploadTemplate.isUpdatedReport = isUpdatedReport;
        //            validateUploadTemplate.isValidTemplate = isValidTemplate;
        //        }

        //    }
        //    catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        //    {
        //        Exception raise = dbEx;
        //        foreach (var validationErrors in dbEx)
        //        {
        //            foreach (var validationError in validationErrors.ValidationErrors)
        //            {
        //                string message = string.Format("{0}:{1}",
        //                    validationErrors.Entry.Entity.ToString(),
        //                    validationError.ErrorMessage);
        //                // raise a new exception nesting
        //                // the current instance as InnerException
        //                raise = new InvalidOperationException(message, raise);
        //            }
        //        }
        //        ErrorHandling expcls = new ErrorHandling();
        //        expcls.Error(raise);
        //        throw raise;
        //    }

        //    return Json(validateUploadTemplate); ;
        //}

        public JsonResult InsertTrainingData(IFormCollection form, IFormFile file)
        {

            JsonResult validateUploadTemplate = new JsonResult(null);
            ClientOutput ct = new ClientOutput();

            DataTable uploadReportDT = new DataTable();
            DataSet ds = new DataSet();
            try
            {
                List<TrainingData> errlist = new List<TrainingData>();
                if (HttpContext.Request.Form.Files["file"].Length > 0)
                {
                    string fileExtension =
                                         System.IO.Path.GetExtension(HttpContext.Request.Form.Files["file"].FileName);

                    if (fileExtension == ".xls" || fileExtension == ".xlsx")
                    {
                        //string fileLocation = Path.Combine(Env.ContentRootPath, "Content/") + HttpContext.Request.Form.Files["file"].FileName;
                        string fileLocation = Path.Combine(Env.ContentRootPath, "Content/") + string.Format(@"{0}.xlsx", "Training" + DateTime.Now.Ticks);
                        if (System.IO.File.Exists(fileLocation))
                        {

                            System.IO.File.Delete(fileLocation);

                        }
                        using (var stream = System.IO.File.Create(fileLocation))
                    { HttpContext.Request.Form.Files["file"].CopyTo(stream); }

                        var File = new FileInfo(fileLocation);

                        bool validtemplate = true;
                        using (ExcelPackage package = new ExcelPackage(File))
                        {
                            System.IO.File.Delete(fileLocation);
                            ExcelWorkbook workBook = package.Workbook;
                            if (workBook != null)
                            {
                                if (workBook.Worksheets.Count > 0)
                                {
                                    ExcelWorksheet workSheet = workBook.Worksheets.First();

                                    //uploadReportDT = ToDataTable(workSheet);


                                    DataTable table = new DataTable();

                                    string rownum = "";


                                    foreach (var firstRowCell in workSheet.Cells[1, 1, 1, workSheet.Dimension.End.Column])
                                    {
                                        table.Columns.Add(firstRowCell.Text.Replace(" ", ""));

                                    }
                                    table.TableName = "TraningReport";


                                    if (table.Columns.Count == 14)
                                    {

                                        if ("programname" != table.Columns[0].ColumnName.ToString().Trim().ToLower())
                                        {
                                            validtemplate = false;

                                        }
                                        if ("startdate" != table.Columns[1].ColumnName.ToString().Trim().ToLower())
                                        {
                                            validtemplate = false;

                                        }
                                        if ("enddate" != table.Columns[2].ColumnName.ToString().Trim().ToLower())
                                        {
                                            validtemplate = false;

                                        }
                                        if ("emp.code" != table.Columns[3].ColumnName.ToString().Trim().ToLower())
                                        {
                                            validtemplate = false;

                                        }
                                        if ("location" != table.Columns[4].ColumnName.ToString().Trim().ToLower())
                                        {
                                            validtemplate = false;

                                        }
                                        if ("attendancestatus" != table.Columns[5].ColumnName.ToString().Trim().ToLower())
                                        {
                                            validtemplate = false;

                                        }
                                        if ("traininghrs" != table.Columns[6].ColumnName.ToString().Trim().ToLower())
                                        {
                                            validtemplate = false;

                                        }
                                        if ("trainingdays" != table.Columns[7].ColumnName.ToString().Trim().ToLower())
                                        {
                                            validtemplate = false;

                                        }
                                        if ("totalhours" != table.Columns[8].ColumnName.ToString().Trim().ToLower())
                                        {
                                            validtemplate = false;

                                        }
                                        if ("trainername" != table.Columns[9].ColumnName.ToString().Trim().ToLower())
                                        {
                                            validtemplate = false;

                                        }
                                        if ("source" != table.Columns[10].ColumnName.ToString().Trim().ToLower())
                                        {
                                            validtemplate = false;

                                        }
                                        if ("category" != table.Columns[11].ColumnName.ToString().Trim().ToLower())
                                        {
                                            validtemplate = false;

                                        }
                                        if ("mode" != table.Columns[12].ColumnName.ToString().Trim().ToLower())
                                        {
                                            validtemplate = false;

                                        }
                                        if ("feedbackrating" != table.Columns[13].ColumnName.ToString().Trim().ToLower())
                                        {
                                            validtemplate = false;

                                        }





                                        if (validtemplate)
                                        {
                                            List<int> lstEmployee = new List<int>();
                                            int correctc = 0;
                                            int faiurecunt = 0;
                                            int Dupliccorrectc = 0;
                                            if (workSheet.Dimension.End.Row > 1)
                                            {
                                                for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                                                {
                                                    bool strtdateformatval = true;
                                                    bool checkstrtdateformat = true;
                                                    bool Enddateformatval = true;
                                                    bool checkenddateformat = true;
                                                    bool EMPintervalue = true;
                                                    bool Isemployeeexist = true;
                                                    bool progmnamecharval = true;
                                                    bool Trainghrsintval = true;
                                                    bool checkhrsintval = true;
                                                    bool trainingdayintval = true;
                                                    bool checktrainingday = true;
                                                    bool Totalhrsintval = true;
                                                    bool checktotalhrs = true;
                                                    bool feedbackratintval = true;
                                                    bool locationcheck = true;
                                                    bool attendancestatuscheck = true;
                                                    bool trainernamechek = true;
                                                    bool sourcecheck = true;
                                                    bool categorycheck = true;
                                                    bool modecheck = true;





                                                    string programname = "";
                                                    string strtdate = "";
                                                    string enddate = "";
                                                    string empid1 = "";
                                                    string location = "";
                                                    string attendancestatus = "";
                                                    string traininghrs = "";
                                                    string trainingdays = "";
                                                    string totalhrs = "";
                                                    string trainername = "";
                                                    string source = "";
                                                    string category = "";
                                                    string mode = "";
                                                    string feedbackrating = "";


                                                    var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];
                                                    var newRow = table.NewRow();
                                                    //if (row.Columns == 14)
                                                    //{
                                                    foreach (var cell in row)
                                                    {
                                                        newRow[cell.Start.Column - 1] = cell.Text;

                                                        if (cell.Start.Column == 1)
                                                        {
                                                            programname = cell.Text;
                                                            if (string.IsNullOrEmpty(Convert.ToString(programname)))

                                                                progmnamecharval = false;

                                                        }
                                                        else if (cell.Start.Column == 2)
                                                        {
                                                            strtdate = cell.Text;
                                                            DateTime value;
                                                            if (string.IsNullOrEmpty(Convert.ToString(cell.Text)))
                                                                strtdateformatval = false;
                                                           else if (!DateTime.TryParse(cell.Text, out value))
                                                                strtdateformatval = false;
                                                            else
                                                            {
                                                                
                                                                    if (Convert.ToDateTime(cell.Text) > DateTime.Now.Date)
                                                                        checkstrtdateformat = false;
                                                            }
                                                        }
                                                        else if (cell.Start.Column == 3)
                                                        {
                                                            enddate = cell.Text;
                                                            DateTime value;

                                                            if (string.IsNullOrEmpty(Convert.ToString(cell.Text)))
                                                                Enddateformatval = false;
                                                            else if (!DateTime.TryParse(cell.Text, out value))
                                                                Enddateformatval = false;
                                                            else
                                                            {

                                                                if (Convert.ToDateTime(cell.Text) > DateTime.Now.Date)
                                                                    checkenddateformat = false;
                                                            }
                                                        }
                                                        else if (cell.Start.Column == 4)
                                                        {
                                                            empid1 = cell.Text;
                                                            int value;
                                                            if (!int.TryParse(cell.Text, out value))
                                                                EMPintervalue = false;
                                                            else
                                                            {
                                                                //lstEmployee.Add(new Employee() { EmployeeId = Convert.ToInt32(cell.Text) });
                                                                if (string.IsNullOrEmpty(Convert.ToString(empid1)))
                                                                    lstEmployee.Add(Convert.ToInt32(cell.Text));
                                                                int emp = Convert.ToInt32(cell.Text);
                                                                var emt = (from et in db.Employees
                                                                           where et.EmployeeId == emp && et.IsActive == true
                                                                           select et).FirstOrDefault();
                                                                if (emt == null)
                                                                    Isemployeeexist = false;

                                                            }
                                                        }
                                                        else if (cell.Start.Column == 5)
                                                        {
                                                            location = cell.Text;
                                                            if (string.IsNullOrEmpty(Convert.ToString(location)))
                                                                locationcheck = false;

                                                        }
                                                        else if (cell.Start.Column == 6)
                                                        {
                                                            attendancestatus = cell.Text;
                                                            if (string.IsNullOrEmpty(Convert.ToString(attendancestatus)))
                                                                attendancestatuscheck = false;

                                                        }
                                                        else if (cell.Start.Column == 7)
                                                        {
                                                            traininghrs = cell.Text;
                                                            decimal value;
                                                            if (string.IsNullOrEmpty(Convert.ToString(traininghrs)))
                                                                Trainghrsintval = false;
                                                           else if (!decimal.TryParse(cell.Text, out value))
                                                                Trainghrsintval = false;
                                                            else
                                                            {
                                                                
                                                                    if (Convert.ToDecimal(cell.Text) < 0 || Convert.ToDecimal(cell.Text) > 24)
                                                                        checkhrsintval = false;

                                                            }
                                                        }
                                                        else if (cell.Start.Column == 8)
                                                        {
                                                            trainingdays = cell.Text;
                                                            decimal value;
                                                            if (string.IsNullOrEmpty(Convert.ToString(trainingdays)))
                                                                trainingdayintval = false;
                                                          else  if (!decimal.TryParse(cell.Text, out value))
                                                                trainingdayintval = false;
                                                            else
                                                            {
                                                               
                                                                    if (Convert.ToDecimal(cell.Text) < 0 || Convert.ToDecimal(cell.Text) > 24)
                                                                        checktrainingday = false;

                                                            }
                                                        }
                                                        else if (cell.Start.Column == 9)
                                                        {
                                                            totalhrs = cell.Text;
                                                            decimal value;
                                                            if (string.IsNullOrEmpty(Convert.ToString(totalhrs)))
                                                                Totalhrsintval = false;
                                                            else if (!decimal.TryParse(cell.Text, out value))
                                                                Totalhrsintval = false;
                                                            else
                                                            {
                                                               
                                                                    if (Convert.ToDecimal(cell.Text) < 0 || Convert.ToDecimal(cell.Text) > 24)
                                                                        checktotalhrs = false;

                                                            }
                                                        }
                                                        else if (cell.Start.Column == 10)
                                                        {
                                                            trainername = cell.Text;
                                                            if (string.IsNullOrEmpty(Convert.ToString(trainername)))
                                                                trainernamechek = false;

                                                        }
                                                        else if (cell.Start.Column == 11)
                                                        {
                                                            source = cell.Text;
                                                            if (string.IsNullOrEmpty(Convert.ToString(source)))
                                                                sourcecheck = false;

                                                        }
                                                        else if (cell.Start.Column == 12)
                                                        {
                                                            category = cell.Text;
                                                            if (string.IsNullOrEmpty(Convert.ToString(category)))
                                                                categorycheck = false;

                                                        }
                                                        else if (cell.Start.Column == 13)
                                                        {
                                                            mode = cell.Text;
                                                            if (string.IsNullOrEmpty(Convert.ToString(mode)))
                                                                modecheck = false;

                                                        }
                                                        else if (cell.Start.Column == 14)
                                                        {
                                                            feedbackrating = cell.Text;
                                                            if ((string.IsNullOrEmpty(Convert.ToString(feedbackrating))))
                                                                feedbackratintval = false;
                                                        }

                                                    }


                                                    string remark = "";

                                                    if (!EMPintervalue || !strtdateformatval || !checkstrtdateformat || !Enddateformatval || !Isemployeeexist
                                                        || !checkenddateformat || !Trainghrsintval || !checkhrsintval || !trainingdayintval || !checktrainingday || !Totalhrsintval
                                                        || !checktotalhrs || !feedbackratintval || !modecheck || !categorycheck || !sourcecheck || !trainernamechek || !attendancestatuscheck || !locationcheck || !progmnamecharval)
                                                    {
                                                        TrainingData err = new TrainingData();
                                                        err.RowNum = rowNumber.ToString();
                                                        err.ProgramName = programname;
                                                        err.StartDate = strtdate;
                                                        err.EndDate = enddate;
                                                        err.ParticipantID = empid1;
                                                        err.Location = location;
                                                        err.AttendanceStatus = attendancestatus;
                                                        err.Traininghrs = traininghrs;
                                                        err.TrainingDays = trainingdays; ;
                                                        err.TotalHours = totalhrs;
                                                        err.TrainerName = trainername;
                                                        err.Source = source;
                                                        err.Category = category;
                                                        err.Mode = mode;
                                                        err.Feedbackrating = feedbackrating;

                                                        if (!EMPintervalue)
                                                        {

                                                            remark = "Enter interger values in EmployeeID column";
                                                        }
                                                        if (!feedbackratintval)
                                                        {

                                                            remark = "Enter interger values in Feedbackrating column";
                                                        }
                                                        if (!Trainghrsintval)
                                                        {
                                                            if (remark == string.Empty)
                                                                remark = "Enter interger values in Training Hours column";
                                                            else
                                                                remark = remark + "," + "Enter interger values in Training Hours column";
                                                        }
                                                        if (!checkhrsintval)
                                                        {
                                                            if (remark == string.Empty)
                                                                remark = "Training Hours should be greater than or equal to Zero and less than or equal to 24";
                                                            else
                                                                remark = remark + "," + "Training Hours should be greater than or equal to Zero and less than or equal to 24";
                                                        }
                                                        if (!Totalhrsintval)
                                                        {
                                                            if (remark == string.Empty)
                                                                remark = "Enter interger values in Total Hours column";
                                                            else
                                                                remark = remark + "," + "Enter interger values in Total Hours column";
                                                        }
                                                        if (!checktotalhrs)
                                                        {
                                                            if (remark == string.Empty)
                                                                remark = "Total Hours should be greater than or equal to Zero and less than or equal to 24";
                                                            else
                                                                remark = remark + "," + "Total Hours should be greater than or equal to Zero and less than or equal to 24";
                                                        }
                                                        if (!strtdateformatval)
                                                        {
                                                            if (remark == string.Empty)
                                                                remark = "Start date should be on date format";
                                                            else
                                                                remark = remark + "," + "Start date should be on date format";
                                                        }
                                                        if (!checkstrtdateformat)
                                                        {
                                                            if (remark == string.Empty)
                                                                remark = "Start Date should be less than or equal to current date";
                                                            else
                                                                remark = remark + "," + "Start Date should be less than or equal to current date";
                                                        }
                                                        if (!Enddateformatval)
                                                        {
                                                            if (remark == string.Empty)
                                                                remark = "End date should be on date format";
                                                            else
                                                                remark = remark + "," + "End date should be on date format";
                                                        }
                                                        if (!checkenddateformat)
                                                        {
                                                            if (remark == string.Empty)
                                                                remark = "End Date should be less than or equal to current date";
                                                            else
                                                                remark = remark + "," + "End Date should be less than or equal to current date";
                                                        }

                                                        if (!Isemployeeexist)
                                                        {
                                                            if (remark == string.Empty)
                                                                remark = "Employee code(s) does not exist with active status";
                                                            else
                                                                remark = remark + "," + "Employee code(s) does not exist with active status";

                                                        }
                                                        if (!progmnamecharval)
                                                        {
                                                            if (remark == string.Empty)
                                                                remark = "Please Enter ProgramName";
                                                            else
                                                                remark = remark + "," + "Please Enter ProgramName";
                                                        }
                                                        if (!locationcheck)
                                                        {
                                                            if (remark == string.Empty)
                                                                remark = "Please Enter Location";
                                                            else
                                                                remark = remark + "," + "Please Enter Location";
                                                        }
                                                        if (!attendancestatuscheck)
                                                        {
                                                            if (remark == string.Empty)
                                                                remark = "Please Enter Attandence Status";
                                                            else
                                                                remark = remark + "," + "Please Enter Attandence Status";
                                                        }
                                                        if (!trainernamechek)
                                                        {
                                                            if (remark == string.Empty)
                                                                remark = "Please Enter Trainer Name";
                                                            else
                                                                remark = remark + "," + "Please Enter Trainer Name";
                                                        }
                                                        if (!sourcecheck)
                                                        {
                                                            if (remark == string.Empty)
                                                                remark = "Please Enter Source";
                                                            else
                                                                remark = remark + "," + "Please Enter Source";
                                                        }
                                                        if (!categorycheck)
                                                        {
                                                            if (remark == string.Empty)
                                                                remark = "Please Enter Category";
                                                            else
                                                                remark = remark + "," + "Please Enter Category";
                                                        }
                                                        if (!modecheck)
                                                        {
                                                            if (remark == string.Empty)
                                                                remark = "Please Enter Mode";
                                                            else
                                                                remark = remark + "," + "Please Enter Mode";
                                                        }






                                                        faiurecunt++;
                                                        err.Remark = remark;
                                                        errlist.Add(err);

                                                    }
                                                    else
                                                    {
                                                        correctc++;
                                                        table.Rows.Add(newRow);
                                                        DataTable dtfil = new DataTable();
                                                        table.DefaultView.RowFilter = "Emp.Code='" + empid1 + "'and ProgramName='" + programname + "'and StartDate='" + strtdate + "' and EndDate='" + enddate + "'";
                                                        dtfil = table.DefaultView.ToTable();
                                                        var rows = dtfil.AsEnumerable().GroupBy(r => new { EmpID = r.Field<string>("Emp.Code"), ProgramName = r.Field<string>("ProgramName"), StartDate = r.Field<string>("StartDate"), EndDate = r.Field<string>("EndDate") }).Where(g => g.Count() > 1);
                                                        if (rows != null)
                                                        {

                                                            if (rows.Count() > 0)
                                                            {
                                                                correctc--;
                                                                Dupliccorrectc++;

                                                                TrainingData err = new TrainingData();
                                                                err.RowNum = rowNumber.ToString();
                                                                err.ProgramName = programname;
                                                                err.StartDate = strtdate;
                                                                err.EndDate = enddate;
                                                                err.ParticipantID = empid1;
                                                                err.Location = location;
                                                                err.AttendanceStatus = attendancestatus;
                                                                err.Traininghrs = traininghrs;
                                                                err.TrainingDays = trainingdays;
                                                                err.TotalHours = totalhrs;
                                                                err.TrainerName = trainername;
                                                                err.Source = source;
                                                                err.Category = category;
                                                                err.Mode = mode;
                                                                err.Feedbackrating = feedbackrating;
                                                                err.Remark = "Duplicate record will not save";
                                                                errlist.Add(err);
                                                            }
                                                            // table.Rows.Remove(newRow);
                                                        }
                                                    }
                                                }


                                                if (faiurecunt > 0)
                                                {

                                                    ct.Isvalid = false;
                                                    ct.message = "No records for saving";
                                                    ct.datalist = errlist;
                                                    ct.Crrtcount = correctc.ToString();
                                                    ct.DuplicateCount = Dupliccorrectc.ToString();
                                                    ct.InCrrtcount = faiurecunt.ToString();
                                                    TempData["errodata"] = errlist;
                                                }
                                                if (correctc > 0)
                                                {
                                                    string sXMLDataRport;
                                                    StringWriter oStringWriter = new StringWriter();
                                                    uploadReportDT = table;
                                                    uploadReportDT.WriteXml(oStringWriter, XmlWriteMode.IgnoreSchema);
                                                    sXMLDataRport = oStringWriter.ToString();
                                                    var xmlSchemaData = new SqlParameter("@xml", sXMLDataRport);
                                                    var dataReuslt = db.Set<List<int>>().FromSqlRaw("exec sp_TrainingReport @xml", xmlSchemaData).ToList();
                                                    if (dataReuslt != null)
                                                    {
                                                        if (dataReuslt.Count > 0)
                                                        {
                                                            ct.Isvalid = true;
                                                            ct.datalist = errlist;
                                                            ct.message = "Correct Count data Saved Successfully";
                                                            TempData["errodata"] = errlist;
                                                            ct.Crrtcount = correctc.ToString();
                                                            ct.DuplicateCount = Dupliccorrectc.ToString();
                                                            ct.InCrrtcount = faiurecunt.ToString();
                                                        }

                                                    }
                                                }
                                            }
                                            else
                                            {
                                                TempData["errodata"] = errlist;
                                                ct.datalist = errlist;
                                                ct.Isvalid = false;
                                                ct.message = "Uploaded file have no data";
                                                ct.Crrtcount = "0";
                                                ct.DuplicateCount = "0";
                                                ct.InCrrtcount = "0";
                                            }


                                        }
                                        else
                                        {
                                            TempData["errodata"] = errlist;
                                            ct.Isvalid = false;
                                            ct.datalist = errlist;
                                            ct.message = "Uploaded file Columns names are not matching with sample file columns names";
                                            ct.Crrtcount = "0";
                                            ct.DuplicateCount = "0";
                                            ct.InCrrtcount = "0";
                                        }
                                    }
                                    else
                                    {
                                        TempData["errodata"] = errlist;
                                        ct.datalist = errlist;
                                        ct.Isvalid = false;
                                        ct.message = "Uploaded file columns count should be equal to 4 ";
                                        ct.Crrtcount = "0";
                                        ct.DuplicateCount = "0";
                                        ct.InCrrtcount = "0";
                                    }



                                }
                                else
                                {
                                    TempData["errodata"] = errlist;
                                    ct.datalist = errlist;
                                    ct.Isvalid = false;
                                    ct.message = "Uploaded file have no data";
                                    ct.Crrtcount = "0";
                                    ct.DuplicateCount = "0";
                                    ct.InCrrtcount = "0";
                                }

                            }
                            else
                            {
                                TempData["errodata"] = errlist;
                                ct.datalist = errlist;
                                ct.Isvalid = false;
                                ct.message = "Uploaded file have no data";
                                ct.Crrtcount = "0";
                                ct.DuplicateCount = "0";
                                ct.InCrrtcount = "0";
                            }

                        }

                    }
                    else
                    {
                        TempData["errodata"] = errlist;
                        ct.datalist = errlist;
                        ct.Isvalid = false;
                        ct.message = "Upload .xls (or) .xlsx extensation files only";
                        ct.Crrtcount = "0";
                        ct.DuplicateCount = "0";
                        ct.InCrrtcount = "0";
                    }
                }
                else
                {
                    TempData["errodata"] = errlist;
                    ct.datalist = errlist;
                    ct.Isvalid = false;
                    ct.message = "File not exists";
                    ct.Crrtcount = "0";
                    ct.DuplicateCount = "0";
                    ct.InCrrtcount = "0";
                }

            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                ct.Isvalid = false;
                ct.message = "";

            }
            validateUploadTemplate.Value = ct;

            return validateUploadTemplate;
        }



        //public DataTable ToDataTable(ExcelWorksheet curentWorkSheet)
        //{
        //    DataTable table = new DataTable();
        //    try
        //    {
        //        ExcelWorksheet workSheet = curentWorkSheet;

        //        foreach (var firstRowCell in workSheet.Cells[1, 1, 1, workSheet.Dimension.End.Column])
        //        {
        //            table.Columns.Add(firstRowCell.Text.Replace(" ", ""));

        //        }

        //        for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
        //        {
        //            var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];
        //            var newRow = table.NewRow();
        //            foreach (var cell in row)
        //            {
        //                newRow[cell.Start.Column - 1] = cell.Text;
        //            }
        //            table.Rows.Add(newRow);
        //        }

        //        table.TableName = "TraningReport";

        //    }
        //    catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        //    {
        //        Exception raise = dbEx;
        //        foreach (var validationErrors in dbEx)
        //        {
        //            foreach (var validationError in validationErrors.ValidationErrors)
        //            {
        //                string message = string.Format("{0}:{1}",
        //                    validationErrors.Entry.Entity.ToString(),
        //                    validationError.ErrorMessage);
        //                // raise a new exception nesting
        //                // the current instance as InnerException
        //                raise = new InvalidOperationException(message, raise);
        //            }
        //        }
        //        ErrorHandling expcls = new ErrorHandling();
        //        expcls.Error(raise);
        //        throw raise;
        //    }


        //    //var rows = table.AsEnumerable().GroupBy(r => new { EmpID = r.Field<string>("EmployeeID") }).Where(g => g.Count() > 1);
        //    //if (rows != null)
        //    //{
        //    //    var i = 0;
        //    //    var count = rows.Count();
        //    //    foreach (var item in rows)
        //    //    {
        //    //        sbEmpIDs.Append(item.Key.EmpID.ToString());
        //    //        if (!(++i == count))
        //    //            sbEmpIDs.Append(",");
        //    //    }
        //    //    DataTable dt = table.Clone();
        //    //    foreach (DataRow dr in table.Rows)
        //    //    {
        //    //        if (!sbEmpIDs.ToString().Contains(dr["EmployeeID"].ToString()))
        //    //        {
        //    //            dt.Rows.Add(dr.ItemArray);
        //    //        }
        //    //    }

        //    //    return dt;
        //    //}
        //    //else
        //        return table;



        //}
        public ActionResult Report()
        {
            try
            {

                var program = from p in db.Trainings
                              select new { p.ProgramName };
                ReportViewModel objviewModel = new ReportViewModel();
                List<SelectListItem> ProgramList = new List<SelectListItem>();
                List<SelectListItem> LocationList = new List<SelectListItem>();
                List<SelectListItem> PracticeList = new List<SelectListItem>();
                List<SelectListItem> AttendanceStatusList = new List<SelectListItem>();
                List<SelectListItem> SourceList = new List<SelectListItem>();
                List<SelectListItem> CategoryList = new List<SelectListItem>();
                List<SelectListItem> ModeList = new List<SelectListItem>();

                ProgramList = (from c in db.Trainings
                               select new SelectListItem
                               {
                                   Text = c.ProgramName,
                                   Value = c.ProgramName.ToString()
                               }).Distinct().ToList();
                ViewBag.ProgramList = ProgramList.ToList();
                LocationList = (from c in db.Trainings
                                select new SelectListItem
                                {
                                    Text = c.Location,
                                    Value = c.Location.ToString()
                                }).Distinct().ToList();
                ViewBag.LocationList = LocationList.ToList();


                PracticeList = (from c in db.Employees
                                select new SelectListItem
                                {
                                    Text = c.Practice,
                                    Value = c.Practice.ToString()
                                }).Distinct().ToList();
                ViewBag.PracticeList = PracticeList.ToList();

                AttendanceStatusList = (from c in db.Trainings
                                        select new SelectListItem
                                        {
                                            Text = c.AttendanceStatus,
                                            Value = c.AttendanceStatus.ToString()
                                        }).Distinct().ToList();
                ViewBag.AttendanceStatusList = AttendanceStatusList.ToList();

                SourceList = (from c in db.Trainings
                              select new SelectListItem
                              {
                                  Text = c.Source,
                                  Value = c.Source.ToString()
                              }).Distinct().ToList();
                ViewBag.SourceList = SourceList.ToList();
                CategoryList = (from c in db.Trainings
                                select new SelectListItem
                                {
                                    Text = c.Category,
                                    Value = c.Category.ToString()
                                }).Distinct().ToList();
                ViewBag.CategoryList = CategoryList.ToList();
                ModeList = (from c in db.Trainings
                            select new SelectListItem
                            {
                                Text = c.Mode,
                                Value = c.Mode.ToString()
                            }).Distinct().ToList();
                ViewBag.ModeList = ModeList.ToList();

                return View();
            }

            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                Exception raise = dbEx;
                // EF Core DbUpdateException does not have EntityValidationErrors - skip validation error iteration
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(raise);
                throw raise;
            }
        }

        public ActionResult GenerateReport(string ProgramName, string Location, string Practice, string AttendanceStatus, string Source, string Category, string Mode, string StartDate, string EndDate)
        {

            try
            {

                List<SqlParameter> par = new List<SqlParameter>();
                par.Add(new SqlParameter("@ProgramName", string.IsNullOrEmpty(ProgramName) ? (object)DBNull.Value : ProgramName));
                par.Add(new SqlParameter("@Location", string.IsNullOrEmpty(Location) ? (object)DBNull.Value : Location));
                par.Add(new SqlParameter("@Practice", string.IsNullOrEmpty(Practice) ? (object)DBNull.Value : Practice));
                par.Add(new SqlParameter("@AttendanceStatus", string.IsNullOrEmpty(AttendanceStatus) ? (object)DBNull.Value : AttendanceStatus));
                par.Add(new SqlParameter("@Source", string.IsNullOrEmpty(Source) ? (object)DBNull.Value : Source));
                par.Add(new SqlParameter("@Category", string.IsNullOrEmpty(Category) ? (object)DBNull.Value : Category));
                par.Add(new SqlParameter("@Mode", string.IsNullOrEmpty(Mode) ? (object)DBNull.Value : Mode));
                par.Add(new SqlParameter("@StartDate", string.IsNullOrEmpty(StartDate) ? (object)DBNull.Value : StartDate));
                par.Add(new SqlParameter("@EndDate", string.IsNullOrEmpty(EndDate) ? (object)DBNull.Value : EndDate));


                var TrainingInfo = db.Set<sp_GetTrainingData_Result>().FromSqlRaw("exec sp_GetTrainingData @ProgramName,@Location,@Practice,@AttendanceStatus,@Source,@Category,@Mode,@StartDate,@EndDate", par.ToArray()).ToList();

                return PartialView("_GenerateReport", TrainingInfo);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                Exception raise = dbEx;
                // EF Core DbUpdateException does not have EntityValidationErrors - skip validation error iteration
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(raise);
                throw raise;
            }
        }

        public ActionResult DownloadReport(string ProgramName, string Location, string Practice, string AttendanceStatus, string Source, string Category, string Mode, string StartDate, string EndDate)
        {


            try
            {

                var programName = new SqlParameter("@ProgramName", ProgramName == "" ? (object)DBNull.Value : ProgramName);
                var location = new SqlParameter("@Location", Location == "" ? (object)DBNull.Value : Location);
                var practice = new SqlParameter("@Practice", Practice == "" ? (object)DBNull.Value : Practice);
                var attendanceStatus = new SqlParameter("@AttendanceStatus", AttendanceStatus == "" ? (object)DBNull.Value : AttendanceStatus);
                var source = new SqlParameter("@Source", Source == "" ? (object)DBNull.Value : Source);
                var category = new SqlParameter("@Category", Category == "" ? (object)DBNull.Value : Category);
                var mode = new SqlParameter("@Mode", Mode == "" ? (object)DBNull.Value : Mode);
                var startDate = new SqlParameter("@StartDate", StartDate == "" ? (object)DBNull.Value : StartDate);
                var endDate = new SqlParameter("@EndDate", EndDate == "" ? (object)DBNull.Value : EndDate);
                var TrainingInfo = db.Set<sp_GetTrainingData_Result>().FromSqlRaw("exec sp_GetTrainingData @ProgramName,@Location,@Practice,@AttendanceStatus,@Source,@Category,@Mode,@StartDate,@EndDate", programName, location, practice, attendanceStatus, source, category, mode, startDate, endDate).ToList();
                var s = (from emp in TrainingInfo
                         select new sp_GetTrainingData_Result
                         {

                             EmployeeId = emp.EmployeeId,
                             EmployeeName = emp.EmployeeName,
                             ProgramName = emp.ProgramName,
                             StartDate = emp.StartDate,
                             EndDate = emp.EndDate,
                             Location = emp.Location,
                             Grade = emp.Grade,
                             Practice = emp.Practice,
                             Position = emp.Position,
                             ManagerName = emp.ManagerName,
                             Attendance = emp.Attendance,
                             TrainingHour = emp.TrainingHour,
                             TrainingDays = emp.TrainingDays,
                             TotalHours = emp.TotalHours,
                             TrainerName = emp.TrainerName,
                             Source = emp.Source,
                             Category = emp.Category,
                             Mode = emp.Mode,
                             Feedbackrating = emp.Feedbackrating,

                         }).OrderBy(p => p.EmployeeId).ToList();


                #region Export to Excel

                using (ExcelPackage package = new ExcelPackage())
            {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Training Information");
                    worksheet.TabColor = System.Drawing.Color.Green;
                    worksheet.DefaultRowHeight = 18f;
                    worksheet.Row(1).Height = 20f;

                    using (var range = worksheet.Cells[1, 1, 1, 19])
{
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#5B9BD5"));
                        range.Style.Font.Color.SetColor(System.Drawing.Color.White);

                        range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Color.SetColor(System.Drawing.Color.LightGray);
                    }

                    //Add the headers
                    worksheet.Cells[1, 1].Value = "Employee ID";
                    worksheet.Cells[1, 2].Value = "Employee Name";
                    worksheet.Cells[1, 3].Value = "Program Name";
                    worksheet.Cells[1, 4].Value = "Start Date";
                    worksheet.Cells[1, 5].Value = "End Date";
                    worksheet.Cells[1, 6].Value = "Location";
                    worksheet.Cells[1, 7].Value = "Grade";
                    worksheet.Cells[1, 8].Value = "Practice";
                    worksheet.Cells[1, 9].Value = "Position";
                    worksheet.Cells[1, 10].Value = "Manager Name";
                    worksheet.Cells[1, 11].Value = "Attendance";
                    worksheet.Cells[1, 12].Value = "Training Hours";
                    worksheet.Cells[1, 13].Value = "Training Days";
                    worksheet.Cells[1, 14].Value = "Total Hours";
                    worksheet.Cells[1, 15].Value = "Trainer Name";
                    worksheet.Cells[1, 16].Value = "Source";
                    worksheet.Cells[1, 17].Value = "Category";
                    worksheet.Cells[1, 18].Value = "Mode";
                    worksheet.Cells[1, 19].Value = "Feedback rating";




                    worksheet.Column(1).Width = 20f;
                    worksheet.Column(2).AutoFit(42f);
                    worksheet.Column(3).AutoFit(42f);
                    worksheet.Column(4).AutoFit(42f);
                    worksheet.Column(5).AutoFit(12f);
                    worksheet.Column(6).AutoFit(42f);
                    worksheet.Column(7).AutoFit(30f);
                    worksheet.Column(8).AutoFit(30f);
                    worksheet.Column(9).AutoFit(30f);
                    worksheet.Column(10).AutoFit(42f);
                    worksheet.Column(11).AutoFit(42f);
                    worksheet.Column(12).AutoFit(30f);
                    worksheet.Column(13).AutoFit(10f);
                    worksheet.Column(14).AutoFit(10f);
                    worksheet.Column(15).AutoFit(10f);
                    worksheet.Column(16).AutoFit(10f);
                    worksheet.Column(17).AutoFit(10f);
                    worksheet.Column(18).AutoFit(10f);
                    worksheet.Column(19).AutoFit(10f);



                    //Add the each row
                    for (int rowIndex = 0, row = 2; rowIndex < s.Count; rowIndex++, row++) // row indicates number of rows
                    {
                        worksheet.Cells[row, 1].Value = s[rowIndex].EmployeeId;
                        worksheet.Cells[row, 2].Value = s[rowIndex].EmployeeName;
                        worksheet.Cells[row, 3].Value = s[rowIndex].ProgramName;
                        worksheet.Cells[row, 4].Value = s[rowIndex].StartDate;
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "dd-MMM-yyyy";
                        //  worksheet.Cells[row, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                        worksheet.Cells[row, 5].Value = s[rowIndex].EndDate;
                        worksheet.Cells[row, 5].Style.Numberformat.Format = "dd-MMM-yyyy";
                        //   worksheet.Cells[row, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        worksheet.Cells[row, 6].Value = s[rowIndex].Location;
                        worksheet.Cells[row, 7].Value = s[rowIndex].Grade;
                        worksheet.Cells[row, 8].Value = s[rowIndex].Practice;
                        worksheet.Cells[row, 9].Value = s[rowIndex].Position;
                        worksheet.Cells[row, 10].Value = s[rowIndex].ManagerName;
                        worksheet.Cells[row, 11].Value = s[rowIndex].Attendance;
                        worksheet.Cells[row, 12].Value = s[rowIndex].TrainingHour;
                        worksheet.Cells[row, 13].Value = s[rowIndex].TrainingDays;
                        worksheet.Cells[row, 14].Value = s[rowIndex].TotalHours;
                        worksheet.Cells[row, 15].Value = s[rowIndex].TrainerName;
                        worksheet.Cells[row, 16].Value = s[rowIndex].Source;
                        worksheet.Cells[row, 17].Value = s[rowIndex].Category;
                        worksheet.Cells[row, 18].Value = s[rowIndex].Mode;
                        worksheet.Cells[row, 19].Value = s[rowIndex].Feedbackrating;



                        if (row % 2 == 1)
                        {
                            using (var range = worksheet.Cells[row, 1, row, 19])
{
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DDEBF7"));

                                range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                range.Style.Border.Top.Color.SetColor(System.Drawing.Color.LightGray);
                                range.Style.Border.Left.Color.SetColor(System.Drawing.Color.LightGray);
                                range.Style.Border.Right.Color.SetColor(System.Drawing.Color.LightGray);
                                range.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.LightGray);
                            }
                        }
                    }



                    Byte[] fileBytes = package.GetAsByteArray();
                    Response.Clear();
                    
                    Response.Headers.Add("content-disposition", "attachment;filename=" + "Training Data" + DateTime.Now.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                    
                    Response.ContentType = "application/vnd.ms-excel";
                    //StringWriter sw = new StringWriter();
                    Response.Body.Write(fileBytes);
                    

                }

                #endregion

                return new EmptyResult();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                Exception raise = dbEx;
                // EF Core DbUpdateException does not have EntityValidationErrors - skip validation error iteration
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(raise);
                throw raise;
            }

        }

        public ActionResult DeleteAll(string TrainingID)
        {
            //TrianzOperationsEntities db = new TrianzOperationsEntities();
            string[] employeelist = TrainingID.Split(',');
            for (int i = 0; i < employeelist.Length; i++)
            {

                int TrainingId = Convert.ToInt32(employeelist[i]);
                Training deletedrecord = db.Trainings.Find(TrainingId);
                db.Trainings.Remove(deletedrecord);
            }


            db.SaveChanges();

            return RedirectToAction("GenerateReport");

        }
        public FileResult DownloadTainingReportTemplate()
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Path.Combine(Env.ContentRootPath, "Content", "Document", "Training Sample Template.xlsx"));
            string fileName = "Training Sample Template.xlsx";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public ActionResult GetErrorRecordsParitalView()
        {
            List<TrainingData> ett = new List<TrainingData>();
            ett = (List<TrainingData>)TempData["errodata"];
            return PartialView("_ErrorRecords", ett);
        }

    }
    public class ClientOutput
    {
        public bool Isvalid { get; set; }
        public string message { get; set; }
        public List<TrainingData> datalist { get; set; }
        public string Crrtcount { get; set; }
        public string InCrrtcount { get; set; }
        public string DuplicateCount { get; set; }
    }
    public class TrainingData
    {
        public string RowNum { get; set; }
        public string ProgramName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string ParticipantID { get; set; }
        public string ParticipantName { get; set; }
        public string Location { get; set; }
        public string Grade { get; set; }
        public string Practice { get; set; }
        public string Project { get; set; }
        public string ManagerName { get; set; }
        public string AttendanceStatus { get; set; }
        public string Traininghrs { get; set; }
        public string TrainingDays { get; set; }
        public string TotalHours { get; set; }
        public string TrainerName { get; set; }
        public string Source { get; set; }
        public string Category { get; set; }
        public string Mode { get; set; }
        public string Feedbackrating { get; set; }
        public string Remark { get; set; }
    }
}
