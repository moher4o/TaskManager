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
                    TempData[ErrorMessageKey] = "Няма задачи по които да е работено от избраните експерти за избрания период";
                    return RedirectToAction(nameof(PeriodReport));
                }

                //var selectedExperts = tasksList.FirstOrDefault().Colleagues.ToList();
                var allExperts = new List<ReportUserServiceModel>();
                tasksList.ForEach(t => allExperts.AddRange(t.Colleagues));  //списък на всички експерти по всички задачи
                var directorate = new SelectServiceModel()
                {
                    Id = allExperts.FirstOrDefault().DirectorateId ?? 0,
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
                    Color colLightGreen = System.Drawing.ColorTranslator.FromHtml("lightgreen");
                    Color colLightBlue = System.Drawing.ColorTranslator.FromHtml("lightblue");

                    foreach (var department in departmentsList)
                    {     //allExperts.Select(s => s.DepartmentId).ToList().Distinct().Contains(department.Id)
                        if (allExperts.Where(s => s.DepartmentId == department.Id).ToList().Count() > 0)   //ако има сред избраните експерти членове на отдела
                        {
                            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(department.TextValue);

                            worksheet.Column(1).Width = 15;
                            worksheet.Cells["A1:E1"].Merge = true;
                            worksheet.Row(1).Height = 25;
                            worksheet.Row(1).Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Row(1).Style.Fill.BackgroundColor.SetColor(colLightGreen);
                            worksheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            worksheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            worksheet.Cells[1, 1].Value = "Дирекция: \""+directorate.TextValue+ "\"  ("+model.StartDate.Date.ToString("DD/MM/yyyy")+
                                "г. - "+ model.EndDate.Date.ToString("DD/MM/yyyy") +"г.)";
                            worksheet.Row(1).Style.Font.Size = 18;
                            worksheet.Row(1).Style.Font.Bold = true;

                            worksheet.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            worksheet.Row(2).Height = 35;
                            worksheet.Row(2).Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Row(2).Style.Fill.BackgroundColor.SetColor(colLightBlue);
                            worksheet.Row(1).Style.Font.Size = 14;
                            worksheet.Cells[2, 1].Value = "№";
                            worksheet.Column(1).Width = 10;
                            worksheet.Cells[2, 2].Value = "Наименование и пояснения към задачата";
                            worksheet.Column(2).Width = 60;
                            worksheet.Cells[2, 3].Value = "Начална Дата";
                            worksheet.Column(3).Width = 15;
                            worksheet.Column(3).Style.Numberformat.Format = "dd-MM-yyyy";
                            worksheet.Cells[2, 4].Value = "Краен срок";
                            worksheet.Column(4).Width = 15;
                            worksheet.Column(4).Style.Numberformat.Format = "dd-MM-yyyy";
                            worksheet.Cells[2, 5].Value = "Описание \\ Резултати";
                            worksheet.Column(5).Width = 60;
                            int column = 6;
                            //foreach (var expert in allExperts.Where(e => e.DepartmentId == department.Id && e.SectorId == null))
                            //{
                            //    worksheet.Cells[2, column].Value = expert.FullName;
                            //    worksheet.Cells[2, column].Style.ShrinkToFit = true;
                            //    column += 1;
                            //}
                        }
                    }

                    //Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#B7DEE8");
                    //ws.Cells["A1:B1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //ws.Cells["A1:B1"].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    //ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Релация");
                    //worksheet.Cells[1, 1].Value = "Релация " + relItemsList[0].RelationTypeName;
                    //worksheet.Row(1).Style.Font.Size = 16;
                    //worksheet.Row(1).Style.Font.Bold = true;

                    //worksheet.Cells[2, 1].Value = "Код на класификация източник";
                    //worksheet.Cells[2, 2].Value = "Код на версия източник";
                    //worksheet.Cells[2, 3].Value = "Код на елемент източник";
                    //worksheet.Cells[2, 4].Value = "Име на елемент източник";
                    //worksheet.Cells[2, 5].Value = "Код на класификация цел";
                    //worksheet.Cells[2, 6].Value = "Код на версия цел";
                    //worksheet.Cells[2, 7].Value = "Код на елемент цел";
                    //worksheet.Cells[2, 8].Value = "Име на елемент цел";
                    //worksheet.Row(2).Style.Font.Size = 12;
                    //worksheet.Row(2).Style.Font.Bold = true;

                    //for (int c = 3; c < relItemsList.Count + 3; c++)
                    //{
                    //    worksheet.Cells[c, 1].Value = relItemsList[c - 3].SrcClassif;
                    //    worksheet.Cells[c, 2].Value = relItemsList[c - 3].SrcVer;
                    //    worksheet.Cells[c, 3].Value = relItemsList[c - 3].SrcItemId;
                    //    worksheet.Cells[c, 4].Value = relItemsList[c - 3].SrcItemName;
                    //    worksheet.Cells[c, 5].Value = relItemsList[c - 3].DestClassif;
                    //    worksheet.Cells[c, 6].Value = relItemsList[c - 3].DestVer;
                    //    worksheet.Cells[c, 7].Value = relItemsList[c - 3].DestItemId;
                    //    worksheet.Cells[c, 8].Value = relItemsList[c - 3].DestItemName;

                    //    worksheet.Row(c).Style.Font.Size = 12;
                    //}


                    package.Save();
                }

                string fileName = "Report_" +DateTime.Now.ToShortTimeString()+".xlsx";
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
