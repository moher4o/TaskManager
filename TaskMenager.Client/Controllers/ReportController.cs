using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Services;
using static TaskManager.Common.DataConstants;
using OfficeOpenXml;
using TaskManager.Services.Models.ReportModels;
using TaskManager.Services.Models.EmployeeModels;
using TaskMenager.Client.Models.Report;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskManager.Services.Models;
using System.Drawing;
using OfficeOpenXml.Style;
using TaskManager.Services.Models.TaskModels;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Drawing.Chart.Style;
using Microsoft.AspNetCore.Authorization;
using TaskManager.Common;
using Microsoft.AspNetCore.Hosting;

namespace TaskMenager.Client.Controllers
{
    [Authorize(Policy = DataConstants.Employee)]
    public class ReportController : BaseController
    {
        private readonly IDirectorateService directorates;
        private readonly IDepartmentsService departments;
        private readonly ISectorsService sectors;

        public ReportController(IDirectorateService directorates, IDepartmentsService departments, ISectorsService sectors, IEmployeesService employees, ITasksService tasks, IHttpContextAccessor httpContextAccessor, IEmailService email, IWebHostEnvironment env) : base(httpContextAccessor, employees, tasks, email, env)
        {
            this.directorates = directorates;
            this.departments = departments;
            this.sectors = sectors;

        }

        public IActionResult PeriodReport()
        {
            try
            {
                var newPeriod = new PeriodReportViewModel();
                if (currentUser.RoleName == SuperAdmin)
                {
                    newPeriod.Directorates = this.directorates.GetDirectoratesNames(null)
                                   .Select(a => new SelectListItem
                                   {
                                       Text = a.TextValue,
                                       Value = a.Id.ToString()
                                   })
                                   .ToList();
                    newPeriod.DirectoratesId = newPeriod.Directorates.FirstOrDefault().Value;
                    newPeriod.Directorates.FirstOrDefault().Selected = true;
                }

                return View(newPeriod);
            }
            catch (Exception)
            {
                TempData["Error"] = "[PeriodReport] Грешка при подготовка на модела за отчет";
                return RedirectToAction("Index", "Home");
            }
        }


        public async Task<IActionResult> GetReport(PeriodReportViewModel model)
        {
            try
            {
                if (model.StartDate > model.EndDate)
                {
                    TempData["Error"] = "Невалидни дати";
                    return RedirectToAction(nameof(PeriodReport));
                }

                var newReport = new PeriodReportViewModel();
                var employeesIds = new List<int>();
                if (currentUser.RoleName == Employee)
                {
                    employeesIds.Add(currentUser.Id);
                }
                else if (currentUser.RoleName == SectorAdmin)
                {
                    employeesIds = this.employees.GetEmployeesNamesBySectorWithDeletedAsync(currentUser.SectorId).Result
                                                        .Select(e => e.Id)
                                                        .ToList();
                }
                else if (currentUser.RoleName == DepartmentAdmin)
                {
                    employeesIds = this.employees.GetEmployeesNamesByDepartmentWithDeletedAsync(currentUser.DepartmentId).Result
                                                        .Select(e => e.Id)
                                                        .ToList();
                }
                else if (currentUser.RoleName == DirectorateAdmin)
                {
                    employeesIds = this.employees.GetEmployeesNamesByDirectorateWithDeletedAsync(currentUser.DirectorateId).Result
                                                        .Select(e => e.Id)
                                                        .ToList();
                }
                else if (currentUser.RoleName == SuperAdmin)
                {
                    if (model.DirectoratesId == null)
                    {
                        TempData["Error"] = "Не е избрана дирекция";
                        return RedirectToAction(nameof(PeriodReport));
                    }
                    employeesIds = this.employees.GetEmployeesNamesByDirectorateWithDeletedAsync(int.Parse(model.DirectoratesId)).Result
                                        .Select(e => e.Id)
                                        .ToList();
                }
                newReport.EmployeesIds = employeesIds.ToArray();
                newReport.StartDate = model.StartDate.Date;
                newReport.EndDate = model.EndDate.Date;

                return await ExportReport(newReport);
            }
            catch (Exception)
            {
                TempData["Error"] = "Грешка при обработката на модела за отчет";
                return RedirectToAction(nameof(PeriodReport));
            }

        }

        public IActionResult ExportSpravkaForEmployee(EmployeeServiceModel employeeWork, DateTime StartDate, DateTime EndDate)
        {
            try
            {
                if (employeeWork.WorkedHoursByTaskByPeriod.Count < 1)
                {
                    TempData["Error"] = $"Няма задачи по които да е работил {employeeWork.FullName} за избрания период";
                    return RedirectToAction("Index", "Home");
                }
                var employeePeriodTasksAll = employeeWork.WorkedHoursByTaskByPeriod
                    .Select(wh => new SelectServiceModel
                    {
                        Id = wh.TaskId,
                        TextValue = wh.TaskName
                    })
                    .ToList();

                var employeePeriodTasks = employeePeriodTasksAll.GroupBy(l => new { l.Id, l.TextValue })   //Важно!!! Премахвам дублиращи се елементи в лист
                    .Select(d => new SelectServiceModel   
                    {
                        Id = d.First().Id,
                        TextValue = d.Key.TextValue
                    });

                var formula = string.Empty;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var stream = new System.IO.MemoryStream();
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Test");
                    worksheet.View.ZoomScale = 100;
                    try
                    {
                        worksheet.Name = employeeWork.FullName;
                    }
                    catch (Exception)
                    {
                        TempData["Error"] = "Грешка при създаването на tab за експерта: " + employeeWork.FullName.Substring(0, 50) + "...";
                        return RedirectToAction("Index", "Home");
                    }
                    worksheet.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Row(2).Height = 48;
                    var headerrange = worksheet.Cells[2, 1, 2, employeePeriodTasks.Count() + 1];
                    headerrange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headerrange.Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);
                    worksheet.Row(2).Style.Font.Size = 12;
                    worksheet.Column(1).Width = 12;
                    worksheet.Cells[2, 1].Value = "Дата";
                    int column = 2;
                    var tableTaskColumn = new Dictionary<int, int>();
                    foreach (var empTask in employeePeriodTasks.OrderBy(e => e.Id))
                    {
                        worksheet.Column(column).Width = 11;
                        worksheet.Cells[2, column].Style.WrapText = true;
                        worksheet.Cells[2, column].Style.Font.Size = 8;
                        worksheet.Cells[2, column].Value = empTask.TextValue;
                        worksheet.Cells[2, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[2, column].Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);
                        tableTaskColumn.Add(empTask.Id,column);
                        column += 1;
                    }
                    int col = column;
                    worksheet.Cells[2, column].Style.WrapText = true;
                    worksheet.Cells[2, column].Style.Font.Size = 12;
                    worksheet.Cells[2, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[2, column].Style.Fill.BackgroundColor.SetColor(Color.LightSteelBlue);
                    worksheet.Column(column).Width = 10;
                    worksheet.Cells[2, column].Value = "Общо за дата :";

                    worksheet.Cells[1, 1, 1, column > 7 ? column : 7].Merge = true;
                    worksheet.Row(1).Height = 25;
                    worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                    worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    worksheet.Cells[1, 1].Style.Indent = 10;
                    worksheet.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    //worksheet.Cells[1, 1].Value = employeeWork.FullName + "  (" + employeeWork.WorkedHoursByTaskByPeriod.Select(wh => wh.WorkDate.Date).FirstOrDefault().ToString("dd/MM/yyyy") +
                    //    "г. - " + employeeWork.WorkedHoursByTaskByPeriod.Select(wh => wh.WorkDate.Date).LastOrDefault().ToString("dd/MM/yyyy") + "г.)";
                    worksheet.Cells[1, 1].Value = employeeWork.FullName + "  (" + StartDate.Date.ToString("dd/MM/yyyy") + "г. - " + EndDate.Date.ToString("dd/MM/yyyy") + "г.)";
                    worksheet.Cells[1, 1].Style.Font.Size = 14;
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    worksheet.View.FreezePanes(400, 2);

                    column = 1;
                    int row = 2;
                    var tempDate = DateTime.Now.Date.AddDays(1);
                    foreach (var item in employeeWork.WorkedHoursByTaskByPeriod)
                    {
                        if (tempDate != item.WorkDate)
                        {
                            row += 1;
                            if (row > 2 && col > 2) //не сочи амфетката с имената на задачите и има поне една задача
                            {
                                formula = "=SUM(B" +row.ToString() +  ":" + (col <= 27 ? ((char)('A' + (col - 2))).ToString() : ("A" + ((char)('A' + (col - 28))).ToString())) + row.ToString() + ")";
                                worksheet.Cells[row, col].Formula = formula;
                                worksheet.Cells[row, col].Style.Font.Size = 12;
                                worksheet.Cells[row, col].Style.Font.Bold = true;
                                worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LightSteelBlue);
                                if (col > 15)
                                {
                                    worksheet.Cells[row, col + 1].Value = item.WorkDate.ToString("dd/MM/yyyy") + "г.";
                                    worksheet.Column(col + 1).Width = 10;
                                }


                            }
                            worksheet.Cells[row, column].Style.Font.Size = 10;
                            worksheet.Cells[row, column].Value = item.WorkDate.ToString("dd/MM/yyyy") + "г.";
                            worksheet.Cells[row, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[row, column].Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);
                            worksheet.Cells[row, tableTaskColumn[item.TaskId]].Value = item.HoursSpend;
                            tempDate = item.WorkDate;
                            
                        }
                        else
                        {
                            worksheet.Cells[row, tableTaskColumn[item.TaskId]].Value = item.HoursSpend;
                        }
                    }
                    row += 1;

                    worksheet.Cells[row, 1].Value = "Общо :";
                    worksheet.Cells[row, 1].Style.Font.Size = 11;
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.LightSteelBlue);

