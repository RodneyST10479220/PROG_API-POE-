
namespace PROG_API_POE_.Models
{
    public class UserProfileRequest
    {
        public string CurrentEmail { get; set; } = "";
        public string CurrentPassword { get; set; } = "";

        public string NewName { get; set; } = "";
        public string NewSurname { get; set; } = "";
        public string NewCellPhoneNumber { get; set; } = "";
        public string NewEmail { get; set; } = "";
        public string NewPassword { get; set; } = "";

        public UserRole NewRole { get; set; } = UserRole.Participant;
    }
}

