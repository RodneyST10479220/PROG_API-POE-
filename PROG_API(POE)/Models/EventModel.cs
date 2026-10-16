using System;
using System.Collections.Generic;

namespace PROG_API_POE_.Models
{
    // Only these three event types are allowed
    public enum EventType
    {
        Run,
        Walk,
        Cycle
    }

    public class EventModel
    {
        public int EventID { get; set; }

        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        public DateTime Date { get; set; }

        public string Location { get; set; } = "";

        public decimal Distance { get; set; }

        public EventType EventType { get; set; }
    }

    // Used when viewing one event with its routes and categories
    public class EventDetailsModel : EventModel
    {
        public List<RouteModel> Routes { get; set; } = new();

        public List<Category> Categories { get; set; } = new();
    }

    public class RouteModel
    {
        public int RouteID { get; set; }

        public string RouteName { get; set; } = "";

        public string Description { get; set; } = "";

        public decimal Distance { get; set; }
    }

    public class Category
    {
        public int CategoryID { get; set; }

        public string CategoryName { get; set; } = "";
    }
}