using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Data;
using TaskManager.Services.Models;
using TaskMenager.Client.Models.InsertDataFromFile;
using static TaskManager.Common.DataConstants;
using NPOI.SS.UserModel;
using TaskManager.Services;
using Microsoft.AspNetCore.Authorization;
using TaskManager.Common;
using Microsoft.AspNetCore.Hosting;

namespace TaskMenager.Client.Controllers
{
    [Authorize(Policy = "Developer")]
    public class InsertDataFromFileController : BaseController
    {

        private readonly ITitleService titles;
        //private readonly IRolesService roles;
        private readonly IDirectorateService directorates;
        private readonly IDepartmentsService departments;
        private readonly ISectorsService sectors;
        
        public InsertDataFromFileController(IRolesService roles, ITitleService titles, IEmployeesService employees, ITasksService tasks, IDepartmentsService departments, ISectorsService sectors, IDirectorateService directorates, IHttpContextAccessor httpContextAccessor, IEmailService email, IWebHostEnvironment env) : base(httpContextAccessor, employees, tasks, email, env)
        {
            //this.roles = roles;
            this.titles = titles;
            this.directorates = directorates;
            this.departments = departments;
            this.sectors = sectors;
        }

        //private IConfiguration Configuration { get; }

        
        public IActionResult SelectFileWithData()
        {
            var br = this.User.Claims.Count();
            var FileTypeSelected = new SelectedFileToInsertViewModel();
            FileTypeSelected.FileTypes.Insert(0, new SelectListItem
            {
                Text = ChooseValue,
                Value = ChooseValue,
                Selected = true
            });
            FileTypeSelected.FileTypes.Insert(1, new SelectListItem
            {
                Text = "Длъжности",
                Value = "Длъжности"
            });
            FileTypeSelected.FileTypes.Insert(1, new SelectListItem
            {
                Text = "Дирекции",
                Value = "Дирекции"
            });
            FileTypeSelected.FileTypes.Insert(1, new SelectListItem
            {
                Text = "Отдели",
                Value = "Отдели"
            });
            FileTypeSelected.FileTypes.Insert(1, new SelectListItem
            {
                Text = "Сектори",
                Value = "Сектори"
            });
            FileTypeSelected.FileTypes.Insert(1, new SelectListItem
            {
                Text = "Служители",
                Value = "Служители"
            });

            return View(FileTypeSelected);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectFileType(IFormFile newItems, string filetypeselected)
        {
            try
            {

                if (filetypeselected == null)
                {
                    TempData["Error"] = "Не е избран вид на входния файл";
                }
                if (newItems == null)
                {
                    TempData["Error"] = "Не е избран файл";
                    return RedirectToAction("SelectFileWithData");
                }

                var fileType = GetMIMEType(newItems.FileName);
                if (fileType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" || fileType == "application/vnd.ms-excel")
                {
                        return await ImportData(newItems, filetypeselected);
                }
                else
                {
                    TempData["Error"] = "Не е избран Excel файл";
                    return RedirectToAction("SelectFileWithData");
                }

            }
            catch (Exception)
            {

                TempData["Error"] = "Хмм...Необработена грешка.";
                return RedirectToAction("SelectFileWithData");
            }

        }

        private async Task<IActionResult> ImportData(IFormFile file, string filetypeselected)
        {
            try
            {
                using (var fs = file.OpenReadStream())
                {

                    //var userName = this.User.Identities.FirstOrDefault().Name;
                    var wb = WorkbookFactory.Create(fs); // фабрика, която създава IWorkbook от xls или xlsx(удобно, защото не се знае какъв файл ще зареди потребителя)
                    ISheet sheet = wb.GetSheetAt(0);
                    var result = string.Empty;
                    if (filetypeselected == "Длъжности")
                    {
                        result = await ImportJobTitlesToDb(sheet);
                    }
                    else if (filetypeselected == "Дирекции")
                    {
                        result = await ImportDirectorates(sheet);
                    }
                    else if (filetypeselected == "Отдели")
                    {
                        result = await ImportDepartments(sheet);
                    }
                    else if (filetypeselected == "Сектори")
                    {
                        result = await ImportSectors(sheet);
                    }
                    else if (filetypeselected == "Служители")
                    {
                        result = await ImportEmployees(sheet);
                    }
                    else
                    {
                        TempData["Error"] = "Няма такъв тип данни: " + filetypeselected;
                    }
                    if (result != "success")
                    {
                        TempData["Error"] = result;
                    }
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Основна грешка!";
            }
            if (TempData["Error"] == null)
            {
                TempData["Success"] = "Данните са добавени успешно!";
            }
            return RedirectToAction("SelectFileWithData");
        }

        private async Task<string> ImportEmployees(ISheet sheet)
        {
            var items = new List<AddNewEmployeeServiceModel>();
            for (int i = sheet.FirstRowNum + 1; i <= sheet.LastRowNum; i++) //Read Excel File
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;
                if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

                try
                {
                    var currentEmployee = new AddNewEmployeeServiceModel();
                    var cellCount = row.LastCellNum;
                    if (cellCount != 7)
                    {
                        TempData["Error"] = $"Възникна проблем на ред {i + 1} при четене от файла. Трябват седем колони с данни за служителя.";
                        break;
                    }

                    currentEmployee.FullName = string.IsNullOrWhiteSpace(row.GetCell(0).ToString()) ? throw new ArgumentNullException("Имената на служителя не може да са null") : row.GetCell(0).ToString();
                    currentEmployee.isDeleted = false;
                    currentEmployee.JobTitle = string.IsNullOrWhiteSpace(row.GetCell(1).ToString()) ? throw new ArgumentNullException("Длъжността не може да е null") : row.GetCell(1).ToString();
                    currentEmployee.Directorate = row.GetCell(2).ToString();
                    currentEmployee.Department = row.GetCell(3).ToString();
                    currentEmployee.Sector = row.GetCell(4).ToString();
                    currentEmployee.DaeuAccaunt = string.IsNullOrWhiteSpace(row.GetCell(5).ToString()) ? throw new ArgumentNullException("Акаунта не може да е null") : row.GetCell(5).ToString();
                    currentEmployee.Role = string.IsNullOrWhiteSpace(row.GetCell(6).ToString()) ? throw new ArgumentNullException("Ролята не може да е null") : row.GetCell(6).ToString();

                    items.Add(currentEmployee);
                }
                catch (Exception ex)
                {
                    return $"Възникна проблем на ред {i + 1} при четене от файла :" + ex.Message;
                }

            }
            return await this.employees.AddEmployeesCollection(items);
        }

        private async Task<string> ImportSectors(ISheet sheet)
        {
            var items = new List<AddNewSectorServiceModel>();
            for (int i = sheet.FirstRowNum + 1; i <= sheet.LastRowNum; i++) //Read Excel File
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;
                if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

                try
                {
                    var currentSector = new AddNewSectorServiceModel();
                    var cellCount = row.LastCellNum;
                    if (cellCount != 2)
                    {
                        TempData["Error"] = $"Възникна проблем на ред {i + 1} при четене от файла. Трябват две колони с данни за секторите.";
                        break;
                    }

                    int tempInt;

                    bool intParseSuccessfull = int.TryParse(row.GetCell(0).ToString(), out tempInt);

                    if (!intParseSuccessfull)
                    {
                        return $"Възникна проблем на ред {i + 1} при парсване на Id на отдел";
                    }
                    else
                    {
                        currentSector.DepartmentId = tempInt;
                    }


                    currentSector.Name = row.GetCell(1).ToString();
                    currentSector.isDeleted = false;

                    items.Add(currentSector);
                }
                catch (Exception)
                {
                    return $"Възникна проблем на ред {i + 1} при четене от файла";
                }

            }
            return await this.sectors.AddSectorsCollection(items);
        }

        private async Task<string> ImportDepartments(ISheet sheet)
        {
            var items = new List<AddNewDepartmentServiceModel>();
            for (int i = sheet.FirstRowNum + 1; i <= sheet.LastRowNum; i++) //Read Excel File
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;
                if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

                try
                {
                    var currentDepartment = new AddNewDepartmentServiceModel();
                    var cellCount = row.LastCellNum;
                    if (cellCount != 2)
                    {
                        TempData["Error"] = $"Възникна проблем на ред {i + 1} при четене от файла. Трябват две колони с данни за отделите.";
                        break;
                    }

                    int directorateTemp;

                    bool intParseSuccessfull = int.TryParse(row.GetCell(0).ToString(), out directorateTemp);

                    if (!intParseSuccessfull)
                    {
                        return $"Възникна проблем на ред {i+1} при парсване на Id на дирекция";
                    }
                    else
                    {
                        currentDepartment.DirectorateId = directorateTemp;
                    }

                    currentDepartment.Name = row.GetCell(1).ToString();
                    currentDepartment.isDeleted = false;

                    items.Add(currentDepartment);
                }
                catch (Exception)
                {
                    return $"Възникна проблем на ред {i + 1} при четене от файла";
                }

            }
            return await this.departments.AddDepartmentsCollection(items);
        }

