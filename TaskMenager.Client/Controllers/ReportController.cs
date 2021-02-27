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
using TaskMenager.Client.Models.Report;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskManager.Services.Models;
using System.Drawing;
using OfficeOpenXml.Style;
using TaskManager.Services.Models.TaskModels;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Drawing.Chart.Style;

namespace TaskMenager.Client.Controllers
{
    public class ReportController : BaseController
    {
        private readonly IDirectorateService directorates;
        private readonly IDepartmentsService departments;
        private readonly ISectorsService sectors;

        public ReportController(IDirectorateService directorates, IDepartmentsService departments, ISectorsService sectors, IEmployeesService employees, ITasksService tasks, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor, employees, tasks)
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
                TempData["Error"] = "Грешка при подготовка на модела за отчет";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> GetReport(PeriodReportViewModel model)
        {
            try
            {

                var newReport = new PeriodReportViewModel();
                var employeesIds = new List<int>();
                if (currentUser.RoleName == Employee)
                {
                    employeesIds.Add(currentUser.Id);
                }
                else if (currentUser.RoleName == SectorAdmin)
                {
                    employeesIds = this.employees.GetEmployeesNamesBySector(currentUser.SectorId)
                                                        .Select(e => e.Id)
                                                        .ToList();
                }
                else if (currentUser.RoleName == DepartmentAdmin)
                {
                    employeesIds = this.employees.GetEmployeesNamesByDepartment(currentUser.DepartmentId)
                                                        .Select(e => e.Id)
                                                        .ToList();
                }
                else if (currentUser.RoleName == DirectorateAdmin)
                {
                    employeesIds = this.employees.GetEmployeesNamesByDirectorate(currentUser.DirectorateId)
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
                    employeesIds = this.employees.GetEmployeesNamesByDirectorate(int.Parse(model.DirectoratesId))
                                        .Select(e => e.Id)
                                        .ToList();
                }
                newReport.EmployeesIds = employeesIds.ToArray();
                if (model.StartDate > model.EndDate)
                {
                    TempData["Error"] = "Невалидни дати";
                    return RedirectToAction(nameof(PeriodReport));
                }
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
                    // TODO
                }
                directorate.TextValue = this.directorates.GetDirectoratesNames(directorate.Id).FirstOrDefault().TextValue;   //дирекцията за която ще е отчета

                var departmentsList = this.departments.GetDepartmentsNamesByDirectorate(directorate.Id);  //отделите в дирекцията

                var sectorsList = await this.sectors.GetSectorsNamesByDirectorate(directorate.Id);

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var stream = new System.IO.MemoryStream();
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    foreach (var department in departmentsList)
                    {     //allExperts.Select(s => s.DepartmentId).ToList().Distinct().Contains(department.Id)
                        if (allExperts.Where(s => s.DepartmentId == department.Id).ToList().Count() > 0)   //ако има експерти в отдела
                        {
                            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Test");
                            worksheet.View.ZoomScale = 80;
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
                                    TaskServiceModel parentTask = await this.tasks.GetTaskAsync(task.ParentTaskId.Value);
                                    if (parentTask != null)
                                    {
                                        worksheet.Row(row).Style.Font.Size = 12;
                                        worksheet.Row(row).Style.Font.Bold = true;
                                        worksheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
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
                                worksheet.Cells[row, taskExpertsColumn].Formula = "=SUM(F" + (row).ToString() + ":" + ((char)('A' + (taskExpertsColumn - 2))).ToString() + (row).ToString() + ")";  //общо часове по задачата
                                worksheet.Cells[row, taskExpertsColumn].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[row, taskExpertsColumn].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                                worksheet.Cells[row, taskExpertsColumn + 1].Formula = "=COUNTA(F" + (row).ToString() + ":" + ((char)('A' + (taskExpertsColumn - 2))).ToString() + (row).ToString() + ")";  //общо eksperti по задачата
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
                                worksheet.Cells[row, col].Formula = "=COUNTA(" + ((char)('A' + (col - 1))).ToString() + "3:" + ((char)('A' + (col - 1))).ToString() + (row - 1).ToString() + ")";
                            }
                            worksheet.Cells[row, taskExpertsColumn].Formula = "=AVERAGE(F" + (row).ToString() + ":" + ((char)('A' + (taskExpertsColumn - 2))).ToString() + (row).ToString() + ")";     //по колко задачи средно е работено
                            worksheet.Cells[row, taskExpertsColumn].Style.Numberformat.Format = "0.00";
                            var modelTableBroiZadachi = worksheet.Cells[row, 5, row, taskExpertsColumn];
                            modelTableBroiZadachi.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            modelTableBroiZadachi.Style.Border.Left.Style = ExcelBorderStyle.Thin;       //border за клетките около броя задачи
                            modelTableBroiZadachi.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            modelTableBroiZadachi.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            modelTableBroiZadachi.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            modelTableBroiZadachi.Style.Fill.BackgroundColor.SetColor(Color.Orange);
                            worksheet.Cells[row, taskExpertsColumn + 1].Formula = "=AVERAGE(" + ((char)('A' + (taskExpertsColumn))).ToString() + "3:" + ((char)('A' + (taskExpertsColumn))).ToString() + (row - 1).ToString() + ")";
                            worksheet.Cells[row, taskExpertsColumn + 1].Style.Numberformat.Format = "0.00";
                            worksheet.Cells[row, taskExpertsColumn + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[row, taskExpertsColumn + 1].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);  //средно колко човека са работили по задача

                            row += 1;
                            worksheet.Cells[row, 5].Value = "Общо часове на служителя";
                            for (int col = 6; col < taskExpertsColumn; col++)   // taskExpertsColumn сочи колоната "Общо колко часа е работено..."
                            {
                                worksheet.Cells[row, col].Formula = "=SUM(" + ((char)('A' + (col - 1))).ToString() + "3:" + ((char)('A' + (col - 1))).ToString() + (row - 2).ToString() + ")";
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
                            worksheet.Cells[row, 6].Formula = "=SUM(F" + (row - 1).ToString() + ":" + ((char)('A' + (taskExpertsColumn - 2))).ToString() + (row - 1).ToString() + ")";  //ОБЩО ЧАСОВЕ
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
                                pieChart.StyleManager.SetChartStyle(ePresetChartStyle.Pie3dChartStyle8, ePresetChartColors.ColorfulPalette1);
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
                    foreach (var sector in sectorsList)
                    {
                        if (allExperts.Where(s => s.SectorId == sector.Id).ToList().Count() > 0)   //ако има експерти в sectora
                        {
                            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Test");
                            worksheet.View.ZoomScale = 80;
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
                                    TaskServiceModel parentTask = await this.tasks.GetTaskAsync(task.ParentTaskId.Value);
                                    if (parentTask != null)
                                    {
                                        worksheet.Row(row).Style.Font.Size = 12;
                                        worksheet.Row(row).Style.Font.Bold = true;
                                        worksheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
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
                                worksheet.Cells[row, taskExpertsColumn].Formula = "=SUM(F" + (row).ToString() + ":" + ((char)('A' + (taskExpertsColumn - 2))).ToString() + (row).ToString() + ")";  //общо часове по задачата
                                worksheet.Cells[row, taskExpertsColumn].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[row, taskExpertsColumn].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                                worksheet.Cells[row, taskExpertsColumn + 1].Formula = "=COUNTA(F" + (row).ToString() + ":" + ((char)('A' + (taskExpertsColumn - 2))).ToString() + (row).ToString() + ")";  //общо eksperti по задачата
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
                                worksheet.Cells[row, col].Formula = "=COUNTA(" + ((char)('A' + (col - 1))).ToString() + "3:" + ((char)('A' + (col - 1))).ToString() + (row - 1).ToString() + ")";
                            }
                            worksheet.Cells[row, taskExpertsColumn].Formula = "=AVERAGE(F" + (row).ToString() + ":" + ((char)('A' + (taskExpertsColumn - 2))).ToString() + (row).ToString() + ")";     //по колко задачи средно е работено
                            worksheet.Cells[row, taskExpertsColumn].Style.Numberformat.Format = "0.00";
                            var modelTableBroiZadachi = worksheet.Cells[row, 5, row, taskExpertsColumn];
                            modelTableBroiZadachi.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            modelTableBroiZadachi.Style.Border.Left.Style = ExcelBorderStyle.Thin;       //border за клетките около броя задачи
                            modelTableBroiZadachi.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            modelTableBroiZadachi.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            modelTableBroiZadachi.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            modelTableBroiZadachi.Style.Fill.BackgroundColor.SetColor(Color.Orange);
                            worksheet.Cells[row, taskExpertsColumn + 1].Formula = "=AVERAGE(" + ((char)('A' + (taskExpertsColumn))).ToString() + "3:" + ((char)('A' + (taskExpertsColumn))).ToString() + (row - 1).ToString() + ")";
                            worksheet.Cells[row, taskExpertsColumn + 1].Style.Numberformat.Format = "0.00";
                            worksheet.Cells[row, taskExpertsColumn + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[row, taskExpertsColumn + 1].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);  //средно колко човека са работили по задача

                            row += 1;
                            worksheet.Cells[row, 5].Value = "Общо часове на служителя";
                            for (int col = 6; col < taskExpertsColumn; col++)   // taskExpertsColumn сочи колоната "Общо колко часа е работено..."
                            {
                                worksheet.Cells[row, col].Formula = "=SUM(" + ((char)('A' + (col - 1))).ToString() + "3:" + ((char)('A' + (col - 1))).ToString() + (row - 2).ToString() + ")";
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
                            worksheet.Cells[row, 6].Formula = "=SUM(F" + (row - 1).ToString() + ":" + ((char)('A' + (taskExpertsColumn - 2))).ToString() + (row - 1).ToString() + ")";  //ОБЩО ЧАСОВЕ
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
                                pieChart.StyleManager.SetChartStyle(ePresetChartStyle.Pie3dChartStyle8, ePresetChartColors.ColorfulPalette1);
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
            catch (Exception)
            {
                TempData["Error"] = "Възникна неочаквана грешка!";
                return RedirectToAction("Index", "Home");
            }

        }

    }
}