                    for (column = 2; column < col; column++)
                    {
                        formula = "=SUM(" + (column <= 26 ? ((char)('A' + (column - 1))).ToString() : ("A" + ((char)('A' + (column - 27))).ToString())) + "3:" + (column <= 26 ? ((char)('A' + (column - 1))).ToString() : ("A" + ((char)('A' + (column - 27))).ToString())) + (row - 1).ToString() + ")";

                        worksheet.Cells[row, column].Formula = formula;
                        worksheet.Cells[row, column].Style.Font.Size = 12;
                        worksheet.Cells[row, column].Style.Font.Bold = true;
                        worksheet.Cells[row, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, column].Style.Fill.BackgroundColor.SetColor(Color.LightSteelBlue);

                    }
                    var bodyrange = worksheet.Cells[3, 2, row, col];
                    bodyrange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    package.Save();
                }
                string fileName = "PersonalReport_" + DateTime.Now.ToShortTimeString() + ".xlsx";
                string fileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                stream.Position = 0;
                return File(stream, fileType, fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"[ExportSpravkaForEmployee] Грешка при експорта. {ex}";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> ExportReport(PeriodReportViewModel model)
        {
            try
            {
                if (model.EmployeesIds.Count() < 1)
                {
                    TempData["Error"] = "Няма включени експерти!";
                    return RedirectToAction("Index", "Home");
                }

                var tasksList = await this.tasks.ExportTasksAsync(model.EmployeesIds, model.StartDate, model.EndDate);

                if (tasksList.Count < 1)
                {
                    TempData["Error"] = "Няма задачи по които да е работено от избраните експерти за избрания период";
                    return RedirectToAction(nameof(PeriodReport));
                }

                //var selectedExperts = tasksList.FirstOrDefault().Colleagues.ToList();
                var allExpertsOnTasks = new HashSet<ReportUserServiceModel>();
                var allExperts = this.employees.GetEmployeesByList(model.EmployeesIds);

                tasksList.ForEach(t => t.Colleagues.ForEach(col => allExpertsOnTasks.Add(col)));  //списък на всички експерти по всички задачи
                var directorate = new SelectServiceModel()
                {
                    Id = allExpertsOnTasks.FirstOrDefault().DirectorateId ?? 0,
                };
                if (directorate.Id == 0)
                {
                    // TODO справката е за експерти назначени в дирекции, вкл директора, не се вкл председател, секретар, заместник председатели, затова не е необходима тази проверка
                }
                directorate.TextValue = this.directorates.GetDirectoratesNames(directorate.Id).FirstOrDefault().TextValue;   //дирекцията за която ще е отчета

                var departmentsList = this.departments.GetDepartmentsNamesByDirectorate(directorate.Id);  //отделите в дирекцията

                var sectorsList = await this.sectors.GetSectorsNamesByDirectorate(directorate.Id);

                var formula = string.Empty;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var stream = new System.IO.MemoryStream();
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    foreach (var department in departmentsList)
                    {     //allExperts.Select(s => s.DepartmentId).ToList().Distinct().Contains(department.Id)
                        if (allExperts.Where(s => s.DepartmentId == department.Id && s.SectorId == null).ToList().Count() > 0)   //ако има експерти в отдела
                        {
                            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Test");
                            worksheet.View.ZoomScale = 70;
                            try
                            {
                                // worksheet = package.Workbook.Worksheets.Add(department.TextValue);
                                worksheet.Name = department.TextValue;
                            }
                            catch (Exception)
                            {
                                TempData["Error"] = "Грешка при създаването на tab за отдел: " + department.TextValue.Substring(0, 50) + "...";
                                return RedirectToAction(nameof(PeriodReport));
                            }

                            worksheet.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            worksheet.Row(2).Height = 48;
                            var headerrange = worksheet.Cells[2, 1, 2, 5];
                            headerrange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            headerrange.Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);
                            worksheet.Row(2).Style.Font.Size = 12;
                            worksheet.Cells[2, 1].Value = "№";
                            worksheet.Column(1).Width = 10;
                            worksheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            //worksheet.Column(1).Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[2, 2].Value = "Наименование и пояснения към задачата";
                            worksheet.Column(2).Width = 60;
                            worksheet.Column(2).Style.WrapText = true;
                            //worksheet.Column(2).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //worksheet.Cells[2, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
                            worksheet.Cells[2, 3].Value = "Начална Дата";
                            worksheet.Column(3).Width = 15;
                            //worksheet.Column(3).Style.Numberformat.Format = "dd-MM-yyyy";
                            worksheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            worksheet.Cells[2, 4].Value = "Краен срок";
                            worksheet.Column(4).Width = 15;
                            //worksheet.Column(4).Style.Numberformat.Format = "dd-MM-yyyy";
                            worksheet.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            worksheet.Cells[2, 5].Value = "Описание \\ Резултати";
                            worksheet.Column(5).Width = 60;
                            //worksheet.Column(5).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //worksheet.Cells[2, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
                            worksheet.Column(5).Style.WrapText = true;
                            int column = 6;

                            var expertsInDepWithNoSectorIds = allExperts
                                .Where(e => e.DepartmentId == department.Id && e.SectorId == null)
                                .OrderBy(e => e.Id)
                                .Select(e => e.Id)
                                .ToHashSet();
                            foreach (var expert in allExperts.Where(e => e.DepartmentId == department.Id && e.SectorId == null).OrderBy(e => e.Id))
                            {
                                worksheet.Column(column).Width = 11;
                                worksheet.Cells[2, column].Style.WrapText = true;
                                worksheet.Cells[2, column].Style.Font.Size = 8;
                                worksheet.Cells[2, column].Value = expert.FullName;
                                worksheet.Cells[2, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[2, column].Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);
                                column += 1;
                            }
                            worksheet.Cells[2, column].Style.WrapText = true;
                            worksheet.Cells[2, column].Style.Font.Size = 8;
                            worksheet.Cells[2, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[2, column].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                            worksheet.Column(column).Width = 15;
                            worksheet.Cells[2, column].Value = "Общо колко часа е работено в/у задачата";
                            column += 1;
                            worksheet.Cells[2, column].Style.WrapText = true;
                            worksheet.Cells[2, column].Style.Font.Size = 8;
                            worksheet.Cells[2, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[2, column].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                            worksheet.Cells[2, column].Value = "Брой служители на задача";

                            worksheet.Cells[1, 1, 1, column].Merge = true;
                            worksheet.Row(1).Height = 25;
                            worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                            worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            worksheet.Cells[1, 1].Style.Indent = 10;
                            worksheet.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            worksheet.Cells[1, 1].Value = "Дирекция: \"" + directorate.TextValue + "\"  (" + model.StartDate.Date.ToString("dd/MM/yyyy") +
                                "г. - " + model.EndDate.Date.ToString("dd/MM/yyyy") + "г.)";
                            worksheet.Cells[1, 1].Style.Font.Size = 14;
                            worksheet.Cells[1, 1].Style.Font.Bold = true;
                            worksheet.View.FreezePanes(100, 6);
                            //дотук се създават първите два реда за отдела в екселския файл
                            var parentTaskId = 0;
                            var row = 3;
                            var workingHoursSpecificSum = 0;
                            var workingHoursProcurementSum = 0;
                            var workingHoursLearningSum = 0;
                            var workingHoursAdminSum = 0;
                            var workingHoursMeetingsSum = 0;
                            var workingHoursOtherSum = 0;
                            var taskExpertsColumn = 6;
                            foreach (var task in tasksList.Where(t => expertsInDepWithNoSectorIds.Overlaps(t.Colleagues.Select(e => e.Id).ToList())).OrderBy(t => t.ParentTaskId).ThenByDescending(t => t.TaskTypeName))
                            {
                                if (task.ParentTaskId != null && task.ParentTaskId != parentTaskId)
                                {
                                    TaskServiceModel parentTask = await this.tasks.GetParentTaskAsync(task.ParentTaskId.Value);
                                    if (parentTask != null)
                                    {
                                        worksheet.Row(row).Style.Font.Size = 12;
                                        worksheet.Row(row).Style.Font.Bold = true;
                                        worksheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                        worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.White);
                                        worksheet.Cells[row, 1].Value = parentTask.Id;
                                        worksheet.Cells[row, 2].Value = parentTask.TaskName.Length > 100 ? parentTask.TaskName.Substring(0, 100) + "..." : parentTask.TaskName;
                                        row += 1;
                                        parentTaskId = parentTask.Id;

                                    }
                                }

                                worksheet.Cells[row, 1].Value = task.Id;
                                worksheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[row, 2].Value = task.TaskName;
                                worksheet.Cells[row, 3].Value = task.StartDate.Date.ToString("dd-MM-yyyy") + "г.";
                                worksheet.Cells[row, 4].Value = task.TaskStatusName == TaskStatusClosed ? task.EndDate.Value.Date.ToString("dd-MM-yyyy") + "г." : task.EndDatePrognose.Value.Date.ToString("dd-MM-yyyy") + "г.";
                                worksheet.Cells[row, 5].Value = task.Description;

                                taskExpertsColumn = 6;
                                foreach (var eId in expertsInDepWithNoSectorIds)
                                {
                                    var curentColegue = task.Colleagues.Where(cl => cl.Id == eId).FirstOrDefault();
                                    if (curentColegue != null)
                                    {
                                        worksheet.Cells[row, taskExpertsColumn].Value = curentColegue.TaskWorkedHours;
                                    }
                                    taskExpertsColumn += 1;
                                }
                                int workedHoursByDepartment = task.Colleagues.Where(cl => expertsInDepWithNoSectorIds.Contains(cl.Id)).Sum(cl => cl.TaskWorkedHours ?? 0);
                                formula = "=SUM(F" + (row).ToString() + ":" + (taskExpertsColumn <= 27 ? ((char)('A' + (taskExpertsColumn - 2))).ToString() : ("A" + ((char)('A' + (taskExpertsColumn - 28))).ToString())) + (row).ToString() + ")";  //общо часове по задачата
                                worksheet.Cells[row, taskExpertsColumn].Formula = formula;

                                // worksheet.Cells[row, taskExpertsColumn].Formula = "=SUM(F" + (row).ToString() + ":" + ((char)('A' + (taskExpertsColumn - 2))).ToString() + (row).ToString() + ")";  //общо часове по задачата
                                worksheet.Cells[row, taskExpertsColumn].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[row, taskExpertsColumn].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                                formula = "=COUNTA(F" + (row).ToString() + ":" + (taskExpertsColumn <= 27 ? ((char)('A' + (taskExpertsColumn - 2))).ToString() : ("A" + ((char)('A' + (taskExpertsColumn - 28))).ToString())) + (row).ToString() + ")";  //общо eksperti по задачата
                                worksheet.Cells[row, taskExpertsColumn + 1].Formula = formula;
                                //worksheet.Cells[row, taskExpertsColumn + 1].Formula = "=COUNTA(F" + (row).ToString() + ":" + ((char)('A' + (taskExpertsColumn - 2))).ToString() + (row).ToString() + ")";  //общо eksperti по задачата
                                worksheet.Cells[row, taskExpertsColumn + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[row, taskExpertsColumn + 1].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);

                                if (task.TaskTypeName == TaskTypeSpecificWork)
                                {
                                    workingHoursSpecificSum += workedHoursByDepartment;
                                    worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);
                                }
                                else if (task.TaskTypeName == TaskTypeProcurement)
                                {
                                    workingHoursProcurementSum += workedHoursByDepartment;
                                    worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.LightSeaGreen);
                                }
                                else if (task.TaskTypeName == TaskTypeLearning)
                                {
                                    workingHoursLearningSum += workedHoursByDepartment;
                                    worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGoldenrodYellow);
                                }
                                else if (task.TaskTypeName == TaskTypeAdminActivity)
                                {
                                    workingHoursAdminSum += workedHoursByDepartment;
                                    worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.Bisque);
                                }
                                else if (task.TaskTypeName == TaskTypeMeetings)
                                {
                                    workingHoursMeetingsSum += workedHoursByDepartment;
                                    worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.LightSalmon);
                                }
                                else if (task.TaskTypeName == TaskTypeOther)
                                {
                                    workingHoursOtherSum += workedHoursByDepartment;
                                    worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                                }

                                row += 1;
                            }
                            var modelTable = worksheet.Cells[2, 1, row, column];
                            modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            modelTable.Style.Border.Left.Style = ExcelBorderStyle.Thin;             //border за клетките около задачите и верикално позициониране
                            modelTable.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            modelTable.Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                            worksheet.Cells[row, 5].Value = "Брой задачи по които е работено";
                            for (int col = 6; col < taskExpertsColumn; col++)   // taskExpertsColumn сочи колоната "Общо колко часа е работено..."
                            {
                                formula = "=COUNTA(" + (col <= 26 ? ((char)('A' + (col - 1))).ToString() : ("A" + ((char)('A' + (col - 27))).ToString())) + "3:" + (col <= 26 ? ((char)('A' + (col - 1))).ToString() : ("A" + ((char)('A' + (col - 27))).ToString())) + (row - 1).ToString() + ")";

                                worksheet.Cells[row, col].Formula = formula;

                                //worksheet.Cells[row, col].Formula = "=COUNTA(" + ((char)('A' + (col - 1))).ToString() + "3:" + ((char)('A' + (col - 1))).ToString() + (row - 1).ToString() + ")";
                            }

