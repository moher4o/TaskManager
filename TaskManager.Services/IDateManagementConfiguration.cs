using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Services
{
    public interface IDateManagementConfiguration
    {
        bool CheckRegistrationDate { get; }
        int ReportDate { get; }
 
    }
}
