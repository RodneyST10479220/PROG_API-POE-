namespace PROG_API_POE_.Models
{
    public class Results
    {
        public int ResultID { get; set; }
        public int EventEnrollmentID { get; set;  }

        public EventEnrollment? EventEnrollment { get; set; }

    }
}
