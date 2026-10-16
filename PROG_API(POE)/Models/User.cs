using System.Text.Json.Serialization;
using static PROG_API_POE_.Database.Authentication;

namespace PROG_API_POE_.Models
{
    public class User
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum UserRole
        {
            Participant,
            Organizer
        }
        public int UserID { get; set; }

        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public UserRole Role { get; set; }

    }
}
