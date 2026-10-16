namespace PROG_API_POE_.Models
{
    public class CategoryModel
    {

        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public int EventID { get; set; }
        public EventModel? Event { get; set; }

    }
}
