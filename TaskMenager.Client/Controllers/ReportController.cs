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
using System.Globalization;
using TaskMenager.Client.Models.Users;

namespace TaskMenager.Client.Controllers
{
    [Authorize(Policy = DataConstants.Employee)]
    public class ReportController : BaseController
    {
        private readonly IDateManagementConfiguration dateConfiguration;
        private readonly IDirectorateService directorates;
        private readonly IDepartmentsService departments;
        private readonly ISectorsService sectors;

        public ReportController(IDateManagementConfiguration _dateConfiguration, IDirectorateService directorates, IDepartmentsService departments, ISectorsService sectors, IEmployeesService employees, ITasksService tasks, IHttpContextAccessor httpContextAccessor, IEmailService email, IWebHostEnvironment env, IEmailConfiguration _emailConfiguration) : base(httpContextAccessor, employees, tasks, email, env, _emailConfiguration)
        {
            this.directorates = directorates;
            this.departments = departments;
            this.sectors = sectors;
            this.dateConfiguration = _dateConfiguration;
        }

        public IActionResult TaskReportPeriod(int taskId)
        {
            try
            {
                var currentTask = this.tasks.GetTaskDetails(taskId).FirstOrDefault();
                if ((((currentUser.RoleName == DataConstants.DirectorateAdmin) && (currentTask.DirectorateId != currentUser.DirectorateId)) || ((currentUser.RoleName == DataConstants.DepartmentAdmin) && (currentTask.DepartmentId != currentUser.DepartmentId)) || ((currentUser.RoleName == DataConstants.SectorAdmin) && (currentTask.SectorId != currentUser.SectorId)) || (currentUser.RoleName == DataConstants.Employee)) && (currentTask.AssignerId != currentUser.Id))
                {
                    TempData["Error"] = "[TaskReport] Текущия потребител няма права за този отчет";
                    return RedirectToAction("TasksList", "Tasks");
                }

                var taskPeriod = new PeriodViewModel();
                taskPeriod.taskId = taskId;
                return View(taskPeriod);
            }
            catch (Exception)
            {
                TempData["Error"] = "[TaskReport] Грешка при подготовка на модела за отчет";
                return RedirectToAction("TasksList", "Tasks");
            }
        }    //task 1 step

        public async Task<IActionResult> TaskReportResult(PeriodViewModel model)
        {
            try
            {
                if (model.StartDate > model.EndDate)
                {
                    TempData["Error"] = "[TaskReportResult] Невалидни дати";
                    return RedirectToAction("TasksList", "Tasks");
                }

                if (model.taskId < 1)
                {
                    TempData["Error"] = "[TaskReportResult] Невалиден номер на задача";
                    return RedirectToAction("TasksList", "Tasks");
                }
                var currentTask = this.tasks.GetTaskDetails(model.taskId).FirstOrDefault();
                model.TaskName = currentTask.TaskName;
                if (currentTask.TaskTypeName == DataConstants.TaskTypeGlobal)    //ако е глобална
                {
                    foreach (var itemId in await this.tasks.GetTaskChildsIdsAsync(currentTask.Id))
                    {
                        model.DateList.AddRange(await this.tasks.GetTaskReport(itemId, model.StartDate.Date, model.EndDate.Date));
                    }
                }
                else
                {
                    model.DateList = await this.tasks.GetTaskReport(model.taskId, model.StartDate.Date, model.EndDate.Date);
                }
                model.CheckRegistrationDate = this.dateConfiguration.CheckRegistrationDate;
                if (model.DateList.Count < 1)
                {
                    TempData["Error"] = "[TaskReportResult] Няма отчет по задачата за този период.";
                    return RedirectToAction("TaskReportPeriod", new { taskId = model.taskId });
                }
                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] = "[TaskReportResult] Грешка при обработката на модела за отчет";
                return RedirectToAction("TasksList", "Tasks");
            }
        }  //task 2 step

        public async Task<IActionResult> ExportSpravkaForTask(PeriodViewModel model)
        {
            try
            {
                var employeesList = new List<int>();
                var dateList = new HashSet<DateTime>();
                if (model.StartDate > model.EndDate)
                {
                    TempData["Error"] = "[ExportSpravkaForTask] Невалидни дати";
                    return RedirectToAction("TasksList", "Tasks");
                }

                if (model.taskId < 1)
                {
                    TempData["Error"] = "[ExportSpravkaForTask] Невалиден номер на задача";
                    return RedirectToAction("TasksList", "Tasks");
                }
                var taskReportForPeriod = new List<TaskWorkedHoursServiceModel>();
                var currentTask = this.tasks.GetTaskDetails(model.taskId).FirstOrDefault();
                if (currentTask.TaskTypeName == DataConstants.TaskTypeGlobal)    //ако е глобална
                {
                    foreach (var itemId in await this.tasks.GetTaskChildsIdsAsync(currentTask.Id))
                    {
                        taskReportForPeriod.AddRange(await this.tasks.GetTaskReport(itemId, model.StartDate.Date, model.EndDate.Date));
                    }
                }
                else
                {
                    taskReportForPeriod = await this.tasks.GetTaskReport(model.taskId, model.StartDate.Date, model.EndDate.Date);
                }
                //var taskReportForPeriod = await this.tasks.GetTaskReport(model.taskId, model.StartDate.Date, model.EndDate.Date);

                foreach (var item in taskReportForPeriod)
                {
                    if (!employeesList.Contains(item.EmployeeId))
                    {
                        employeesList.Add(item.EmployeeId);             //намират се всички служители и дати на които е работено за периода
                    }
                    if (!dateList.Contains(item.WorkDate.Date))
                    {
                        dateList.Add(item.WorkDate.Date);
                    }
                }

                // var employeePeriodTasks = employeePeriodTasksAll.GroupBy(l => new { l.Id, l.TextValue })   //Важно!!! Премахвам дублиращи се елементи в лист


                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var stream = new System.IO.MemoryStream();
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Test");
                    worksheet.View.ZoomScale = 100;
                    try
                    {
                        worksheet.Name = "Отчет за период";
                    }
                    catch (Exception)
                    {
                        TempData["Error"] = "Грешка при създаването на tab за отчета: " + currentTask.TaskName.Substring(0, 50) + "...";
                        return RedirectToAction("TasksList", "Tasks");
                    }

                    worksheet.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Row(2).Height = 48;
                    var headerrange = worksheet.Cells[2, 1, 2, employeesList.Count() + 1];
                    headerrange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headerrange.Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);
                    worksheet.Row(2).Style.Font.Size = 12;
                    worksheet.Column(1).Width = 12;
                    worksheet.Cells[2, 1].Value = "Дата";
                    int column = 2;
                    var tableEmpColumn = new Dictionary<int, int>();
                    foreach (var employeeId in employeesList)
                    {
                        worksheet.Column(column).Width = 11;
                        worksheet.Cells[2, column].Style.WrapText = true;
                        worksheet.Cells[2, column].Style.Font.Size = 8;
                        worksheet.Cells[2, column].Value = taskReportForPeriod.Where(wh => wh.EmployeeId == employeeId).Select(wh => wh.EmployeeName).FirstOrDefault();
                        worksheet.Cells[2, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[2, column].Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);
                        tableEmpColumn.Add(employeeId, column);
                        column += 1;
                    }
                    int col = column;
                    worksheet.Cells[2, column].Style.WrapText = true;
                    worksheet.Cells[2, column].Style.Font.Size = 12;
                    worksheet.Cells[2, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[2, column].Style.Fill.BackgroundColor.SetColor(Color.LightSteelBlue);
                    worksheet.Column(column).Width = 10;
                    worksheet.Cells[2, column].Value = "Общо за дата :";

                    worksheet.Cells[1, 1, 1, column > 20 ? column : 20].Merge = true;
                    worksheet.Row(1).Height = 25;
                    worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                    worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    worksheet.Cells[1, 1].Style.Indent = 10;
                    worksheet.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Cells[1, 1].Value = currentTask.TaskName + "  (" + model.StartDate.Date.ToString("dd/MM/yyyy") + "г. - " + model.EndDate.Date.ToString("dd/MM/yyyy") + "г.)";
                    worksheet.Cells[1, 1].Style.Font.Size = 14;
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    //worksheet.View.FreezePanes(400, 2);
                    var row = 3;

                    foreach (var date in dateList)
                    {

                        worksheet.Cells[row, 1].Style.Font.Size = 10;
                        worksheet.Cells[row, 1].Value = date.Date.ToString("dd/MM/yyyy") + "г.";
                        worksheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);

                        foreach (var empid in employeesList)
                        {
                            var hours = taskReportForPeriod.Where(wh => wh.EmployeeId == empid && wh.WorkDate.Date == date.Date).Sum(wh => wh.HoursSpend);
                            if (hours > 0)
                            {
                                worksheet.Cells[row, tableEmpColumn[empid]].Value = hours;
                            }

                        }
                        worksheet.Cells[row, col].Style.WrapText = true;
                        worksheet.Cells[row, col].Style.Font.Size = 12;
                        worksheet.Cells[row, col].Style.Font.Bold = true;
                        worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LightSteelBlue);
                        worksheet.Column(col).Width = 10;
                        worksheet.Cells[row, col].Value = taskReportForPeriod.Where(wh => wh.WorkDate.Date == date.Date).Sum(wh => wh.HoursSpend);
                        row += 1;
                    }

