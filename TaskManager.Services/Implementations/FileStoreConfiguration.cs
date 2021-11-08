using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Services.Implementations
{
    public class FileStoreConfiguration : IFileStoreConfiguration
    {
        public bool StoreFiles { get; set; } = false;

        public long TaskDirectorySize { get; set; }

        public string Location { get; set; }

        public string MessageFileName { get; set; }
    }
}
