﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Services.Models
{
    public class AddNewJobTitlesServiceModel
    {
        public  int Id { get; set; }

        public string Name { get; set; }

        public bool isDeleted { get; set; } = false;

    }
}
