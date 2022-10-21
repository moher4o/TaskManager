﻿using System;
using System.Collections.Generic;
using TaskManager.Services.Models.MobMessages;

namespace TaskManager.WebApi.Models
{
    [Serializable]
    public class ResponceApiModel
    {
        public string ApiResponce { get; set; } = "error";

        public List<MessageListModel> UserMessages { get; set; } = new List<MessageListModel>();

        public List<TaskApiModel> Taskove { get; set; } = new List<TaskApiModel>();

        public List<UsersListViewModel> Employees { get; set; } = new List<UsersListViewModel>();
    }
}

