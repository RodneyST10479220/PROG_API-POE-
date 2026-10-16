namespace PROG_API_POE_.Models
{
    public class EventEnrollment
    {

        public int EventEnrollmentID { get; set; }

        public int ParticipantID { get; set; }
        public Participant? Participant { get; set; }

        public int CategoryID { get; set; }
        public Category? Category { get; set; }

    }
}
