using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace TaskMenager.Client.Models.InsertDataFromFile
{
    public class SelectedFileToInsertViewModel
    {

        public IList<SelectListItem> FileTypes { get; set; } = new List<SelectListItem>();
    }
}
