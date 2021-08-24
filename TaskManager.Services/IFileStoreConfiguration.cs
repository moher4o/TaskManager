using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Services
{
    public interface IFileStoreConfiguration
    {
        bool StoreFiles { get; set; }

        long TaskDirectorySize { get; set; }

        string Location { get; set; }

    }
}
