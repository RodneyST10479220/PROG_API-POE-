namespace PROG_API_POE_.Models
{
    public class Route
    {

        public int RouteID { get; set; }
        public string RouteName { get; set; } = "";
        public int EventID { get; set; }
        public EventModel? Event { get; set; }

    }
}
