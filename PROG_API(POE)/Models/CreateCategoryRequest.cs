
using System.ComponentModel.DataAnnotations;

namespace PROG_API_POE_.Models
{
    public class CreateCategoryRequest
    {
        [Required]
        [StringLength(255, MinimumLength = 1)]
        public string CategoryName { get; set; } = "";

        [Range(0, 120)]
        public int MinAge { get; set; }

        [Range(0, 120)]
        public int MaxAge { get; set; }
    }

    public class UpdateCategoryRequest
    {
        [Required]
        [StringLength(255, MinimumLength = 1)]
        public string CategoryName { get; set; } = "";
    }
}
