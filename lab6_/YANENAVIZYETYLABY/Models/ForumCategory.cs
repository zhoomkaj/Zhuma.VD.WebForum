using System.ComponentModel.DataAnnotations;

namespace YANENAVIZYETYLABY.Models
{
    public class ForumCategory
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
    }
}