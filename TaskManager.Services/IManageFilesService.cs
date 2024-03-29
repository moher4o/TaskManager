﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Services
{
   public interface IManageFilesService
    {
        Task<string> AddFile(int taskId, IFormFile file);

        Task<string> AddFile(int taskId, FileInfo file, string originalFileName);

        bool DeleteTaskFile(int taskId, string fileName);

        bool DeleteTaskDirectory(int taskId);

        List<string> GetFilesInDirectory(int taskId);

        bool DeleteFile(int taskId, string fileName);

        Task<byte[]> ExportFile(int taskId, string fileName);

        Task<string> MessageOnStart();
    }
}