                            formula = "=AVERAGE(F" + (row).ToString() + ":" + (taskExpertsColumn <= 27 ? ((char)('A' + (taskExpertsColumn - 2))).ToString() : ("A" + ((char)('A' + (taskExpertsColumn - 28))).ToString())) + (row).ToString() + ")";
                            worksheet.Cells[row, taskExpertsColumn].Formula = formula; //по колко задачи средно е работено

                            //worksheet.Cells[row, taskExpertsColumn].Formula = "=AVERAGE(F" + (row).ToString() + ":" + ((char)('A' + (taskExpertsColumn - 2))).ToString() + (row).ToString() + ")";     //по колко задачи средно е работено
                            worksheet.Cells[row, taskExpertsColumn].Style.Numberformat.Format = "0.00";
                            var modelTableBroiZadachi = worksheet.Cells[row, 5, row, taskExpertsColumn];
                            modelTableBroiZadachi.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            modelTableBroiZadachi.Style.Border.Left.Style = ExcelBorderStyle.Thin;       //border за клетките около броя задачи
                            modelTableBroiZadachi.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            modelTableBroiZadachi.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            modelTableBroiZadachi.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            modelTableBroiZadachi.Style.Fill.BackgroundColor.SetColor(Color.Orange);
                            //worksheet.Cells[row, taskExpertsColumn + 1].Formula = "=AVERAGE(" + ((char)('A' + (taskExpertsColumn))).ToString() + "3:" + ((char)('A' + (taskExpertsColumn))).ToString() + (row - 1).ToString() + ")";
                            formula = "=AVERAGE(" + (taskExpertsColumn <= 25 ? ((char)('A' + (taskExpertsColumn))).ToString() : ("A" + ((char)('A' + (taskExpertsColumn - 26))).ToString())) + "3:" + (taskExpertsColumn <= 25 ? ((char)('A' + (taskExpertsColumn))).ToString() : ("A" + ((char)('A' + (taskExpertsColumn - 26))).ToString())) + (row - 1).ToString() + ")";   // ако има повече от 19 човека в сектора се променя генерацията на колоната
                            worksheet.Cells[row, taskExpertsColumn + 1].Formula = formula;

