using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManager.WebApi.Models
{
    public class AuthTaskUpdate
    {
        public int taskId { get; set; }
        public int hoursSpend { get; set; }
        public DateTime workDate { get; set; }
    }


}