                    worksheet.Cells[row, 1].Value = "Общо :";
                    worksheet.Cells[row, 1].Style.Font.Size = 11;
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.LightSteelBlue);


                    foreach (var employeeId in employeesList)
                    {
                        worksheet.Cells[row, tableEmpColumn[employeeId]].Style.Font.Size = 12;
                        worksheet.Cells[row, tableEmpColumn[employeeId]].Style.Font.Bold = true;
                        worksheet.Cells[row, tableEmpColumn[employeeId]].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, tableEmpColumn[employeeId]].Style.Fill.BackgroundColor.SetColor(Color.LightSteelBlue);

                        var hours = taskReportForPeriod.Where(wh => wh.EmployeeId == employeeId).Sum(wh => wh.HoursSpend);
                        worksheet.Cells[row, tableEmpColumn[employeeId]].Value = hours;

                    }
                    worksheet.Cells[row, col].Value = taskReportForPeriod.Sum(wh => wh.HoursSpend);
                    worksheet.Cells[row, col].Style.Font.Size = 13;
                    worksheet.Cells[row, col].Style.Font.Bold = true;
                    worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LightSteelBlue);


                    var bodyrange = worksheet.Cells[3, 2, row, col];
                    bodyrange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.View.ShowGridLines = true;
                    worksheet.PrinterSettings.ShowGridLines = true;
                    worksheet.PrinterSettings.Orientation = eOrientation.Landscape;

                    //create a new piechart of type Doughnut
                    var doughtnutChart = worksheet.Drawings.AddChart("crtExtensionCount", eChartType.DoughnutExploded) as ExcelDoughnutChart;
                    //Set position to row 1 column 7 and 16 pixels offset
                    doughtnutChart.SetPosition(row + 2, 0, 1, 10);
                    doughtnutChart.SetSize(500, 500);
                    doughtnutChart.Series.Add(ExcelRange.GetAddress(row, 2, row, col - 1), ExcelRange.GetAddress(2, 2, 2, col - 1));
                    doughtnutChart.Title.Text = "ЕКСПЕРТ / ЧАСОВЕ";
                    doughtnutChart.DataLabel.ShowPercent = true;
                    //doughtnutChart.DataLabel.ShowLeaderLines = true;
                    doughtnutChart.Style = eChartStyle.Style26; //3D look

                    ExcelPieChart pieChart = worksheet.Drawings.AddChart("pieChart", eChartType.Pie3D) as ExcelPieChart;
                    pieChart.Title.Text = "Дата / Часове";
                    //select the ranges for the pie. First the values, then the header range
                    pieChart.Legend.Position = eLegendPosition.Bottom;
                    pieChart.DataLabel.ShowPercent = true;
                    pieChart.DataLabel.ShowLeaderLines = true;
                    pieChart.DataLabel.ShowCategory = true;
                    pieChart.Legend.Remove();
                    var rangeTitles = ExcelRange.GetAddress(3, 1, row - 1, 1);
                    pieChart.Series.Add(ExcelRange.GetAddress(3, col, row - 1, col), rangeTitles);
                    pieChart.SetSize(600, 500);
                    pieChart.SetPosition(row + 2, 0, 10, 0);
                    pieChart.StyleManager.SetChartStyle(ePresetChartStyle.Pie3dChartStyle8, ePresetChartColors.ColorfulPalette3);
                    pieChart.DisplayBlanksAs = eDisplayBlanksAs.Gap;
                    pieChart.DataLabel.Font.Fill.Color = Color.Black;
                    pieChart.DataLabel.Font.Size = 12;
                    //pieChart.StyleManager.SetChartStyle(ePresetChartStyle.Pie3dChartStyle7);


                    package.Save();
                }

                string fileName = "TaskReport_" + DateTime.Now.ToShortTimeString() + ".xlsx";
                string fileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                stream.Position = 0;
                return File(stream, fileType, fileName);

                //TempData["Success"] = $"[ExportSpravkaForTask] {model.StartDate}, {model.EndDate}";
                //return RedirectToAction("TasksList", "Tasks");


            }
            catch (Exception)
            {
                TempData["Error"] = $"[ExportSpravkaForTask] Грешка при обработката на модела за създаване на файла. {model.StartDate}, {model.EndDate}";
                return RedirectToAction("TasksList", "Tasks");
            }

        }   //task 3 step

        public async Task<IActionResult> ExportSpravkaForEmployee(PeriodViewModel model)
        {
            try
            {
                var taskList = new List<int>();
                var employeeWork = await this.employees.GetPersonalReport(model.userId, model.StartDate.Date, model.EndDate.Date);

                if (employeeWork.WorkedHoursByTaskByPeriod.Count < 1)
                {
                    TempData["Error"] = $"Няма задачи по които да е работил {employeeWork.FullName} за избрания период";
                    return RedirectToAction("UsersList", "Users");
                }

                foreach (var item in employeeWork.WorkedHoursByTaskByPeriod)
                {
                    if (!taskList.Contains(item.TaskId) && item.TaskName != "Отпуски" && item.TaskName != "Болнични")
                    {
                        taskList.Add(item.TaskId);             //намират се всички служители и дати на които е работено за периода
                    }
                }


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
                        return RedirectToAction("UsersList", "Users");
                    }
                    worksheet.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Row(2).Height = 48;
                    var headerrange = worksheet.Cells[2, 1, 2, taskList.Count() + 1];
                    headerrange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headerrange.Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);
                    worksheet.Row(2).Style.Font.Size = 12;
                    worksheet.Column(1).Width = 12;
                    worksheet.Cells[2, 1].Value = "Дата";
                    int column = 2;
                    var tableTaskColumn = new Dictionary<int, int>();
                    foreach (var taskId in taskList)
                    {
                        worksheet.Column(column).Width = 11;
                        worksheet.Cells[2, column].Style.WrapText = true;
                        worksheet.Cells[2, column].Style.Font.Size = 8;
                        worksheet.Cells[2, column].Value = employeeWork.WorkedHoursByTaskByPeriod.Where(wh => wh.TaskId == taskId).Select(wh => wh.TaskName).FirstOrDefault();
                        worksheet.Cells[2, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[2, column].Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);
                        tableTaskColumn.Add(taskId, column);
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
                    worksheet.Cells[1, 1].Value = employeeWork.FullName + "  (" + model.StartDate.Date.ToString("dd/MM/yyyy") + "г. - " + model.EndDate.Date.ToString("dd/MM/yyyy") + "г.)";
                    worksheet.Cells[1, 1].Style.Font.Size = 14;
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    //worksheet.View.FreezePanes(400, 2);

                    column = 1;
                    int row = 2;
                    var tempDate = DateTime.Now.Date.AddDays(1);
                    foreach (var item in employeeWork.WorkedHoursByTaskByPeriod)
                    {
                        if (tempDate != item.WorkDate)
                        {
                            row += 1;
                            if (item.TaskName == "Отпуски" || item.TaskName == "Болнични")
                            {
                                worksheet.Cells[row, col].Formula = "";
                                worksheet.Cells[row, column + 1].Value = (item.TaskName == "Отпуски" ? "Отпуск" : "Болничен");
                                worksheet.Cells[row, column].Style.Font.Size = 10;
                                worksheet.Cells[row, column].Value = item.WorkDate.ToString("dd/MM/yyyy") + "г.";
                                var holidayrow = worksheet.Cells[row, column, row, col];
                                holidayrow.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                holidayrow.Style.Fill.BackgroundColor.SetColor(item.TaskName == "Отпуски" ? Color.LightBlue : Color.LightSalmon);


                            }
                            else
                            {
                                if (row > 2 && col > 2) //не сочи амфетката с имената на задачите и има поне една задача
                                {
                                    formula = "=SUM(B" + row.ToString() + ":" + (col <= 27 ? ((char)('A' + (col - 2))).ToString() : ("A" + ((char)('A' + (col - 28))).ToString())) + row.ToString() + ")";
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
                            }

                            tempDate = item.WorkDate;

                        }
                        else
                        {
                            if (item.TaskName == "Отпуски" || item.TaskName == "Болнични")
                            {
                                worksheet.Cells[row, col].Formula = "";
                                worksheet.Cells[row, column + 1].Value = (item.TaskName == "Отпуски" ? "Отпуск" : "Болничен");
                                var holidayrow = worksheet.Cells[row, column, row, col];
                                holidayrow.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                holidayrow.Style.Fill.BackgroundColor.SetColor(item.TaskName == "Отпуски" ? Color.LightBlue : Color.LightSalmon);
                                if (col > 15)
                                {
                                    worksheet.Cells[row, col + 1].Value = "";
                                }

                            }
                            else
                            {
                                worksheet.Cells[row, tableTaskColumn[item.TaskId]].Value = item.HoursSpend;
                            }

                        }
                    }
                    row += 1;

                    worksheet.Cells[row, 1].Value = "Общо :";
                    worksheet.Cells[row, 1].Style.Font.Size = 11;
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.LightSteelBlue);

                    for (column = 2; column <= col; column++)
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

                    //create a new piechart of type Doughnut
                    var doughtnutChart = worksheet.Drawings.AddChart("crtExtensionCount", eChartType.DoughnutExploded) as ExcelDoughnutChart;
                    //Set position to row 1 column 7 and 16 pixels offset
                    doughtnutChart.SetPosition(row + 2, 0, 1, 10);
                    doughtnutChart.SetSize(500, 500);
                    doughtnutChart.Series.Add(ExcelRange.GetAddress(row, 2, row, col - 1), ExcelRange.GetAddress(2, 2, 2, col - 1));
                    doughtnutChart.Title.Text = "Задача / Часове";
                    doughtnutChart.DataLabel.ShowPercent = true;
                    //doughtnutChart.DataLabel.ShowLeaderLines = true;
                    doughtnutChart.Style = eChartStyle.Style26; //3D look

                    worksheet.Cells[row + 2, 10].Value = "Отпуск за периода :";
                    worksheet.Cells[row + 2, 12].Value = employeeWork.WorkedHoursByTaskByPeriod.Where(t => t.TaskName == "Отпуски").Count();
                    var totalholidayrow = worksheet.Cells[row + 2, 10, row + 2, 12];
                    totalholidayrow.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    totalholidayrow.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);

                    worksheet.Cells[row + 3, 10].Value = "Болнични за периода :";
                    worksheet.Cells[row + 3, 12].Value = employeeWork.WorkedHoursByTaskByPeriod.Where(t => t.TaskName == "Болнични").Count();
                    var totalillrow = worksheet.Cells[row + 3, 10, row + 3, 12];
                    totalillrow.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    totalillrow.Style.Fill.BackgroundColor.SetColor(Color.LightSalmon);


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
                return RedirectToAction("UsersList", "Users");
            }
        }   //employee 3 step

        public IActionResult PeriodReport()
        {
            try
            {
                var newPeriod = new PeriodReportViewModel();
                newPeriod.Directorates = this.directorates.GetDirectoratesNames(null)
                               .Select(a => new SelectListItem
                               {
                                   Text = a.TextValue,
                                   Value = a.Id.ToString()
                               })
                               .ToList();
                if (currentUser.DirectorateId.HasValue)
                {
                    newPeriod.DirectoratesId = currentUser.DirectorateId.Value.ToString();
                    newPeriod.Directorates.Where(d => d.Value == currentUser.DirectorateId.Value.ToString()).FirstOrDefault().Selected = true;
                }
                else
                {
                    newPeriod.Directorates.Insert(0, new SelectListItem
                    {
                        Text = ChooseValue,
                        Value = "0",
                        Selected = true
                    });
                    newPeriod.DirectoratesId = "0";

                    //newPeriod.DirectoratesId = newPeriod.Directorates.FirstOrDefault().Value;
                    //    newPeriod.Directorates.FirstOrDefault().Selected = true;
                }
                return View(newPeriod);
            }
            catch (Exception)
            {
                TempData["Error"] = "[PeriodReport] Грешка при подготовка на модела за отчет";
                return RedirectToAction("Index", "Home");
            }
        }       //  I


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
                    employeesIds = this.employees.GetEmployeesNamesBySectorAsync(currentUser.SectorId).Result
                                                        .Select(e => e.Id)
                                                        .ToList();
                }
                else if (currentUser.RoleName == DepartmentAdmin)
                {
                    employeesIds = this.employees.GetEmployeesNamesByDepartmentAsync(currentUser.DepartmentId).Result
                                                        .Select(e => e.Id)
                                                        .ToList();
                }
                else if (currentUser.RoleName == DirectorateAdmin)
                {
                    employeesIds = this.employees.GetEmployeesNamesByDirectorateAsync(currentUser.DirectorateId).Result
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
                    employeesIds = this.employees.GetEmployeesNamesByDirectorateAsync(int.Parse(model.DirectoratesId)).Result
                                        .Select(e => e.Id)
                                        .ToList();
                }
                newReport.EmployeesIds = employeesIds.ToArray();
                newReport.StartDate = model.StartDate.Date;
                newReport.EndDate = model.EndDate.Date;
                newReport.WithDepTabs = model.WithDepTabs;
                return await ExportReport(newReport);
            }
            catch (Exception)
            {
                TempData["Error"] = "Грешка при обработката на модела за отчет";
                return RedirectToAction(nameof(PeriodReport));
            }

        }    //  II


        public async Task<IActionResult> ExportReport(PeriodReportViewModel model)   //   III
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

                var allExpertsOnTasks = new HashSet<ReportUserServiceModel>();
                var allExperts = this.employees.GetEmployeesByList(model.EmployeesIds);

                tasksList.ForEach(t => t.Colleagues.ForEach(col => allExpertsOnTasks.Add(col)));  //списък на всички експерти по всички задачи
                var directorate = new SelectServiceModel()
                {
                    Id = allExpertsOnTasks.FirstOrDefault().DirectorateId ?? 0
                };
                if (directorate.Id == 0)
                {
                    directorate.TextValue = "Разширен отчет";
                    // TODO справката е за експерти назначени в дирекции, вкл директора, не се вкл председател, секретар, заместник председатели, затова не е необходима тази проверка
                }
                else
                {
                    directorate.TextValue = this.directorates.GetDirectoratesNames(directorate.Id).FirstOrDefault().TextValue;   //дирекцията за която ще е отчета
                }

                var departmentsList = this.departments.GetDepartmentsNamesByDirectorate(directorate.Id).ToList();  //отделите в дирекцията
                var sectorsList = await this.sectors.GetSectorsNamesByDirectorate(directorate.Id);    //секторите в дирекцията

                //създавам виртуален отдел Директори и всички директори ги назначавам в него(само за справката)
                var otdelDirectori = new SelectServiceModel()
                {
                    DirectorateName = directorate.DirectorateName,
                    DepartmentName = directorate.TextValue,
                    Id = 999,
                    isDeleted = false,
                    TextValue = directorate.TextValue
                };
                departmentsList.Insert(0, otdelDirectori);
                //създавам виртуален отдел Директори и всички директори ги назначавам в него(само за справката)
                foreach (var director in allExperts.Where(s => s.DepartmentId == null && s.SectorId == null).ToList())
                {
                    director.DepartmentId = 999;
                }

                
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var stream = new System.IO.MemoryStream();
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    if (model.WithDepTabs == false)
                    {
                        if (allExperts.Count() > 0)   //ако има експерти
                        {
                            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Test");
                            worksheet.View.ZoomScale = 70;
                            try
                            {
                                 worksheet.Name = "Разширен отчет";
                            }
                            catch (Exception)
                            {
                                TempData["Error"] = "Грешка при създаването на tab за справката";
                                return RedirectToAction(nameof(PeriodReport));
                            }

                            worksheet.Cells[1, 1, 1, (8 + allExperts.Count())].Merge = true;
                            worksheet.Row(1).Height = 25;
                            worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                            worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            worksheet.Cells[1, 1].Style.Indent = 10;
                            worksheet.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            if (allExperts.Count() == 1)
                            {
                                worksheet.Cells[1, 1].Value = "Отчет на: " + allExperts.Select(e => e.FullName).FirstOrDefault() + " (" + model.StartDate.Date.ToString("dd/MM/yyyy") +
                                    "г. - " + model.EndDate.Date.ToString("dd/MM/yyyy") + "г.)";
                            }

                            else
                            {
                               var adminUnitName = (currentUser.RoleName == SuperAdmin || currentUser.RoleName == DirectorateAdmin) ? directorate.TextValue : currentUser.DepartmentName;
                                worksheet.Cells[1, 1].Value = ((currentUser.RoleName == SuperAdmin || currentUser.RoleName == DirectorateAdmin) ? "Дирекция: \"" : "Отдел: \"") + adminUnitName + "\"  (" + model.StartDate.Date.ToString("dd/MM/yyyy") + "г. - " + model.EndDate.Date.ToString("dd/MM/yyyy") + "г.)";
                            }
                            worksheet.Cells[1, 1].Style.Font.Size = 14;
                            worksheet.Cells[1, 1].Style.Font.Bold = true;
                            //дотук се създава първия ред за отдела в екселския файл
                            
                            await DrawExpertsData(tasksList, allExperts.OrderBy(e => e.Id), worksheet);
                        }

                    }
                    else
                    {
                        foreach (var department in departmentsList)
                        {     
                            if (allExperts.Where(s => s.DepartmentId == department.Id && s.SectorId == null).ToList().Count() > 0)   //ако има експерти в отдела
                            {
                                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Test");
                                worksheet.View.ZoomScale = 70;
                                try
                                {
                                    worksheet.Name = department.TextValue;
                                }
                                catch (Exception)
                                {
                                    TempData["Error"] = "Грешка при създаването на tab за отдел: " + department.TextValue.Substring(0, 50) + "...";
                                    return RedirectToAction(nameof(PeriodReport));
                                }

                                worksheet.Cells[1, 1, 1, (8 + allExperts
                                    .Where(e => e.DepartmentId == department.Id && e.SectorId == null).Count())].Merge = true;
                                worksheet.Row(1).Height = 25;
                                worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                worksheet.Cells[1, 1].Style.Indent = 10;
                                worksheet.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                if (allExperts.Count() == 1)
                                {
                                    worksheet.Cells[1, 1].Value = "Отчет на: " + allExperts.Where(s => s.DepartmentId == department.Id).Select(e => e.FullName).FirstOrDefault() + " (" + model.StartDate.Date.ToString("dd/MM/yyyy") +
                                        "г. - " + model.EndDate.Date.ToString("dd/MM/yyyy") + "г.)";
                                }

                                else
                                {
                                    worksheet.Cells[1, 1].Value = ((department.TextValue == directorate.TextValue) ? "Дирекция: \"" : "Отдел: \"") + department.TextValue + "\"  (" + model.StartDate.Date.ToString("dd/MM/yyyy") + "г. - " + model.EndDate.Date.ToString("dd/MM/yyyy") + "г.)";
                                }
                                worksheet.Cells[1, 1].Style.Font.Size = 14;
                                worksheet.Cells[1, 1].Style.Font.Bold = true;
                                //дотук се създава първия ред за отдела в екселския файл

                                await DrawExpertsData(tasksList, allExperts.Where(e => e.DepartmentId == department.Id && e.SectorId == null).OrderBy(e => e.Id), worksheet);
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
                                    worksheet.Name = sector.TextValue;
                                }
                                catch (Exception)
                                {
                                    TempData["Error"] = "Грешка при създаването на tab за сектор: " + sector.TextValue.Substring(0, 50) + "...";
                                    return RedirectToAction(nameof(PeriodReport));
                                }

                                worksheet.Cells[1, 1, 1, (8 + allExperts.Where(e => e.SectorId == sector.Id).Count())].Merge = true;
                                worksheet.Row(1).Height = 25;
                                worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                worksheet.Cells[1, 1].Style.Indent = 10;
                                worksheet.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                if (allExperts.Where(s => s.SectorId == sector.Id).ToList().Count() == 1)
                                {
                                    worksheet.Cells[1, 1].Value = "Отчет на: " + allExperts.Where(s => s.SectorId == sector.Id).Select(e => e.FullName).FirstOrDefault() + " (" + model.StartDate.Date.ToString("dd/MM/yyyy") +
                                        "г. - " + model.EndDate.Date.ToString("dd/MM/yyyy") + "г.)";
                                }
                                else
                                {
                                    worksheet.Cells[1, 1].Value = "Сектор: \"" + sector.TextValue + "\"  (" + model.StartDate.Date.ToString("dd/MM/yyyy") +
                                        "г. - " + model.EndDate.Date.ToString("dd/MM/yyyy") + "г.)";
                                }
                                worksheet.Cells[1, 1].Style.Font.Size = 14;
                                worksheet.Cells[1, 1].Style.Font.Bold = true;
                                // първи ред

                                await DrawExpertsData(tasksList, allExperts.Where(s => s.SectorId == sector.Id).OrderBy(e => e.Id), worksheet);
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

        private async Task DrawExpertsData(List<ReportServiceModel> tasksList, IEnumerable<ReportUserServiceModel> allExperts, ExcelWorksheet worksheet)
        {
            var expertsIds = allExperts
                .OrderBy(e => e.Id)
                .Select(e => e.Id)
                .ToHashSet();

            var formula = string.Empty;
            worksheet.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Row(2).Height = 48;
            var headerrange = worksheet.Cells[2, 1, 2, 5];
            headerrange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerrange.Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);
            worksheet.Row(2).Style.Font.Size = 12;
            worksheet.Cells[2, 1].Value = "№";
            worksheet.Column(1).Width = 10;
            worksheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[2, 2].Value = "Наименование и пояснения към задачата";
            worksheet.Column(2).Width = 60;
            worksheet.Column(2).Style.WrapText = true;
            worksheet.Cells[2, 3].Value = "Начална Дата";
            worksheet.Column(3).Width = 15;
            worksheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[2, 4].Value = "Краен срок";
            worksheet.Column(4).Width = 15;
            worksheet.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[2, 5].Value = "Описание \\ Резултати";
            worksheet.Column(5).Width = 60;
            worksheet.Column(5).Style.WrapText = true;
            int column = 6;

            foreach (var expert in allExperts)
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
            column += 1;
            worksheet.Cells[2, column].Style.WrapText = true;
            worksheet.Cells[2, column].Value = "Коментари по задачата";
            //worksheet.View.FreezePanes(100, 6);
            //дотук се създава втория ред за отдела в екселския файл
            var parentTaskId = 0;
            var row = 3;
            var workingDaysSystemSum = 0;
            var workingHoursSpecificSum = 0;
            var workingHoursProcurementSum = 0;
            var workingHoursLearningSum = 0;
            var workingHoursAdminSum = 0;
            var workingHoursMeetingsSum = 0;
            var workingHoursOtherSum = 0;
            var workingHoursSProjectSum = 0;
            var taskExpertsColumn = 6;
            foreach (var task in tasksList.Where(t => expertsIds.Overlaps(t.Colleagues.Select(e => e.Id).ToList()) && t.TaskTypeName != TaskTypeSystem).OrderBy(t => t.ParentTaskId).ThenByDescending(t => t.TaskTypeName))
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
                var allExpertsNotes = string.Empty;
                foreach (var eId in expertsIds)
                {
                    var curentColegue = task.Colleagues.Where(cl => cl.Id == eId).FirstOrDefault();
                    if (curentColegue != null)
                    {
                        worksheet.Cells[row, taskExpertsColumn].Value = curentColegue.TaskWorkedHours;
                        if (!string.IsNullOrWhiteSpace(curentColegue.UserNotesForPeriod))   //ако има коментари за периода от експерта --> ги добавям към всички коментари
                        {
                            allExpertsNotes = allExpertsNotes + Environment.NewLine + "*" + curentColegue.FullName.ToUpper() + "*" + Environment.NewLine + curentColegue.UserNotesForPeriod;
                        }
                    }
                    taskExpertsColumn += 1;
                }
                string sufix = GetSufixByEmpCount(taskExpertsColumn);
                var reminder = ((taskExpertsColumn - 2) % 26);
                
                formula = "=SUM(F" + (row).ToString() + ":" + (taskExpertsColumn <= 27 ? ((char)('A' + (taskExpertsColumn - 2))).ToString() : (sufix + ((char)('A' + reminder)).ToString())) + (row).ToString() + ")";  //общо часове по задачата
                worksheet.Cells[row, taskExpertsColumn].Formula = formula;

                worksheet.Cells[row, taskExpertsColumn].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, taskExpertsColumn].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                formula = "=COUNTA(F" + (row).ToString() + ":" + (taskExpertsColumn <= 27 ? ((char)('A' + (taskExpertsColumn - 2))).ToString() : (sufix + ((char)('A' + reminder)).ToString())) + (row).ToString() + ")";  //общо eksperti по задачата
                worksheet.Cells[row, taskExpertsColumn + 1].Formula = formula;
                worksheet.Cells[row, taskExpertsColumn + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, taskExpertsColumn + 1].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);

                worksheet.Column(taskExpertsColumn + 2).Width = 80;
                worksheet.Cells[row, taskExpertsColumn + 2].Style.WrapText = true;
                worksheet.Cells[row, taskExpertsColumn + 2].Value = allExpertsNotes;    //коментарите по задачата

                int workedHoursByDepartment = task.Colleagues.Where(cl => expertsIds.Contains(cl.Id)).Sum(cl => cl.TaskWorkedHours ?? 0);
                if (task.TaskTypeName == TaskTypeSystem)
                {
                    workingDaysSystemSum += workedHoursByDepartment / 8;
                    worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.LightSlateGray);
                }

                else if (task.TaskTypeName == TaskTypeSpecificWork)
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
                else if (task.TaskTypeName == TaskTypeSpecialTeam)
                {
                    workingHoursSProjectSum += workedHoursByDepartment;
                    worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.MistyRose);
                }


                row += 1;
            }
            var modelTable = worksheet.Cells[2, 1, row, column];
            modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            modelTable.Style.Border.Left.Style = ExcelBorderStyle.Thin;             //border за клетките около задачите и верикално позициониране
            modelTable.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            modelTable.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            if (row > 3)  // дали има поне една задача по която да е работено в отдела/сектора
            {

                worksheet.Cells[row, 5].Value = "Брой задачи по които е работено";
                for (int col = 6; col < taskExpertsColumn; col++)   // taskExpertsColumn сочи колоната "Общо колко часа е работено..."
                {
                    var sufixCounta = GetSufixByEmpCount(col+1);
                    var reminderCounta = ((col-1) % 26);


                    formula = "=COUNTA(" + (col <= 26 ? ((char)('A' + (col - 1))).ToString() : (sufixCounta + ((char)('A' + reminderCounta)).ToString())) + "3:" + (col <= 26 ? ((char)('A' + (col - 1))).ToString() : (sufixCounta + ((char)('A' + reminderCounta)).ToString())) + (row - 1).ToString() + ")";
                    //formula = "=COUNTA(" + (col <= 26 ? ((char)('A' + (col - 1))).ToString() : (sufix + ((char)('A' + (col - 27))).ToString())) + "3:" + (col <= 26 ? ((char)('A' + (col - 1))).ToString() : ("A" + ((char)('A' + (col - 27))).ToString())) + (row - 1).ToString() + ")";

                    worksheet.Cells[row, col].Formula = formula;
                }
                worksheet.Cells[row, taskExpertsColumn].Style.Numberformat.Format = "0.00";

                var sufixAverage = GetSufixByEmpCount(taskExpertsColumn);
                var reminderAverage = ((taskExpertsColumn-2) % 26);
                formula = "=AVERAGE(F" + (row).ToString() + ":" + (taskExpertsColumn <= 27 ? ((char)('A' + (taskExpertsColumn - 2))).ToString() : (sufixAverage + ((char)('A' + reminderAverage)).ToString())) + (row).ToString() + ")";
                //formula = "=AVERAGE(F" + (row).ToString() + ":" + (taskExpertsColumn <= 27 ? ((char)('A' + (taskExpertsColumn - 2))).ToString() : ("A" + ((char)('A' + (taskExpertsColumn - 28))).ToString())) + (row).ToString() + ")";

                worksheet.Cells[row, taskExpertsColumn].Formula = formula; //по колко задачи средно е работено
                var modelTableBroiZadachi = worksheet.Cells[row, 5, row, taskExpertsColumn];
                modelTableBroiZadachi.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                modelTableBroiZadachi.Style.Border.Left.Style = ExcelBorderStyle.Thin;       //border за клетките около броя задачи
                modelTableBroiZadachi.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                modelTableBroiZadachi.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                modelTableBroiZadachi.Style.Fill.PatternType = ExcelFillStyle.Solid;
                modelTableBroiZadachi.Style.Fill.BackgroundColor.SetColor(Color.Orange);

                sufixAverage = GetSufixByEmpCount(taskExpertsColumn+2);
                reminderAverage = (taskExpertsColumn % 26);
                formula = "=AVERAGE(" + (taskExpertsColumn <= 25 ? ((char)('A' + (taskExpertsColumn))).ToString() : (sufixAverage + ((char)('A' + reminderAverage)).ToString())) + "3:" + (taskExpertsColumn <= 25 ? ((char)('A' + (taskExpertsColumn))).ToString() : (sufixAverage + ((char)('A' + reminderAverage)).ToString())) + (row - 1).ToString() + ")";   // ако има повече от 19 човека в сектора се променя генерацията на колоната
                worksheet.Cells[row, taskExpertsColumn + 1].Style.Numberformat.Format = "0.00";
                worksheet.Cells[row, taskExpertsColumn + 1].Formula = formula;

                worksheet.Cells[row, taskExpertsColumn + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, taskExpertsColumn + 1].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);  //средно колко човека са работили по задача

                row += 1;
                worksheet.Cells[row, 5].Value = "Общо часове на служителя";
                for (int col = 6; col < taskExpertsColumn; col++)   // taskExpertsColumn сочи колоната "Общо колко часа е работено..."
                {
                    var sufixSum = GetSufixByEmpCount(col+1);
                    var reminderSum = ((col - 1) % 26);

                    formula = "=SUM(" + (col <= 26 ? ((char)('A' + (col - 1))).ToString() : (sufixSum + ((char)('A' + reminderSum)).ToString())) + "3:" + (col <= 26 ? ((char)('A' + (col - 1))).ToString() : (sufixSum + ((char)('A' + reminderSum)).ToString())) + (row - 2).ToString() + ")";
                    worksheet.Cells[row, col].Formula = formula;
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

                sufixAverage = GetSufixByEmpCount(taskExpertsColumn);
                reminderAverage = ((taskExpertsColumn - 2) % 26);

                formula = "=SUM(F" + (row - 1).ToString() + ":" + (taskExpertsColumn <= 27 ? ((char)('A' + (taskExpertsColumn - 2))).ToString() : (sufixAverage + ((char)('A' + reminderAverage)).ToString())) + (row - 1).ToString() + ")";  //ОБЩО ЧАСОВЕ

                worksheet.Cells[row, 6].Formula = formula;

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
                    var modelTablePercentage = worksheet.Cells[row, 2, row + 7, 3];
                    modelTablePercentage.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    modelTablePercentage.Style.Border.Left.Style = ExcelBorderStyle.Thin;       //border за клетките около процентите
                    modelTablePercentage.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    modelTablePercentage.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    modelTablePercentage.Style.Fill.PatternType = ExcelFillStyle.Solid;


                    worksheet.Cells[row, 2].Value = TaskTypeSpecificWork;
                    worksheet.Cells[row, 3].Value = workingHoursSpecificSum;
                    worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);

                    row += 1;
                    worksheet.Cells[row, 2].Value = TaskTypeProcurement;
                    worksheet.Cells[row, 3].Value = workingHoursProcurementSum;
                    worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.LightSeaGreen);

                    row += 1;
                    worksheet.Cells[row, 2].Value = TaskTypeLearning;
                    worksheet.Cells[row, 3].Value = workingHoursLearningSum;
                    worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.LightGoldenrodYellow);

                    row += 1;
                    worksheet.Cells[row, 2].Value = TaskTypeAdminActivity;
                    worksheet.Cells[row, 3].Value = workingHoursAdminSum;
                    worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.Bisque);

                    row += 1;
                    worksheet.Cells[row, 2].Value = TaskTypeMeetings;
                    worksheet.Cells[row, 3].Value = workingHoursMeetingsSum;
                    worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.LightSalmon);

                    row += 1;
                    worksheet.Cells[row, 2].Value = TaskTypeSpecialTeam;
                    worksheet.Cells[row, 3].Value = workingHoursSProjectSum;
                    worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.MistyRose);

                    row += 1;
                    worksheet.Cells[row, 2].Value = TaskTypeOther;
                    worksheet.Cells[row, 3].Value = workingHoursOtherSum;
                    worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                    row += 1;
                    worksheet.Cells[row, 2].Value = "Общо часове";
                    worksheet.Cells[row, 3].Value = totalHoursWorckedByType;
                    worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.White);
                    worksheet.Cells[row, 2, row, 3].Style.Font.Bold = true;

                    //Отпуски и болнични начало
                    var rowBeforeOtpuski = row;
                    var totalSystemDays = 0;
                    foreach (var task in tasksList.Where(t => expertsIds.Overlaps(t.Colleagues.Select(e => e.Id).ToList()) && t.TaskTypeName == TaskTypeSystem).OrderBy(t => t.Id))
                    {
                        row += 2;
                        totalSystemDays = 0;
                        worksheet.Cells[row, 2].Value = task.TaskName;
                        worksheet.Cells[row, 2, row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, 2].Style.Font.Bold = true;
                        worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(task.TaskName == "Отпуски" ? Color.LightBlue : Color.LightSalmon);
                        row += 1;
                        foreach (var eId in expertsIds)
                        {
                            var curentColegue = task.Colleagues.Where(cl => cl.Id == eId).FirstOrDefault();
                            if (curentColegue != null)
                            {
                                var daysontask = curentColegue.TaskWorkedHours / 8;
                                if (daysontask != null)
                                {
                                    worksheet.Cells[row, 2].Value = curentColegue.FullName;
                                    worksheet.Cells[row, 3].Value = (daysontask > 1 ? daysontask.ToString() + " дни" : daysontask.ToString() + " ден");
                                    totalSystemDays += daysontask.HasValue ? daysontask.Value : 0;
                                    row += 1;
                                }
                            }

                        }
                        worksheet.Cells[row, 2].Value = "Общо дни";
                        worksheet.Cells[row, 3].Value = (totalSystemDays == 1 ? totalSystemDays + " ден" : totalSystemDays.ToString() + " дни");
                        worksheet.Cells[row, 2, row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.White);
                        worksheet.Cells[row, 2, row, 3].Style.Font.Bold = true;

                    }
                    if (rowBeforeOtpuski + 2 <= row)    //ако няма отпуски и болнични да не слага бордер
                    {
                        var modelOtpuski = worksheet.Cells[rowBeforeOtpuski + 2, 2, row, 3];
                        modelOtpuski.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        modelOtpuski.Style.Border.Left.Style = ExcelBorderStyle.Thin;       //border за клетките около болнични и отпуски
                        modelOtpuski.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        modelOtpuski.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        row = rowBeforeOtpuski;
                    }

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
                    pieChart.SetPosition(row - 7, 0, 8, 0);
                    pieChart.StyleManager.SetChartStyle(ePresetChartStyle.Pie3dChartStyle8, ePresetChartColors.ColorfulPalette3);
                    pieChart.DisplayBlanksAs = eDisplayBlanksAs.Gap;
                    pieChart.DataLabel.Font.Fill.Color = Color.Black;
                    pieChart.DataLabel.Font.Size = 12;
                    //create a new piechart of type Doughnut
                    var doughtnutChart = worksheet.Drawings.AddChart("crtExtensionCount", eChartType.DoughnutExploded) as ExcelDoughnutChart;
                    //Set position to row 1 column 7 and 16 pixels offset
                    doughtnutChart.SetPosition(row - 7, 0, 4, 10);
                    doughtnutChart.SetSize(500, 500);
                    doughtnutChart.Series.Add(ExcelRange.GetAddress(row - 7, 3, row - 1, 3), ExcelRange.GetAddress(row - 7, 2, row - 1, 2));
                    doughtnutChart.Title.Text = "ТИП ЗАДАЧА / ЧАСОВЕ";
                    doughtnutChart.DataLabel.ShowPercent = true;
                    //doughtnutChart.DataLabel.ShowLeaderLines = true;
                    doughtnutChart.Style = eChartStyle.Style26; //3D look
                }
                else
                {
                    //Отпуски и болнични начало
                    var rowBeforeOtpuski = row;
                    var totalSystemDays = 0;
                    foreach (var task in tasksList.Where(t => expertsIds.Overlaps(t.Colleagues.Select(e => e.Id).ToList()) && t.TaskTypeName == TaskTypeSystem).OrderBy(t => t.Id))
                    {
                        row += 2;
                        totalSystemDays = 0;
                        worksheet.Cells[row, 2].Value = task.TaskName;
                        worksheet.Cells[row, 2, row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, 2].Style.Font.Bold = true;
                        worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(task.TaskName == "Отпуски" ? Color.LightBlue : Color.LightSalmon);
                        row += 1;
                        foreach (var eId in expertsIds)
                        {
                            var curentColegue = task.Colleagues.Where(cl => cl.Id == eId).FirstOrDefault();
                            if (curentColegue != null)
                            {
                                var daysontask = curentColegue.TaskWorkedHours / 8;
                                if (daysontask != null)
                                {
                                    worksheet.Cells[row, 2].Value = curentColegue.FullName;
                                    worksheet.Cells[row, 3].Value = (daysontask > 1 ? daysontask.ToString() + " дни" : daysontask.ToString() + " ден");
                                    totalSystemDays += daysontask.HasValue ? daysontask.Value : 0;
                                    row += 1;
                                }
                            }

                        }
                        worksheet.Cells[row, 2].Value = "Общо дни";
                        worksheet.Cells[row, 2, row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, 3].Value = (totalSystemDays == 1 ? totalSystemDays + " ден" : totalSystemDays.ToString() + " дни");
                        worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.White);
                        worksheet.Cells[row, 2, row, 3].Style.Font.Bold = true;

                    }
                    if (rowBeforeOtpuski + 2 <= row)    //ако няма отпуски и болнични да не слага бордер
                    {
                        var modelOtpuski = worksheet.Cells[rowBeforeOtpuski + 2, 2, row, 3];
                        modelOtpuski.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        modelOtpuski.Style.Border.Left.Style = ExcelBorderStyle.Thin;       //border за клетките около болнични и отпуски
                        modelOtpuski.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        modelOtpuski.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        row = rowBeforeOtpuski;
                    }
                }
            }
            else
            {
                var modelTableSumHours = worksheet.Cells[row + 1, 5, row + 1, 6];
                modelTableSumHours.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                modelTableSumHours.Style.Border.Left.Style = ExcelBorderStyle.Thin;       //border за клетките Общо часове
                modelTableSumHours.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                modelTableSumHours.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                modelTableSumHours.Style.Fill.PatternType = ExcelFillStyle.Solid;
                modelTableSumHours.Style.Fill.BackgroundColor.SetColor(Color.White);
                modelTableSumHours.Style.Font.Size = 13;
                modelTableSumHours.Style.Font.Bold = true;
                worksheet.Cells[row + 1, 5].Value = "Брой задачи по които е работено";
                worksheet.Cells[row + 1, 6].Value = 0;

                var rowBeforeOtpuski = row;
                var totalSystemDays = 0;
                foreach (var task in tasksList.Where(t => expertsIds.Overlaps(t.Colleagues.Select(e => e.Id).ToList()) && t.TaskTypeName == TaskTypeSystem).OrderBy(t => t.Id))
                {
                    row += 2;
                    totalSystemDays = 0;
                    worksheet.Cells[row, 2].Value = task.TaskName;
                    worksheet.Cells[row, 2, row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 2].Style.Font.Bold = true;
                    worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(task.TaskName == "Отпуски" ? Color.LightBlue : Color.LightSalmon);
                    row += 1;
                    foreach (var eId in expertsIds)
                    {
                        var curentColegue = task.Colleagues.Where(cl => cl.Id == eId).FirstOrDefault();
                        if (curentColegue != null)
                        {
                            var daysontask = curentColegue.TaskWorkedHours / 8;
                            if (daysontask != null)
                            {
                                worksheet.Cells[row, 2].Value = curentColegue.FullName;
                                worksheet.Cells[row, 3].Value = (daysontask > 1 ? daysontask.ToString() + " дни" : daysontask.ToString() + " ден");
                                totalSystemDays += daysontask.HasValue ? daysontask.Value : 0;
                                row += 1;
                            }
                        }

                    }
                    worksheet.Cells[row, 2].Value = "Общо дни";
                    worksheet.Cells[row, 2, row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 3].Value = (totalSystemDays == 1 ? totalSystemDays + " ден" : totalSystemDays.ToString() + " дни");
                    worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.White);
                    worksheet.Cells[row, 2, row, 3].Style.Font.Bold = true;

                }
                if (rowBeforeOtpuski + 2 <= row)    //ако няма отпуски и болнични да не слага бордер
                {
                    var modelOtpuski = worksheet.Cells[rowBeforeOtpuski + 2, 2, row, 3];
                    modelOtpuski.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    modelOtpuski.Style.Border.Left.Style = ExcelBorderStyle.Thin;       //border за клетките около болнични и отпуски
                    modelOtpuski.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    modelOtpuski.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    row = rowBeforeOtpuski;
                }
            }
        }

        private string GetSufix(int column)
        {
            var reminder = column % 26;
            return ((char)('A' + reminder)).ToString();

        }

        private string GetSufixByEmpCount(int columns)
        {
            string result = columns switch
            {
                _ when columns <= 27 => string.Empty,
                _ when columns <= 53 => "A",
                _ when columns <= 79 => "B",
                _ when columns <= 105 => "C",
                _ when columns <= 131 => "D",
                _ when columns <= 157 => "E",
                _ when columns <= 183 => "F",
                _ when columns <= 209 => "G",
                _ when columns <= 235 => "H",
                _ when columns <= 261 => "I",
                _ when columns <= 287 => "J",
                _ when columns <= 313 => "K",
                _ when columns <= 339 => "L",
                _ when columns <= 365 => "M",
                _ when columns <= 391 => "N",
                _ when columns <= 417 => "O",
                _ when columns <= 443 => "P",
                _ when columns <= 469 => "Q",
                _ when columns <= 495 => "R",
                _ when columns <= 521 => "S",
                _ when columns <= 547 => "T",
                _ when columns <= 573 => "U",
                _ when columns <= 599 => "V",
                _ when columns <= 625 => "W",
                _ when columns <= 651 => "X",
                _ when columns <= 677 => "Y",
                _ when columns <= 703 => "Z",
                _ when columns <= 729 => "AA",
                _ => "AB"
            };

            return result;
        }

        public IActionResult SetPersonalPeriodDate(int userId = 0)    //employee 1 step
        {
            try
            {
                var myPeriod = new PeriodViewModel();
                if (userId < 1)
                {
                    myPeriod.userId = currentUser.Id;

                }
                else
                {
                    myPeriod.userId = userId;
                }
                return View(myPeriod);
            }
            catch (Exception)
            {
                TempData["Error"] = "[SetPersonalPeriodDate] Грешка при подготовка на модела за отчет";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> PersonalPeriodReport(PeriodViewModel model)   //employee 2 step
        {
            try
            {
                if (model.StartDate.Date > model.EndDate.Date)
                {
                    TempData["Error"] = "[PersonalPeriodReport] Невалидни дати";
                    return RedirectToAction("Index", "Home");
                }

                if (model.userId < 1)
                {
                    TempData["Error"] = "[PersonalPeriodReport] Невалиден потребител";
                    return RedirectToAction("UsersList", "Users");
                }

                model.PersonalDateList = await this.employees.GetPersonalReport(model.userId, model.StartDate.Date, model.EndDate.Date);
                model.CheckRegistrationDate = this.dateConfiguration.CheckRegistrationDate;

                if (model.PersonalDateList.WorkedHoursByTaskByPeriod.Count < 1)
                {
                    TempData["Error"] = $"Няма задачи по които да е работил/а {model.PersonalDateList.FullName} за избрания период";
                    return RedirectToAction("UsersList", "Users");
                }
                return View(model);
                //return ExportSpravkaForEmployee(employeeWorkForPeriod, model.StartDate.Date, model.EndDate.Date);
            }
            catch (Exception)
            {
                TempData["Error"] = "[PersonalPeriodReport] Грешка при обработката на модела за отчет";
                return RedirectToAction("UsersList", "Users");
            }


        }

        public async Task<IActionResult> PhonesAndEmails()
        {
            try
            {
                var users = await this.employees.GetAllUsers(false);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var stream = new System.IO.MemoryStream();
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Test");
                    worksheet.View.ZoomScale = 100;
                    try
                    {
                        worksheet.Name = "Телефонен указател";
                    }
                    catch (Exception)
                    {
                        TempData["Error"] = "Грешка при създаването на tab";
                        return RedirectToAction("Index", "Home");
                    }
                    worksheet.Cells[1, 1, 1, 5].Merge = true;
                    worksheet.Row(1).Height = 25;
                    worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //worksheet.Cells[1, 1].Style.Indent = 10;
                    worksheet.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Cells[1, 1].Value = "Телефонен и email указател към дата: " + DateTime.Now.Date.ToString("dd/MM/yyyy");
                    worksheet.Cells[1, 1].Style.Font.Size = 14;
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    int row = 3;
                    //int col = 1;
                    worksheet.Column(1).Width = 40;
                    worksheet.Column(2).Width = 50;
                    worksheet.Column(3).Width = 16;
                    worksheet.Column(4).Width = 16;
                    worksheet.Column(5).Width = 33;
                    worksheet.Row(row).Style.Indent = 1;
                    worksheet.Row(row).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Row(row).Style.Font.Size = 13;
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Value = "Длъжност";
                    worksheet.Cells[row, 2].Style.Font.Bold = true;
                    worksheet.Cells[row, 2].Value = "Име";
                    worksheet.Cells[row, 3].Style.Font.Bold = true;
                    worksheet.Cells[row, 3].Value = "Телефон";
                    worksheet.Cells[row, 4].Style.Font.Bold = true;
                    worksheet.Cells[row, 4].Value = "Мобилен";
                    worksheet.Cells[row, 5].Style.Font.Bold = true;
                    worksheet.Cells[row, 5].Value = "Email";
                    row += 1;

                    var directorates = users.Select(u => u.DirectorateId).ToHashSet();
                    foreach (var dirId in directorates)
                    {
                        worksheet.Cells[row, 1, row, 5].Merge = true;
                        worksheet.Row(row).Height = 15;
                        worksheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        //worksheet.Cells[row, 1].Style.Indent = 10;
                        worksheet.Cells[row, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        worksheet.Cells[row, 1].Value = users.Where(u => u.DirectorateId == dirId).Select(u => !string.IsNullOrWhiteSpace(u.DirectorateName) ? u.DirectorateName.ToUpper() : "").FirstOrDefault();
                        worksheet.Cells[row, 1].Style.Font.Size = 14;
                        worksheet.Cells[row, 1].Style.Font.Bold = true;
                        row += 1;
                        var directors = users.Where(u => u.DirectorateId == dirId && u.DepartmentId == null && u.SectorId == null).OrderByDescending(d => d.JobTitleName).ToList();
                        var direktorBoss = directors.Where(u => u.JobTitleName == "Директор" || u.JobTitleName == "Ръководител Инспекторат" || u.JobTitleName == "Председател" || u.JobTitleName == "Министър" || u.JobTitleName == "Министър председател" || u.JobTitleName == "Секретар").FirstOrDefault();
                        if (direktorBoss != null)
                        {
                            worksheet.Row(row).Style.Indent = 1;
                            worksheet.Row(row).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            worksheet.Row(row).Style.Font.Size = 13;
                            worksheet.Cells[row, 1].Style.Font.Bold = true;
                            worksheet.Cells[row, 1].Value = direktorBoss.JobTitleName;
                            worksheet.Cells[row, 2].Style.Font.Bold = true;
                            worksheet.Cells[row, 2].Value = direktorBoss.FullName;
                            worksheet.Cells[row, 3].Value = direktorBoss.TelephoneNumber;
                            worksheet.Cells[row, 4].Value = direktorBoss.MobileNumber;
                            row += 1;
                            directors.Remove(direktorBoss);
                        }
                        foreach (var user in directors)
                        {
                            worksheet.Row(row).Style.Indent = 1;
                            worksheet.Row(row).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            worksheet.Row(row).Style.Font.Size = 13;
                            worksheet.Cells[row, 1].Value = user.JobTitleName;
                            worksheet.Cells[row, 2].Value = user.FullName;
                            worksheet.Cells[row, 3].Value = user.TelephoneNumber;
                            worksheet.Cells[row, 4].Value = user.MobileNumber;
                            worksheet.Cells[row, 5].Value = user.Email;
                            row += 1;
                        }
                        var departments = users.Where(u => u.DirectorateId == dirId && u.DepartmentId != null).Select(u => u.DepartmentId).ToHashSet();
                        foreach (var depId in departments)
                        {
                            worksheet.Cells[row, 1, row, 5].Merge = true;
                            worksheet.Row(row).Height = 15;
                            worksheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                            worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            //worksheet.Cells[row, 1].Style.Indent = 12;
                            worksheet.Cells[row, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            worksheet.Cells[row, 1].Value = "Отдел: " + users.Where(u => u.DirectorateId == dirId && u.DepartmentId == depId).Select(u => !string.IsNullOrWhiteSpace(u.DepartmentName) ? u.DepartmentName.ToUpper() : "").FirstOrDefault();
                            worksheet.Cells[row, 1].Style.Font.Size = 13;
                            worksheet.Cells[row, 1].Style.Font.Bold = true;
                            row += 1;
                            var expertsInDepartment = users.Where(u => u.DirectorateId == dirId && u.DepartmentId == depId && u.SectorId == null).OrderBy(d => d.JobTitleName).ToList();
                            var departmentBoss = expertsInDepartment.Where(u => u.JobTitleName == "Началник отдел").FirstOrDefault();
                            if (departmentBoss != null)
                            {
                                worksheet.Row(row).Style.Indent = 1;
                                worksheet.Row(row).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                worksheet.Row(row).Style.Font.Size = 13;
                                worksheet.Cells[row, 1].Style.Font.Bold = true;
                                worksheet.Cells[row, 1].Value = departmentBoss.JobTitleName;
                                worksheet.Cells[row, 2].Style.Font.Bold = true;
                                worksheet.Cells[row, 2].Value = departmentBoss.FullName;
                                worksheet.Cells[row, 3].Value = departmentBoss.TelephoneNumber;
                                worksheet.Cells[row, 4].Value = departmentBoss.MobileNumber;
                                worksheet.Cells[row, 5].Value = departmentBoss.Email;
                                row += 1;
                                expertsInDepartment.Remove(departmentBoss);
                            }
                            foreach (var userInDepartment in expertsInDepartment)
                            {
                                worksheet.Row(row).Style.Indent = 1;
                                worksheet.Row(row).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                worksheet.Row(row).Style.Font.Size = 13;
                                worksheet.Cells[row, 1].Value = userInDepartment.JobTitleName;
                                worksheet.Cells[row, 2].Value = userInDepartment.FullName;
                                worksheet.Cells[row, 3].Value = userInDepartment.TelephoneNumber;
                                worksheet.Cells[row, 4].Value = userInDepartment.MobileNumber;
                                worksheet.Cells[row, 5].Value = userInDepartment.Email;
                                row += 1;
                            }
                            var sectors = users.Where(u => u.DirectorateId == dirId && u.DepartmentId == depId && u.SectorId != null).Select(u => u.SectorId).ToHashSet();
                            foreach (var sectorId in sectors)
                            {
                                worksheet.Cells[row, 1, row, 5].Merge = true;
                                worksheet.Row(row).Height = 15;
                                worksheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                                worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                //worksheet.Cells[row, 1].Style.Indent = 13;
                                worksheet.Cells[row, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                worksheet.Cells[row, 1].Value = "Сектор: " + users.Where(u => u.DirectorateId == dirId && u.DepartmentId == depId && u.SectorId == sectorId).Select(u => !string.IsNullOrWhiteSpace(u.SectorName) ? u.SectorName.ToUpper() : "").FirstOrDefault();
                                worksheet.Cells[row, 1].Style.Font.Size = 13;
                                worksheet.Cells[row, 1].Style.Font.Bold = true;
                                row += 1;
                                var expertsInSector = users.Where(u => u.DirectorateId == dirId && u.DepartmentId == depId && u.SectorId == sectorId).OrderBy(d => d.JobTitleName).ToList();
                                var sectorBoss = expertsInSector.Where(u => u.JobTitleName == "Началник сектор").FirstOrDefault();
                                if (sectorBoss != null)
                                {
                                    worksheet.Row(row).Style.Indent = 1;
                                    worksheet.Row(row).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                    worksheet.Row(row).Style.Font.Size = 13;
                                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                                    worksheet.Cells[row, 1].Value = sectorBoss.JobTitleName;
                                    worksheet.Cells[row, 2].Style.Font.Bold = true;
                                    worksheet.Cells[row, 2].Value = sectorBoss.FullName;
                                    worksheet.Cells[row, 3].Value = sectorBoss.TelephoneNumber;
                                    worksheet.Cells[row, 4].Value = sectorBoss.MobileNumber;
                                    worksheet.Cells[row, 5].Value = sectorBoss.Email;
                                    row += 1;
                                    expertsInSector.Remove(sectorBoss);
                                }
                                foreach (var userInSector in expertsInSector)
                                {
                                    worksheet.Row(row).Style.Indent = 1;
                                    worksheet.Row(row).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                    worksheet.Row(row).Style.Font.Size = 13;
                                    worksheet.Cells[row, 1].Value = userInSector.JobTitleName;
                                    worksheet.Cells[row, 2].Value = userInSector.FullName;
                                    worksheet.Cells[row, 3].Value = userInSector.TelephoneNumber;
                                    worksheet.Cells[row, 4].Value = userInSector.MobileNumber;
                                    worksheet.Cells[row, 5].Value = userInSector.Email;
                                    row += 1;
                                }

                            }
                        }
                    }
                    worksheet.View.ShowGridLines = true;
                    worksheet.PrinterSettings.PaperSize = ePaperSize.A4;
                    worksheet.PrinterSettings.ShowGridLines = true;
                    worksheet.PrinterSettings.Orientation = eOrientation.Portrait;
                    worksheet.PrinterSettings.FitToWidth = 1;
                    package.Save();
                }
                string fileName = "PhoneBook_" + DateTime.Now.ToShortTimeString() + ".xlsx";
                string fileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                stream.Position = 0;
                return File(stream, fileType, fileName);

            }
            catch (Exception)
            {
                TempData["Error"] = "[PhonesAndEmails] Грешка при подготовка на модела за тел. указател";
                return RedirectToAction("Index", "Home");
            }
        }

        #region API Calls

        #endregion

    }
}