                            worksheet.Cells[row, taskExpertsColumn + 1].Style.Numberformat.Format = "0.00";
                            worksheet.Cells[row, taskExpertsColumn + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[row, taskExpertsColumn + 1].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);  //средно колко човека са работили по задача

                            row += 1;
                            worksheet.Cells[row, 5].Value = "Общо часове на служителя";
                            for (int col = 6; col < taskExpertsColumn; col++)   // taskExpertsColumn сочи колоната "Общо колко часа е работено..."
                            {
                                formula = "=SUM(" + (col <= 26 ? ((char)('A' + (col - 1))).ToString() : ("A" + ((char)('A' + (col - 27))).ToString())) + "3:" + (col <= 26 ? ((char)('A' + (col - 1))).ToString() : ("A" + ((char)('A' + (col - 27))).ToString())) + (row - 2).ToString() + ")";

                                worksheet.Cells[row, col].Formula = formula;

                                //worksheet.Cells[row, col].Formula = "=SUM(" + ((char)('A' + (col - 1))).ToString() + "3:" + ((char)('A' + (col - 1))).ToString() + (row - 2).ToString() + ")";
                            }
                            var modelTableBroiChasove = worksheet.Cells[row, 5, row, taskExpertsColumn - 1];
                            modelTableBroiChasove.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            modelTableBroiChasove.Style.Border.Left.Style = ExcelBorderStyle.Thin;       //border за клетките около броя часове на служителите
                            modelTableBroiChasove.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            modelTableBroiChasove.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            modelTableBroiChasove.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            modelTableBroiChasove.Style.Fill.BackgroundColor.SetColor(Color.Bisque);

                            row += 1;
                            worksheet.Cells[row, 5].Value = "Общо часове на всички служители";
                            formula = "=SUM(F" + (row - 1).ToString() + ":" + (taskExpertsColumn <= 27 ? ((char)('A' + (taskExpertsColumn - 2))).ToString() : ("A" + ((char)('A' + (taskExpertsColumn - 28))).ToString())) + (row - 1).ToString() + ")";  //ОБЩО ЧАСОВЕ
                            worksheet.Cells[row, 6].Formula = formula;
                            // worksheet.Cells[row, 6].Formula = "=SUM(F" + (row - 1).ToString() + ":" + ((char)('A' + (taskExpertsColumn - 2))).ToString() + (row - 1).ToString() + ")";  //ОБЩО ЧАСОВЕ

                            var modelTableSumHours = worksheet.Cells[row, 5, row, 6];
                            modelTableSumHours.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            modelTableSumHours.Style.Border.Left.Style = ExcelBorderStyle.Thin;       //border за клетките Общо часове
                            modelTableSumHours.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            modelTableSumHours.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            modelTableSumHours.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            modelTableSumHours.Style.Fill.BackgroundColor.SetColor(Color.White);
                            modelTableSumHours.Style.Font.Size = 12;
                            modelTableSumHours.Style.Font.Bold = true;

                            for (int col = 6; col <= taskExpertsColumn + 1; col++)
                            {
                                worksheet.Column(col).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            }

                            row += 2;
                            var totalHoursWorckedByType = workingHoursSpecificSum + workingHoursProcurementSum + workingHoursLearningSum + workingHoursAdminSum +
                                workingHoursMeetingsSum + workingHoursOtherSum;

