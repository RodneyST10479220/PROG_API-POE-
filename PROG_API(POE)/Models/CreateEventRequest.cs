using System;
using System.ComponentModel.DataAnnotations;

namespace PROG_API_POE_.Models
{
    public class CreateEventRequest
    {
        [Required]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Location { get; set; } = "";

        [Range(0.01, double.MaxValue)]
        public decimal Distance { get; set; }

        public EventType EventType { get; set; }
    }

    public class UpdateEventRequest
    {
        [Required]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Location { get; set; } = "";

        [Range(0.01, double.MaxValue)]
        public decimal Distance { get; set; }

        public EventType EventType { get; set; }
    }
}