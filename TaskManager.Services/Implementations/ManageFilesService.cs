﻿using Microsoft.AspNetCore.Http;
using TaskManager.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace TaskManager.Services.Implementations
{
    public class ManageFilesService : IManageFilesService
    {
        private readonly IEmailConfiguration fileConfiguration;

        public ManageFilesService(IEmailConfiguration emailConfiguration)
        {
            fileConfiguration = emailConfiguration;
        }


        public bool DeleteTaskFile(int taskId, string fileName)
        {
            string directoryName = taskId.ToString();
            string globalPath = Environment.CurrentDirectory;

            var path = Path.Combine(globalPath, DataConstants.ClassFilesSubDirectory, directoryName,fileName);

            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                file.Delete();
                return true;
            }
            return false;
        }

        public bool DeleteTaskDirectory(int taskId)
        {
            string directoryName = taskId.ToString();
            string globalPath = Environment.CurrentDirectory;

            var path = Path.Combine(globalPath, DataConstants.ClassFilesSubDirectory,directoryName);

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            return true;
        }

        public async Task<string> AddFile(int taskId, IFormFile file)
        {
            if (fileConfiguration.StoreFiles)
            {
                string directoryName = taskId.ToString();
                string globalPath = Environment.CurrentDirectory;

                var classDirectory = Path.Combine(
                            globalPath, DataConstants.ClassFilesSubDirectory, directoryName);

                if (!Directory.Exists(classDirectory))
                {
                    Directory.CreateDirectory(classDirectory);
                }

                DirectoryInfo dirInfo = new DirectoryInfo(classDirectory);
                long dirSize = await Task.Run(() => dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length));

                var sumSizeMb = (dirSize + file.Length) / 1048576;  //в мегабайти
                if (sumSizeMb > fileConfiguration.TaskDirectorySize)
                {
                    return $"Файловото пространство({fileConfiguration.TaskDirectorySize}Mb) заделено за задачата, ще бъде превишено. Оставащото свободно пространство е {fileConfiguration.TaskDirectorySize - (dirSize/ 1048576)}Mb";
                }

                var filePath = Path.Combine(classDirectory, file.FileName);

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    try
                    {
                        await file.CopyToAsync(stream);
                    }
                    catch (Exception ex)
                    {

                        return ex.Message;
                    }

                }
                return "success";
            }
            else
            {
                return "Услугата не е разрешена";
            }
        }

        public IList<string> GetFilesInDirectory(int taskId)
        {
            var fileList = new List<string>();
            try
            {
                string directoryName = taskId.ToString();
                string globalPath = Environment.CurrentDirectory;

                var classDirectory = Path.Combine(
                            globalPath, DataConstants.ClassFilesSubDirectory, directoryName);

                if (!Directory.Exists(classDirectory))
                {
                    return fileList;
                }



                foreach (var file in Directory.GetFiles(classDirectory))
                {
                    FileInfo info = new FileInfo(file);
                    fileList.Add(Path.GetFileName(info.FullName));
                }

            }
            catch (Exception)
            {
                return fileList;
            }

            return fileList;
        }

        public bool DeleteFile(int taskId, string fileName)
        {
            try
            {
                string directoryName = taskId.ToString();
                string globalPath = Environment.CurrentDirectory;

                var classDirectory = Path.Combine(
                            globalPath, DataConstants.ClassFilesSubDirectory, directoryName);

                if (!Directory.Exists(classDirectory))
                {
                    return false;
                }

                var filePath = Path.Combine(classDirectory, fileName);

                FileInfo file = new FileInfo(filePath);
                file.Delete();
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<byte[]> ExportFile(int taskId, string fileName)
        {
            try
            {
                string directoryName = taskId.ToString();
                string globalPath = Environment.CurrentDirectory;

                var classDirectory = Path.Combine(
                            globalPath, DataConstants.ClassFilesSubDirectory, directoryName);

                if (!Directory.Exists(classDirectory))
                {
                    return null;
                }

                var filePath = Path.Combine(classDirectory, fileName);

                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] imagen = new byte[stream.Length];
                    await stream.ReadAsync(imagen, 0, (int)stream.Length);
                    return imagen;
                }

            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