                            if (totalHoursWorckedByType > 0)
                            {
                                var modelTablePercentage = worksheet.Cells[row, 2, row + 6, 3];
                                modelTablePercentage.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                modelTablePercentage.Style.Border.Left.Style = ExcelBorderStyle.Thin;       //border за клетките около процентите
                                modelTablePercentage.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                modelTablePercentage.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                modelTablePercentage.Style.Fill.PatternType = ExcelFillStyle.Solid;


                                worksheet.Cells[row, 2].Value = TaskTypeSpecificWork;
                                worksheet.Cells[row, 3].Value = workingHoursSpecificSum;
                                //worksheet.Cells[row, 4].Value = totalHoursWorckedByType != 0 ? ((double)workingHoursSpecificSum / totalHoursWorckedByType).ToString("0.00%") : "0%";
                                worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);

                                row += 1;
                                worksheet.Cells[row, 2].Value = TaskTypeProcurement;
                                worksheet.Cells[row, 3].Value = workingHoursProcurementSum;
                                //worksheet.Cells[row, 4].Value = totalHoursWorckedByType != 0 ? ((double)workingHoursProcurementSum / totalHoursWorckedByType).ToString("0.00%") : "0%";
                                worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.LightSeaGreen);

                                row += 1;
                                worksheet.Cells[row, 2].Value = TaskTypeLearning;
                                worksheet.Cells[row, 3].Value = workingHoursLearningSum;
                                //worksheet.Cells[row, 4].Value = totalHoursWorckedByType != 0 ? ((double)workingHoursLearningSum / totalHoursWorckedByType).ToString("0.00%") : "0%";
                                worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.LightGoldenrodYellow);

                                row += 1;
                                worksheet.Cells[row, 2].Value = TaskTypeAdminActivity;
                                worksheet.Cells[row, 3].Value = workingHoursAdminSum;
                                //worksheet.Cells[row, 4].Value = totalHoursWorckedByType != 0 ? ((double)workingHoursAdminSum / totalHoursWorckedByType).ToString("0.00%") : "0%";
                                worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.Bisque);

                                row += 1;
                                worksheet.Cells[row, 2].Value = TaskTypeMeetings;
                                worksheet.Cells[row, 3].Value = workingHoursMeetingsSum;
                                //worksheet.Cells[row, 4].Value = totalHoursWorckedByType != 0 ? ((double)workingHoursMeetingsSum / totalHoursWorckedByType).ToString("0.00%") : "0%";
                                worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.LightSalmon);

                                row += 1;
                                worksheet.Cells[row, 2].Value = TaskTypeOther;
                                worksheet.Cells[row, 3].Value = workingHoursOtherSum;
                                //worksheet.Cells[row, 4].Value = totalHoursWorckedByType != 0 ? ((double)workingHoursOtherSum / totalHoursWorckedByType).ToString("0.00%") : "0%";
                                worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                                row += 1;
                                worksheet.Cells[row, 2].Value = "Общо часове";
                                //worksheet.Cells[row, 3].Formula = "=SUM(C"+(row-6).ToString()+":C"+(row-1).ToString()+")";
                                worksheet.Cells[row, 3].Value = totalHoursWorckedByType;
                                //worksheet.Cells[row, 4].Value = "100%";
                                worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.White);
                                worksheet.Cells[row, 2, row, 3].Style.Font.Bold = true;

                                //Графики начало
                                ExcelPieChart pieChart = worksheet.Drawings.AddChart("pieChart", eChartType.Pie3D) as ExcelPieChart;

                                pieChart.Title.Text = "N: Задача / Часове";
                                //select the ranges for the pie. First the values, then the header range
                                pieChart.Legend.Position = eLegendPosition.Bottom;
                                pieChart.DataLabel.ShowPercent = true;
                                pieChart.DataLabel.ShowLeaderLines = true;
                                pieChart.DataLabel.ShowCategory = true;
                                //pieChart.ShowDataLabelsOverMaximum = true;
                                pieChart.Legend.Remove();
                                var rangeTitles = ExcelRange.GetAddress(3, 1, row - 11, 1);
                                if ((row - 13) < 14)  // броя задачи променя дали имената на задачите се вземат или номерата им
                                {
                                    rangeTitles = ExcelRange.GetAddress(3, 2, row - 11, 2);
                                }
                                pieChart.Series.Add(ExcelRange.GetAddress(3, taskExpertsColumn, row - 11, taskExpertsColumn), rangeTitles);
                                pieChart.SetSize(600, 500);
                                pieChart.SetPosition(row - 7, 0, 6, 0);
                                pieChart.StyleManager.SetChartStyle(ePresetChartStyle.Pie3dChartStyle8, ePresetChartColors.ColorfulPalette3);
                                pieChart.DisplayBlanksAs = eDisplayBlanksAs.Gap;
                                pieChart.DataLabel.Font.Fill.Color = Color.Black;
                                pieChart.DataLabel.Font.Size = 12;
                                //create a new piechart of type Doughnut
                                var doughtnutChart = worksheet.Drawings.AddChart("crtExtensionCount", eChartType.DoughnutExploded) as ExcelDoughnutChart;
                                //Set position to row 1 column 7 and 16 pixels offset
                                doughtnutChart.SetPosition(row - 7, 0, 3, 10);
                                doughtnutChart.SetSize(500, 500);
                                doughtnutChart.Series.Add(ExcelRange.GetAddress(row - 6, 3, row - 1, 3), ExcelRange.GetAddress(row - 6, 2, row - 1, 2));
                                doughtnutChart.Title.Text = "ТИП ЗАДАЧА / ЧАСОВЕ";
                                doughtnutChart.DataLabel.ShowPercent = true;
                                //doughtnutChart.DataLabel.ShowLeaderLines = true;
                                doughtnutChart.Style = eChartStyle.Style26; //3D look
                            }
                        }
                    }
                    foreach (var sector in sectorsList)
                    {
                        if (allExperts.Where(s => s.SectorId == sector.Id).ToList().Count() > 0)   //ако има експерти в sectora
                        {
                            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Test");
                            worksheet.View.ZoomScale = 70;
                            try
                            {
                                // worksheet = package.Workbook.Worksheets.Add(department.TextValue);
                                worksheet.Name = sector.TextValue;
                            }
                            catch (Exception)
                            {
                                TempData["Error"] = "Грешка при създаването на tab за сектор: " + sector.TextValue.Substring(0, 50) + "...";
                                return RedirectToAction(nameof(PeriodReport));
                            }

                            worksheet.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            worksheet.Row(2).Height = 48;
                            var headerrange = worksheet.Cells[2, 1, 2, 5];
                            headerrange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            headerrange.Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);
                            worksheet.Row(2).Style.Font.Size = 12;
                            worksheet.Cells[2, 1].Value = "№";
                            worksheet.Column(1).Width = 10;
                            worksheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            //worksheet.Column(1).Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[2, 2].Value = "Наименование и пояснения към задачата";
                            worksheet.Column(2).Width = 60;
                            worksheet.Column(2).Style.WrapText = true;
                            //worksheet.Column(2).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //worksheet.Cells[2, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
                            worksheet.Cells[2, 3].Value = "Начална Дата";
                            worksheet.Column(3).Width = 15;
                            //worksheet.Column(3).Style.Numberformat.Format = "dd-MM-yyyy";
                            worksheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            worksheet.Cells[2, 4].Value = "Краен срок";
                            worksheet.Column(4).Width = 15;
                            //worksheet.Column(4).Style.Numberformat.Format = "dd-MM-yyyy";
                            worksheet.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            worksheet.Cells[2, 5].Value = "Описание \\ Резултати";
                            worksheet.Column(5).Width = 60;
                            //worksheet.Column(5).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //worksheet.Cells[2, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
                            worksheet.Column(5).Style.WrapText = true;
                            int column = 6;

                            var expertsInSector = allExperts
                                .Where(e => e.SectorId == sector.Id)
                                .OrderBy(e => e.Id)
                                .Select(e => e.Id)
                                .ToHashSet();
                            foreach (var expert in allExperts.Where(e => e.SectorId == sector.Id).OrderBy(e => e.Id))
                            {
                                worksheet.Column(column).Width = 11;
                                worksheet.Cells[2, column].Style.WrapText = true;
                                worksheet.Cells[2, column].Style.Font.Size = 8;
                                worksheet.Cells[2, column].Value = expert.FullName;
                                worksheet.Cells[2, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[2, column].Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);
                                column += 1;
                            }
                            worksheet.Cells[2, column].Style.WrapText = true;
                            worksheet.Cells[2, column].Style.Font.Size = 8;
                            worksheet.Cells[2, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[2, column].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                            worksheet.Column(column).Width = 15;
                            worksheet.Cells[2, column].Value = "Общо колко часа е работено в/у задачата";
                            column += 1;
                            worksheet.Cells[2, column].Style.WrapText = true;
                            worksheet.Cells[2, column].Style.Font.Size = 8;
                            worksheet.Cells[2, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[2, column].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                            worksheet.Cells[2, column].Value = "Брой служители на задача";

                            worksheet.Cells[1, 1, 1, column].Merge = true;
                            worksheet.Row(1).Height = 25;
                            worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                            worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            worksheet.Cells[1, 1].Style.Indent = 10;
                            worksheet.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            worksheet.Cells[1, 1].Value = "Дирекция: \"" + directorate.TextValue + "\"  (" + model.StartDate.Date.ToString("dd/MM/yyyy") +
                                "г. - " + model.EndDate.Date.ToString("dd/MM/yyyy") + "г.)";
                            worksheet.Cells[1, 1].Style.Font.Size = 14;
                            worksheet.Cells[1, 1].Style.Font.Bold = true;
                            worksheet.View.FreezePanes(100, 6);
                            //дотук се създават първите два реда за сектора в екселския файл
                            var parentTaskId = 0;
                            var row = 3;
                            var workingHoursSpecificSum = 0;
                            var workingHoursProcurementSum = 0;
                            var workingHoursLearningSum = 0;
                            var workingHoursAdminSum = 0;
                            var workingHoursMeetingsSum = 0;
                            var workingHoursOtherSum = 0;
                            var taskExpertsColumn = 6;
                            foreach (var task in tasksList.Where(t => expertsInSector.Overlaps(t.Colleagues.Select(e => e.Id).ToList())).OrderBy(t => t.ParentTaskId).ThenByDescending(t => t.TaskTypeName))
                            {
                                if (task.ParentTaskId != null && task.ParentTaskId != parentTaskId)
                                {
                                    TaskServiceModel parentTask = await this.tasks.GetParentTaskAsync(task.ParentTaskId.Value);
                                    if (parentTask != null)
                                    {
                                        worksheet.Row(row).Style.Font.Size = 12;
                                        worksheet.Row(row).Style.Font.Bold = true;
                                        worksheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                        worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.White);
                                        worksheet.Cells[row, 1].Value = parentTask.Id;
                                        worksheet.Cells[row, 2].Value = parentTask.TaskName;
                                        row += 1;
                                        parentTaskId = parentTask.Id;

                                    }
                                }

                                worksheet.Cells[row, 1].Value = task.Id;
                                worksheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[row, 2].Value = task.TaskName;
                                worksheet.Cells[row, 3].Value = task.StartDate.Date.ToString("dd-MM-yyyy") + "г.";
                                worksheet.Cells[row, 4].Value = task.TaskStatusName == TaskStatusClosed ? task.EndDate.Value.Date.ToString("dd-MM-yyyy") + "г." : task.EndDatePrognose.Value.Date.ToString("dd-MM-yyyy") + "г.";
                                worksheet.Cells[row, 5].Value = task.Description;

                                taskExpertsColumn = 6;
                                foreach (var eId in expertsInSector)
                                {
                                    var curentColegue = task.Colleagues.Where(cl => cl.Id == eId).FirstOrDefault();
                                    if (curentColegue != null)
                                    {
                                        worksheet.Cells[row, taskExpertsColumn].Value = curentColegue.TaskWorkedHours;
                                    }
                                    taskExpertsColumn += 1;
                                }
                                int workedHoursByDepartment = task.Colleagues.Where(cl => expertsInSector.Contains(cl.Id)).Sum(cl => cl.TaskWorkedHours ?? 0);
                                formula = "=SUM(F" + (row).ToString() + ":" + (taskExpertsColumn <= 27 ? ((char)('A' + (taskExpertsColumn - 2))).ToString() : ("A" + ((char)('A' + (taskExpertsColumn - 28))).ToString())) + (row).ToString() + ")";  //общо часове по задачата
                                worksheet.Cells[row, taskExpertsColumn].Formula = formula;
                                //worksheet.Cells[row, taskExpertsColumn].Formula = "=SUM(F" + (row).ToString() + ":" + ((char)('A' + (taskExpertsColumn - 2))).ToString() + (row).ToString() + ")";  //общо часове по задачата
                                worksheet.Cells[row, taskExpertsColumn].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[row, taskExpertsColumn].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                                formula = "=COUNTA(F" + (row).ToString() + ":" + (taskExpertsColumn <= 27 ? ((char)('A' + (taskExpertsColumn - 2))).ToString() : ("A" + ((char)('A' + (taskExpertsColumn - 28))).ToString())) + (row).ToString() + ")";  //общо eksperti по задачата
                                worksheet.Cells[row, taskExpertsColumn + 1].Formula = formula;

                                //worksheet.Cells[row, taskExpertsColumn + 1].Formula = "=COUNTA(F" + (row).ToString() + ":" + ((char)('A' + (taskExpertsColumn - 2))).ToString() + (row).ToString() + ")";  //общо eksperti по задачата
                                worksheet.Cells[row, taskExpertsColumn + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[row, taskExpertsColumn + 1].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);

                                if (task.TaskTypeName == TaskTypeSpecificWork)
                                {
                                    workingHoursSpecificSum += workedHoursByDepartment;
                                    worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);
                                }
                                else if (task.TaskTypeName == TaskTypeProcurement)
                                {
                                    workingHoursProcurementSum += workedHoursByDepartment;
                                    worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.LightSeaGreen);
                                }
                                else if (task.TaskTypeName == TaskTypeLearning)
                                {
                                    workingHoursLearningSum += workedHoursByDepartment;
                                    worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGoldenrodYellow);
                                }
                                else if (task.TaskTypeName == TaskTypeAdminActivity)
                                {
                                    workingHoursAdminSum += workedHoursByDepartment;
                                    worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.Bisque);
                                }
                                else if (task.TaskTypeName == TaskTypeMeetings)
                                {
                                    workingHoursMeetingsSum += workedHoursByDepartment;
                                    worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.LightSalmon);
                                }
                                else if (task.TaskTypeName == TaskTypeOther)
                                {
                                    workingHoursOtherSum += workedHoursByDepartment;
                                    worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                                }

                                row += 1;
                            }
                            var modelTable = worksheet.Cells[2, 1, row, column];
                            modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            modelTable.Style.Border.Left.Style = ExcelBorderStyle.Thin;             //border за клетките около задачите и верикално позициониране
                            modelTable.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            modelTable.Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                            worksheet.Cells[row, 5].Value = "Брой задачи по които е работено";
                            for (int col = 6; col < taskExpertsColumn; col++)   // taskExpertsColumn сочи колоната "Общо колко часа е работено..."
                            {
                                formula = "=COUNTA(" + (col <= 26 ? ((char)('A' + (col - 1))).ToString() : ("A" + ((char)('A' + (col - 27))).ToString())) + "3:" + (col <= 26 ? ((char)('A' + (col - 1))).ToString() : ("A" + ((char)('A' + (col - 27))).ToString())) + (row - 1).ToString() + ")";

                                worksheet.Cells[row, col].Formula = formula;
                                //worksheet.Cells[row, col].Formula = "=COUNTA(" + ((char)('A' + (col - 1))).ToString() + "3:" + ((char)('A' + (col - 1))).ToString() + (row - 1).ToString() + ")";
                            }
                            formula = "=AVERAGE(F" + (row).ToString() + ":" + (taskExpertsColumn <= 27 ? ((char)('A' + (taskExpertsColumn - 2))).ToString() : ("A" + ((char)('A' + (taskExpertsColumn - 28))).ToString())) + (row).ToString() + ")";
                            worksheet.Cells[row, taskExpertsColumn].Formula = formula; //по колко задачи средно е работено

                            //worksheet.Cells[row, taskExpertsColumn].Formula = "=AVERAGE(F" + (row).ToString() + ":" + ((char)('A' + (taskExpertsColumn - 2))).ToString() + (row).ToString() + ")";     //по колко задачи средно е работено
                            worksheet.Cells[row, taskExpertsColumn].Style.Numberformat.Format = "0.00";
                            var modelTableBroiZadachi = worksheet.Cells[row, 5, row, taskExpertsColumn];
                            modelTableBroiZadachi.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            modelTableBroiZadachi.Style.Border.Left.Style = ExcelBorderStyle.Thin;       //border за клетките около броя задачи
                            modelTableBroiZadachi.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            modelTableBroiZadachi.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            modelTableBroiZadachi.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            modelTableBroiZadachi.Style.Fill.BackgroundColor.SetColor(Color.Orange);
                            formula = "=AVERAGE(" + (taskExpertsColumn <= 25 ? ((char)('A' + (taskExpertsColumn))).ToString() : ("A" + ((char)('A' + (taskExpertsColumn - 26))).ToString())) + "3:" + (taskExpertsColumn <= 25 ? ((char)('A' + (taskExpertsColumn))).ToString() : ("A" + ((char)('A' + (taskExpertsColumn - 26))).ToString())) + (row - 1).ToString() + ")";   // ако има повече от 19 човека в сектора се променя генерацията на колоната
                            worksheet.Cells[row, taskExpertsColumn + 1].Formula = formula;
                            worksheet.Cells[row, taskExpertsColumn + 1].Style.Numberformat.Format = "0.00";
                            worksheet.Cells[row, taskExpertsColumn + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[row, taskExpertsColumn + 1].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);  //средно колко човека са работили по задача

                            row += 1;
                            worksheet.Cells[row, 5].Value = "Общо часове на служителя";
                            for (int col = 6; col < taskExpertsColumn; col++)   // taskExpertsColumn сочи колоната "Общо колко часа е работено..."
                            {
                                formula = "=SUM(" + (col <= 26 ? ((char)('A' + (col - 1))).ToString() : ("A" + ((char)('A' + (col - 27))).ToString())) + "3:" + (col <= 26 ? ((char)('A' + (col - 1))).ToString() : ("A" + ((char)('A' + (col - 27))).ToString())) + (row - 2).ToString() + ")";

                                worksheet.Cells[row, col].Formula = formula;

                                //worksheet.Cells[row, col].Formula = "=SUM(" + ((char)('A' + (col - 1))).ToString() + "3:" + ((char)('A' + (col - 1))).ToString() + (row - 2).ToString() + ")";
                            }
                            var modelTableBroiChasove = worksheet.Cells[row, 5, row, taskExpertsColumn - 1];
                            modelTableBroiChasove.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            modelTableBroiChasove.Style.Border.Left.Style = ExcelBorderStyle.Thin;       //border за клетките около броя часове на служителите
                            modelTableBroiChasove.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            modelTableBroiChasove.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            modelTableBroiChasove.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            modelTableBroiChasove.Style.Fill.BackgroundColor.SetColor(Color.Bisque);

                            row += 1;
                            worksheet.Cells[row, 5].Value = "Общо часове на всички служители";
                            formula = "=SUM(F" + (row - 1).ToString() + ":" + (taskExpertsColumn <= 27 ? ((char)('A' + (taskExpertsColumn - 2))).ToString() : ("A" + ((char)('A' + (taskExpertsColumn - 28))).ToString())) + (row - 1).ToString() + ")";  //ОБЩО ЧАСОВЕ
                            worksheet.Cells[row, 6].Formula = formula;
                            //worksheet.Cells[row, 6].Formula = "=SUM(F" + (row - 1).ToString() + ":" +( ((char)('A' + (taskExpertsColumn - 2))).ToString() ) + (row - 1).ToString() + ")";  //ОБЩО ЧАСОВЕ

                            var modelTableSumHours = worksheet.Cells[row, 5, row, 6];
                            modelTableSumHours.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            modelTableSumHours.Style.Border.Left.Style = ExcelBorderStyle.Thin;       //border за клетките Общо часове
                            modelTableSumHours.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            modelTableSumHours.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            modelTableSumHours.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            modelTableSumHours.Style.Fill.BackgroundColor.SetColor(Color.White);
                            modelTableSumHours.Style.Font.Size = 12;
                            modelTableSumHours.Style.Font.Bold = true;

                            for (int col = 6; col <= taskExpertsColumn + 1; col++)
                            {
                                worksheet.Column(col).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            }

                            row += 2;
                            var totalHoursWorckedByType = workingHoursSpecificSum + workingHoursProcurementSum + workingHoursLearningSum + workingHoursAdminSum +
                                workingHoursMeetingsSum + workingHoursOtherSum;

                            if (totalHoursWorckedByType > 0)
                            {
                                var modelTablePercentage = worksheet.Cells[row, 2, row + 6, 3];
                                modelTablePercentage.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                modelTablePercentage.Style.Border.Left.Style = ExcelBorderStyle.Thin;       //border за клетките около процентите
                                modelTablePercentage.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                modelTablePercentage.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                modelTablePercentage.Style.Fill.PatternType = ExcelFillStyle.Solid;


                                worksheet.Cells[row, 2].Value = TaskTypeSpecificWork;
                                worksheet.Cells[row, 3].Value = workingHoursSpecificSum;
                                //worksheet.Cells[row, 4].Value = totalHoursWorckedByType != 0 ? ((double)workingHoursSpecificSum / totalHoursWorckedByType).ToString("0.00%") : "0%";
                                worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);

                                row += 1;
                                worksheet.Cells[row, 2].Value = TaskTypeProcurement;
                                worksheet.Cells[row, 3].Value = workingHoursProcurementSum;
                                //worksheet.Cells[row, 4].Value = totalHoursWorckedByType != 0 ? ((double)workingHoursProcurementSum / totalHoursWorckedByType).ToString("0.00%") : "0%";
                                worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.LightSeaGreen);

                                row += 1;
                                worksheet.Cells[row, 2].Value = TaskTypeLearning;
                                worksheet.Cells[row, 3].Value = workingHoursLearningSum;
                                //worksheet.Cells[row, 4].Value = totalHoursWorckedByType != 0 ? ((double)workingHoursLearningSum / totalHoursWorckedByType).ToString("0.00%") : "0%";
                                worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.LightGoldenrodYellow);

                                row += 1;
                                worksheet.Cells[row, 2].Value = TaskTypeAdminActivity;
                                worksheet.Cells[row, 3].Value = workingHoursAdminSum;
                                //worksheet.Cells[row, 4].Value = totalHoursWorckedByType != 0 ? ((double)workingHoursAdminSum / totalHoursWorckedByType).ToString("0.00%") : "0%";
                                worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.Bisque);

                                row += 1;
                                worksheet.Cells[row, 2].Value = TaskTypeMeetings;
                                worksheet.Cells[row, 3].Value = workingHoursMeetingsSum;
                                //worksheet.Cells[row, 4].Value = totalHoursWorckedByType != 0 ? ((double)workingHoursMeetingsSum / totalHoursWorckedByType).ToString("0.00%") : "0%";
                                worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.LightSalmon);

                                row += 1;
                                worksheet.Cells[row, 2].Value = TaskTypeOther;
                                worksheet.Cells[row, 3].Value = workingHoursOtherSum;
                                //worksheet.Cells[row, 4].Value = totalHoursWorckedByType != 0 ? ((double)workingHoursOtherSum / totalHoursWorckedByType).ToString("0.00%") : "0%";
                                worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                                row += 1;
                                worksheet.Cells[row, 2].Value = "Общо часове";
                                //worksheet.Cells[row, 3].Formula = "=SUM(C"+(row-6).ToString()+":C"+(row-1).ToString()+")";
                                worksheet.Cells[row, 3].Value = totalHoursWorckedByType;
                                //worksheet.Cells[row, 4].Value = "100%";
                                worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.White);
                                worksheet.Cells[row, 2, row, 3].Style.Font.Bold = true;

                                //Графики начало
                                ExcelPieChart pieChart = worksheet.Drawings.AddChart("pieChart", eChartType.Pie3D) as ExcelPieChart;
                                pieChart.Title.Text = "N: Задача / Часове";
                                //select the ranges for the pie. First the values, then the header range
                                pieChart.Legend.Position = eLegendPosition.Bottom;
                                pieChart.DataLabel.ShowPercent = true;
                                pieChart.DataLabel.ShowLeaderLines = true;
                                pieChart.DataLabel.ShowCategory = true;
                                pieChart.Legend.Remove();
                                var rangeTitles = ExcelRange.GetAddress(3, 1, row - 11, 1);
                                if ((row - 13) < 14)  // броя задачи променя дали имената на задачите се вземат или номерата им
                                {
                                    rangeTitles = ExcelRange.GetAddress(3, 2, row - 11, 2);
                                }
                                pieChart.Series.Add(ExcelRange.GetAddress(3, taskExpertsColumn, row - 11, taskExpertsColumn), rangeTitles);
                                pieChart.SetSize(600, 500);
                                pieChart.SetPosition(row - 7, 0, 6, 0);
                                pieChart.StyleManager.SetChartStyle(ePresetChartStyle.Pie3dChartStyle8, ePresetChartColors.ColorfulPalette3);
                                pieChart.DisplayBlanksAs = eDisplayBlanksAs.Gap;
                                pieChart.DataLabel.Font.Fill.Color = Color.Black;
                                pieChart.DataLabel.Font.Size = 12;
                                //pieChart.StyleManager.SetChartStyle(ePresetChartStyle.Pie3dChartStyle7);

                                //create a new piechart of type Doughnut
                                var doughtnutChart = worksheet.Drawings.AddChart("crtExtensionCount", eChartType.DoughnutExploded) as ExcelDoughnutChart;
                                //Set position to row 1 column 7 and 16 pixels offset
                                doughtnutChart.SetPosition(row - 7, 0, 3, 10);
                                doughtnutChart.SetSize(500, 500);
                                doughtnutChart.Series.Add(ExcelRange.GetAddress(row - 6, 3, row - 1, 3), ExcelRange.GetAddress(row - 6, 2, row - 1, 2));
                                doughtnutChart.Title.Text = "ТИП ЗАДАЧА / ЧАСОВЕ";
                                doughtnutChart.DataLabel.ShowPercent = true;
                                //doughtnutChart.DataLabel.ShowLeaderLines = true;
                                doughtnutChart.Style = eChartStyle.Style26; //3D look
                            }
                        }

                    }


                    package.Save();
                }

                string fileName = "Report_" + DateTime.Now.ToShortTimeString() + ".xlsx";
                string fileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                stream.Position = 0;
                return File(stream, fileType, fileName);
                //return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Възникна неочаквана грешка! {ex}";
                return RedirectToAction("Index", "Home");
            }

        }

        public IActionResult SetEmployeePeriodDate(int userId)
        {
            try
            {
                var newPeriod = new PeriodViewModel();
                newPeriod.userId = userId;
                return RedirectToAction("Index", new { newPeriod});
            }
            catch (Exception)
            {
                TempData["Error"] = "[SetPersonalPeriodDate] Грешка при подготовка на модела за отчет";
                return RedirectToAction("Index", "Home");
            }
        }


        public IActionResult SetPersonalPeriodDate()
        {
            try
            {
                var newPeriod = new PeriodViewModel();
                newPeriod.userId = currentUser.Id;
                return View(newPeriod);
            }
            catch (Exception)
            {
                TempData["Error"] = "[SetPersonalPeriodDate] Грешка при подготовка на модела за отчет";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> PersonalPeriodReport(PeriodViewModel model)
        {
            try
            {
                if (model.StartDate > model.EndDate)
                {
                    TempData["Error"] = "[PersonalPeriodReport] Невалидни дати";
                    return RedirectToAction("Index", "Home");
                }

                if (model.userId < 1)
                {
                    TempData["Error"] = "[PersonalPeriodReport] Невалиден потребител";
                    return RedirectToAction("Index", "Home");
                }

                var employeeWorkForPeriod = await this.employees.GetPersonalReport(model.userId, model.StartDate.Date, model.EndDate.Date);
                return ExportSpravkaForEmployee(employeeWorkForPeriod, model.StartDate.Date, model.EndDate.Date);
            }
            catch (Exception)
            {
                TempData["Error"] = "[PersonalPeriodReport] Грешка при обработката на модела за отчет";
                return RedirectToAction("Index", "Home");
            }


        }


    }
}
