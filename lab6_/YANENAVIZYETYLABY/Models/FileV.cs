using System;
using Microsoft.AspNetCore.Http;

namespace YANENAVIZYETYLABY.Models
{
    public class FileV
    {
        public string Name { get; set; }
        public IFormFile FilePath { get; set; }
    }
}
