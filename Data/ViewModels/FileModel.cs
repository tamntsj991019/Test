using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class FileModel
    {
        public Guid Id { get; set; }
        public string FileType { get; set; }
        public byte[] Data { get; set; }
        public string FileName { get; set; }
    }

    public class FileUploadModel
    {
        public IFormFile File { get; set; }
    }

    public class FileUploadListModel
    {
        public List<IFormFile> Files { get; set; }
    }
}
