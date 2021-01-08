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

namespace TaskMenager.Client.Controllers
{
    public class InsertDataFromFileController : Controller
    {

        private readonly ITitleService titles;
        public InsertDataFromFileController(ITitleService titles, IConfiguration configuration)
        {
            this.titles = titles;
            this.Configuration = configuration;
        }

        private IConfiguration Configuration { get; }


        public IActionResult SelectFileWithData()
        {
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

            return View(FileTypeSelected);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectFileType(IFormFile newItems, string filetypeselected)
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
                //return RedirectToAction("ImportClassItems", "Class", new { newItems, classCode, versionCode });
                return await ImportJobtitles(newItems, filetypeselected);
            }
            else
            {
                TempData[ErrorMessageKey] = "Не е избран Excel файл";
                return RedirectToAction("SelectFileWithData");
            }
        }

        private async Task<IActionResult> ImportJobtitles(IFormFile file, string filetypeselected)
        {
            try
            {
                using (var fs = file.OpenReadStream())
                {

                    int level;

                    //var userName = this.User.Identities.FirstOrDefault().Name;
                    var items = new List<AddNewJobTitlesServiceModel>();
                    //HSSFWorkbook hssfwb = new HSSFWorkbook(fs); //чете xls
                    //XSSFWorkbook xssfwb = new XSSFWorkbook(fs); // чете xlsx
                    var wb = WorkbookFactory.Create(fs); // фабрика, която създава IWorkbook от xls или xlsx(удобно, защото не се знае какъв файл ще зареди потребителя)
                    ISheet sheet = wb.GetSheetAt(0);

                    for (int i = sheet.FirstRowNum + 1; i <= sheet.LastRowNum; i++) //Read Excel File
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue;
                        if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

                        try
                        {
                            var currentTitle = new AddNewJobTitlesServiceModel();
                            var cellCount = row.LastCellNum;
                            if (cellCount != 3)
                            {
                                TempData[ErrorMessageKey] = $"Възникна проблем на ред {i + 1} при четене от файла. Колоните не са 3.";
                                break;
                            }
                            currentTitle.JobTitleId = row.GetCell(0).ToString();
                            currentTitle.Name = row.GetCell(1).ToString();
                            currentTitle.isDeleted = false;

                            items.Add(currentTitle);
                        }
                        catch (Exception)
                        {
                            TempData[ErrorMessageKey] = $"Възникна проблем на ред {i + 1} при четене от файла";
                            break;
                        }

                    }
                    var result = await this.titles.AddTitlesCollection(items);
                    if (result != "success")
                    {
                        TempData[ErrorMessageKey] = result;
                    }
                }
            }
            catch (Exception)
            {
                TempData[ErrorMessageKey] = "Основна грешка!";
            }
            if (TempData[ErrorMessageKey] == null)
            {
                TempData[SuccessMessageKey] = "Длъжностите са добавени успешно!";
            }
            return RedirectToAction("SelectFileWithData");
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
