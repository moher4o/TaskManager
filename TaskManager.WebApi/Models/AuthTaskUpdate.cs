using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManager.WebApi.Models
{
    public class AuthTaskUpdate
    {
        public string UserSecretKey { get; set; }
        public string Token { get; set; }
        public string Message { get; set; }
        public string Note { get; set; }

        public string FileName { get; set; }
        public virtual ICollection<int> Receivers { get; set; } = new List<int>();
        public int RType { get; set; }
        public int TaskId { get; set; }
        public int HoursSpend { get; set; }
        public DateTime WorkDate { get; set; } = DateTime.Now;

    }


}
