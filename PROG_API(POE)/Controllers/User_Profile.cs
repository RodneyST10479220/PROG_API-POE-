
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PROG_API_POE_.Database;
using PROG_API_POE_.Models;

namespace PROG_API_POE_.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class User_Profile : ControllerBase
    {
        private readonly user_profile profile = new user_profile();

        // GET: api/users/profile?email=...&password=...
        [HttpGet("profile")]
        public IActionResult GetProfile(
            [FromQuery] string email,
            [FromQuery] string password)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                return BadRequest("Email and password are required.");
            }

            try
            {
                var user = profile.GetProfile(email, password);

                if (user == null)
                    return Unauthorized("Invalid email or password.");

                return Ok(user);
            }
            catch (SqlException)
            {
                return StatusCode(500, "A database error occurred.");
            }
        }

        // PUT: api/users/profile
        [HttpPut("profile")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult UpdateProfile(
            [FromForm] UserProfileRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CurrentEmail) ||
                string.IsNullOrWhiteSpace(request.CurrentPassword) ||
                string.IsNullOrWhiteSpace(request.NewName) ||
                string.IsNullOrWhiteSpace(request.NewEmail))
            {
                return BadRequest(
                    "CurrentEmail, CurrentPassword, NewName and NewEmail are required.");
            }

            if (!Enum.IsDefined(typeof(UserRole), request.NewRole))
            {
                return BadRequest("Invalid NewRole.");
            }

            if (request.NewRole == UserRole.Participant &&
                (string.IsNullOrWhiteSpace(request.NewSurname) ||
                 string.IsNullOrWhiteSpace(request.NewCellPhoneNumber)))
            {
                return BadRequest(
                    "NewSurname and NewCellPhoneNumber are required for Participants.");
            }

            try
            {
                bool updated = profile.UpdateProfile(request);

                if (!updated)
                    return Unauthorized(
                        "Current email or password is incorrect.");

                string passwordToUse =
                    string.IsNullOrWhiteSpace(request.NewPassword)
                    ? request.CurrentPassword
                    : request.NewPassword;

                var updatedProfile =
                    profile.GetProfile(request.NewEmail, passwordToUse);

                return Ok(updatedProfile);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (SqlException ex) when (
                ex.Number == 2601 || ex.Number == 2627)
            {
                return Conflict(
                    "That email address is already registered.");
            }
            catch (SqlException)
            {
                return StatusCode(500, "A database error occurred.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
