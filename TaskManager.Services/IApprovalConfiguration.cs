using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Services
{
    public interface IApprovalConfiguration
    {
        bool ReportApproval { get; }
    }
}
