
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace PROG_API_POE_.Database
{
    public class EventCredentials_Database
    {
        private readonly string connectionString =
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Race_DB;Integrated Security=True;";

        public (int UserID, string Role, int? OrganizerID)? Authenticate(
            string email, string password)
        {
            using SqlConnection connection =
                new SqlConnection(connectionString);

            string query = @"
                SELECT UserID, Role, PasswordHash
                FROM dbo.Users
                WHERE Email = @Email";

            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.Add("@Email", SqlDbType.NVarChar, 256)
                .Value = email.Trim();

            connection.Open();

            int userId;
            string role;
            string passwordHash;

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (!reader.Read())
                    return null;

                userId = reader.GetInt32(0);
                role = reader.GetString(1);
                passwordHash = reader.GetString(2);
            }

            if (!BCrypt.Net.BCrypt.Verify(password, passwordHash))
                return null;

            if (role.Equals("Participant",
                StringComparison.OrdinalIgnoreCase))
                return (userId, "Participant", null);

            if (role.Equals("Organizer",
                StringComparison.OrdinalIgnoreCase))
            {
                using SqlCommand organizerCommand = new SqlCommand(@"
                    SELECT OrganizerID
                    FROM dbo.EventOrganizer
                    WHERE UserID = @UserID", connection);

                organizerCommand.Parameters.Add("@UserID", SqlDbType.Int)
                    .Value = userId;

                object? result = organizerCommand.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                    return null;

                return (userId, "Organizer", Convert.ToInt32(result));
            }

            return null;
        }
    }
}
