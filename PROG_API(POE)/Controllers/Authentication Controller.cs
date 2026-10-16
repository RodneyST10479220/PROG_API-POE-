
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PROG_API_POE_.Database;
using PROG_API_POE_.Models;

namespace PROG_API_POE_.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Authentication_Controller : ControllerBase
    {
        private readonly Authentication authentication =
            new Authentication();

        [HttpPost("register")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult RegisterUser(
            [FromForm] RegisterRequest request)
        {
            try
            {
                string result = authentication.Register(
                                   request.Name,
                                   request.Email,
                                   request.Password,
                                   request.Role,
                                   request.Surname,
                                   request.CellPhoneNumber);

                if (result == "Invalid role selected")
                    return BadRequest(result);

                return Ok(result);
            }
            catch (SqlException ex) when (
                ex.Number == 2601 || ex.Number == 2627)
            {
                return Conflict("This email is already registered.");
            }

            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }

        }

        [HttpPost("login")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult LoginUser(
            [FromForm] LoginRequest request)
        {
            try
            {
                bool result = authentication.Login(
                    request.Email,
                    request.Password,
                    request.Role);

                if (!result)
                {
                    return Unauthorized(
                        "Invalid email, password or role.");
                }

                return Ok("Login successful.");
            }
            catch (SqlException)
            {
                return StatusCode(
                    500, "A database error occurred. Check your database schema.");
            }
        }
    }
}