        //private async Task<string> ImportRoles(ISheet sheet)
        //{
        //    var items = new List<AddNewRoleServiceModel>();
        //    for (int i = sheet.FirstRowNum + 1; i <= sheet.LastRowNum; i++) //Read Excel File
        //    {
        //        IRow row = sheet.GetRow(i);
        //        if (row == null) continue;
        //        if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

        //        try
        //        {
        //            var currentRole = new AddNewRoleServiceModel();
        //            var cellCount = row.LastCellNum;
        //            if (cellCount != 1)
        //            {
        //                TempData["Error"] = $"Възникна проблем на ред {i + 1} при четене от файла. Трябва поне една колона с данни за ролите.";
        //                break;
        //            }
        //            //currentDirectorate.DirectorateId = row.GetCell(0).ToString();
        //            currentRole.Name = row.GetCell(0).ToString();
        //            currentRole.isDeleted = false;

        //            items.Add(currentRole);
        //        }
        //        catch (Exception)
        //        {
        //            return $"Възникна проблем на ред {i + 1} при четене от файла";
        //        }

        //    }
        //    return await this.roles.AddRolesCollection(items);
        //}

        private async Task<string> ImportDirectorates(ISheet sheet)
        {
            var items = new List<AddNewDirectorateServiceModel>();
            for (int i = sheet.FirstRowNum + 1; i <= sheet.LastRowNum; i++) //Read Excel File
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;
                if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

                try
                {
                    var currentDirectorate = new AddNewDirectorateServiceModel();
                    var cellCount = row.LastCellNum;
                    if (cellCount != 1)
                    {
                        TempData["Error"] = $"Възникна проблем на ред {i + 1} при четене от файла. Трябва поне една колона с данни за дирекциите.";
                        break;
                    }
                    //currentDirectorate.DirectorateId = row.GetCell(0).ToString();
                    currentDirectorate.Name = row.GetCell(0).ToString();
                    currentDirectorate.isDeleted = false;

                    items.Add(currentDirectorate);
                }
                catch (Exception)
                {
                    return $"Възникна проблем на ред {i + 1} при четене от файла";
                }

            }
            return await this.directorates.AddDirectoratesCollection(items);
        }

