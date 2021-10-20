using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Services.Implementations
{
    public class DateManagementConfiguration : IDateManagementConfiguration
    {
       public bool CheckRegistrationDate { get; set; }

       public  int ReportDate { get; set; }

    }
}
