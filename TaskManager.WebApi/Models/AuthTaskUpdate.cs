using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManager.WebApi.Models
{
    public class AuthTaskUpdate
    {
        public int TaskId { get; set; }
        public int HoursSpend { get; set; }
        public DateTime WorkDate { get; set; } = DateTime.Now.Date;

        public string Token { get; set; }

        public int RType { get; set; }
    }


}