        private async Task<string> ImportJobTitlesToDb(ISheet sheet)
        {
            var items = new List<AddNewJobTitlesServiceModel>();
            for (int i = sheet.FirstRowNum + 1; i <= sheet.LastRowNum; i++) //Read Excel File
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;
                if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

                try
                {
                    var currentTitle = new AddNewJobTitlesServiceModel();
                    var cellCount = row.LastCellNum;
                    if (cellCount < 1)
                    {
                        TempData["Error"] = $"Възникна проблем на ред {i + 1} при четене от файла. Трябва поне една колона с данни за длъжностите.";
                        break;
                    }
                    //currentTitle.JobTitleId = row.GetCell(0).ToString();
                    currentTitle.Name = row.GetCell(0).ToString();
                    currentTitle.isDeleted = false;

                    items.Add(currentTitle);
                }
                catch (Exception)
                {
                    return $"Възникна проблем на ред {i + 1} при четене от файла";
                }

            }
            return await this.titles.AddTitlesCollection(items);
        }


        //private async Task<IActionResult> ImportClassItems(IFormFile file, string classCode, string versionCode)
        //{
        //    try
        //    {
        //        using (var fs = file.OpenReadStream())
        //        {

        //            int orderNo;
        //            var currentuser = await this.userManager.GetUserAsync(User);
        //            var items = new List<AddNewClassItemsServiceModel>();
        //            //HSSFWorkbook hssfwb = new HSSFWorkbook(fs); //чете xls
        //            //XSSFWorkbook xssfwb = new XSSFWorkbook(fs); // чете xlsx
        //            var wb = WorkbookFactory.Create(fs); // фабрика, която създава IWorkbook от xls или xlsx(удобно, защото не се знае какъв файл ще зареди потребителя)
        //            ISheet sheet = wb.GetSheetAt(0);

        //            for (int i = sheet.FirstRowNum + 1; i <= sheet.LastRowNum; i++) //Read Excel File
        //            {
        //                IRow row = sheet.GetRow(i);
        //                if (row == null) continue;
        //                if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

        //                try
        //                {
        //                    var currentItem = new AddNewClassItemsServiceModel();

        //                    currentItem.Classif = classCode;
        //                    currentItem.Version = versionCode;
        //                    currentItem.ItemCode = row.GetCell(0).ToString();
        //                    currentItem.Description = row.GetCell(1).ToString();
        //                    currentItem.DescriptionShort = currentItem.Description;
        //                    currentItem.DescriptionEng = row.GetCell(2).ToString();
        //                    bool intParseSuccessfull = int.TryParse(row.GetCell(3).ToString(), out orderNo);
        //                    if (intParseSuccessfull)
        //                    {
        //                        currentItem.OrderNo = orderNo;
        //                    }

        //                    if (this.classification.IsClassificationHierachical(classCode))
        //                    {
        //                        currentItem.ParentItemCode = row.GetCell(4) != null ? row.GetCell(4).ToString() : null;
        //                    }


        //                    intParseSuccessfull = int.TryParse(row.GetCell(5).ToString(), out orderNo);
        //                    if (intParseSuccessfull)
        //                    {
        //                        currentItem.ItemLevel = orderNo;
        //                    }
        //                    else
        //                    {
        //                        currentItem.ItemLevel = 9999;
        //                    }
        //                    string isLeaf = row.GetCell(6).ToString();
        //                    if (isLeaf == "Y" || isLeaf == "1")
        //                    {
        //                        currentItem.IsLeaf = true;
        //                    }
        //                    else
        //                    {
        //                        currentItem.IsLeaf = false;
        //                    }

        //                    currentItem.EnteredByUserId = currentuser.Id;

        //                    currentItem.EntryTime = DateTime.UtcNow;

        //                    items.Add(currentItem);
        //                }
        //                catch (Exception)
        //                {
        //                    TempData[ErrorMessageKey] = $"Възникна проблем на ред {i + 1} при четене от файла";
        //                    break;
        //                }

        //            }
        //            var result = await classification.AddItemsCollection(items);
        //            if (result != "success")
        //            {
        //                TempData[ErrorMessageKey] = result;
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        TempData[ErrorMessageKey] = "Основна грешка!";
        //    }
        //    if (TempData[ErrorMessageKey] == null)
        //    {
        //        TempData[SuccessMessageKey] = "Елементите на класификацията са добавени успешно!";
        //    }
        //    return RedirectToAction("AdminTasks", "Users", new { area = "Admin" });
        //}
    }
}
