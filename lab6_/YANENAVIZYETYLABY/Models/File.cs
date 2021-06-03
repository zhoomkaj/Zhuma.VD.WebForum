using System;
using System.ComponentModel.DataAnnotations;

namespace YANENAVIZYETYLABY.Models
{
    public class File
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        public string Extension { get; set; }
        public long Size { get; set; }

        public Folder Folder { get; set; }
        public Guid FolderId { get; set; }
    }
}