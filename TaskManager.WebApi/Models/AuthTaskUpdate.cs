using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManager.WebApi.Models
{
    public class AuthTaskUpdate
    {
        public string username { get; set; }
        public string Hmac { get; set; }
        public string Data { get; set; }
    }


}
