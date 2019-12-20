using System;
using System.Collections.Generic;
using System.Text;

namespace UploadAssetToBlob
{
    public class AzureStorageConfig
    {
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
        public string ImageContainer { get; set; }
    }
}
