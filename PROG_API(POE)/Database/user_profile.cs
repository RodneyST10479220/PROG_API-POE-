
using Microsoft.Data.SqlClient;
using PROG_API_POE_.Models;

namespace PROG_API_POE_.Database
{
    public class user_profile
    {
        private readonly string connectionString =
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Race_DB;Integrated Security=True;";

        public object? GetProfile(string email, string password)
        {
            using SqlConnection connect = new SqlConnection(connectionString);
            connect.Open();

            string query = @"
                SELECT UserID, Name, Email, PasswordHash, Role
                FROM dbo.Users
                WHERE Email = @Email";

            int userID;
            string userName;
            string userEmail;
            string role;
            string passwordHash;

            using (SqlCommand command = new SqlCommand(query, connect))
            {
                command.Parameters.AddWithValue("@Email", email);

                using SqlDataReader reader = command.ExecuteReader();

                if (!reader.Read())
                    return null;

                passwordHash = reader["PasswordHash"].ToString()!;

                if (!BCrypt.Net.BCrypt.Verify(password, passwordHash))
                    return null;

                userID = (int)reader["UserID"];
                userName = reader["Name"].ToString()!;
                userEmail = reader["Email"].ToString()!;
                role = reader["Role"].ToString()!;
            }

            if (Enum.TryParse<UserRole>(
                role, true, out UserRole userRole))
            {
                if (userRole == UserRole.Participant)
                {
                    string participantQuery = @"
                        SELECT Name, Surname, CellPhoneNumber
                        FROM dbo.Participant
                        WHERE UserID = @UserID";

                    using SqlCommand command =
                        new SqlCommand(participantQuery, connect);

                    command.Parameters.AddWithValue("@UserID", userID);

                    using SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        return new
                        {
                            UserID = userID,
                            Name = reader["Name"].ToString(),
                            Surname = reader["Surname"].ToString(),
                            CellPhoneNumber =
                                reader["CellPhoneNumber"].ToString(),
                            Email = userEmail,
                            Role = userRole
                        };
                    }
                }
                else if (userRole == UserRole.Organizer)
                {
                    string organizerQuery = @"
                        SELECT OrganizerName
                        FROM dbo.EventOrganizer
                        WHERE UserID = @UserID";

                    using SqlCommand command =
                        new SqlCommand(organizerQuery, connect);

                    command.Parameters.AddWithValue("@UserID", userID);

                    using SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        return new
                        {
                            UserID = userID,
                            Name = reader["OrganizerName"].ToString(),
                            Email = userEmail,
                            Role = userRole
                        };
                    }
                }
            }

            return new
            {
                UserID = userID,
                Name = userName,
                Email = userEmail,
                Role = role
            };
        }

        public bool UpdateProfile(UserProfileRequest request)
        {
            using SqlConnection connect =
                new SqlConnection(connectionString);

            connect.Open();

            int userID;
            string storedPasswordHash;
            string currentRoleText;

            string findUser = @"
                SELECT UserID, PasswordHash, Role
                FROM dbo.Users
                WHERE Email = @CurrentEmail";

            using (SqlCommand command = new SqlCommand(findUser, connect))
            {
                command.Parameters.AddWithValue(
                    "@CurrentEmail", request.CurrentEmail);

                using SqlDataReader reader = command.ExecuteReader();

                if (!reader.Read())
                    return false;

                userID = (int)reader["UserID"];
                storedPasswordHash =
                    reader["PasswordHash"].ToString()!;
                currentRoleText = reader["Role"].ToString()!;
            }

            if (!BCrypt.Net.BCrypt.Verify(
                request.CurrentPassword, storedPasswordHash))
                return false;

            if (!Enum.TryParse<UserRole>(
                    currentRoleText, true, out UserRole currentRole))
            {
                throw new InvalidOperationException(
                    "The current database role is invalid.");
            }

            if (!Enum.IsDefined(typeof(UserRole), request.NewRole))
            {
                throw new ArgumentException("Invalid new role.");
            }

            if (string.IsNullOrWhiteSpace(request.NewName) ||
                string.IsNullOrWhiteSpace(request.NewEmail))
            {
                throw new ArgumentException(
                    "NewName and NewEmail are required.");
            }

            if (request.NewRole == UserRole.Participant &&
                (string.IsNullOrWhiteSpace(request.NewSurname) ||
                 string.IsNullOrWhiteSpace(request.NewCellPhoneNumber)))
            {
                throw new ArgumentException(
                    "NewSurname and NewCellPhoneNumber are required for Participants.");
            }

            bool changingRole = currentRole != request.NewRole;

            using SqlTransaction transaction = connect.BeginTransaction();

            try
            {
                // When switching roles, unlink the old profile.
                // The old record is retained to avoid deleting history.
                if (changingRole && currentRole == UserRole.Participant)
                {
                    string detachParticipant = @"
                        UPDATE dbo.Participant
                        SET UserID = NULL
                        WHERE UserID = @UserID";

                    using SqlCommand command =
                        new SqlCommand(detachParticipant, connect, transaction);

                    command.Parameters.AddWithValue("@UserID", userID);
                    command.ExecuteNonQuery();
                }
                else if (changingRole &&
                         currentRole == UserRole.Organizer)
                {
                    string detachOrganizer = @"
                        UPDATE dbo.EventOrganizer
                        SET UserID = NULL
                        WHERE UserID = @UserID";

                    using SqlCommand command =
                        new SqlCommand(detachOrganizer, connect, transaction);

                    command.Parameters.AddWithValue("@UserID", userID);
                    command.ExecuteNonQuery();
                }

                string newPasswordHash =
                    string.IsNullOrWhiteSpace(request.NewPassword)
                    ? storedPasswordHash
                    : BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

                string updateUser = @"
                    UPDATE dbo.Users
                    SET Name = @Name,
                        Email = @Email,
                        PasswordHash = @PasswordHash,
                        Role = @Role
                    WHERE UserID = @UserID";

                using (SqlCommand command =
                    new SqlCommand(updateUser, connect, transaction))
                {
                    command.Parameters.AddWithValue(
                        "@Name", request.NewName);
                    command.Parameters.AddWithValue(
                        "@Email", request.NewEmail);
                    command.Parameters.AddWithValue(
                        "@PasswordHash", newPasswordHash);
                    command.Parameters.AddWithValue(
                        "@Role", request.NewRole.ToString());
                    command.Parameters.AddWithValue(
                        "@UserID", userID);

                    command.ExecuteNonQuery();
                }

                if (request.NewRole == UserRole.Participant)
                {
                    if (changingRole)
                    {
                        string insertParticipant = @"
                            INSERT INTO dbo.Participant
                                (ParticipantID, Name, Surname,
                                 CellPhoneNumber, UserID)
                            SELECT ISNULL(MAX(ParticipantID), 0) + 1,
                                   @Name, @Surname, @Phone, @UserID
                            FROM dbo.Participant";

                        using SqlCommand command =
                            new SqlCommand(insertParticipant, connect, transaction);

                        command.Parameters.AddWithValue(
                            "@Name", request.NewName);
                        command.Parameters.AddWithValue(
                            "@Surname", request.NewSurname);
                        command.Parameters.AddWithValue(
                            "@Phone", request.NewCellPhoneNumber);
                        command.Parameters.AddWithValue(
                            "@UserID", userID);

                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        string updateParticipant = @"
                            UPDATE dbo.Participant
                            SET Name = @Name,
                                Surname = @Surname,
                                CellPhoneNumber = @Phone
                            WHERE UserID = @UserID";

                        using SqlCommand command =
                            new SqlCommand(updateParticipant, connect, transaction);

                        command.Parameters.AddWithValue(
                            "@Name", request.NewName);
                        command.Parameters.AddWithValue(
                            "@Surname", request.NewSurname);
                        command.Parameters.AddWithValue(
                            "@Phone", request.NewCellPhoneNumber);
                        command.Parameters.AddWithValue(
                            "@UserID", userID);

                        if (command.ExecuteNonQuery() == 0)
                            throw new InvalidOperationException(
                                "Participant profile not found.");
                    }
                }
                else if (request.NewRole == UserRole.Organizer)
                {
                    if (changingRole)
                    {
                        string insertOrganizer = @"
                            INSERT INTO dbo.EventOrganizer
                                (OrganizerID, OrganizerName, UserID)
                            SELECT ISNULL(MAX(OrganizerID), 0) + 1,
                                   @Name, @UserID
                            FROM dbo.EventOrganizer";

                        using SqlCommand command =
                            new SqlCommand(insertOrganizer, connect, transaction);

                        command.Parameters.AddWithValue(
                            "@Name", request.NewName);
                        command.Parameters.AddWithValue(
                            "@UserID", userID);

                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        string updateOrganizer = @"
                            UPDATE dbo.EventOrganizer
                            SET OrganizerName = @Name
                            WHERE UserID = @UserID";

                        using SqlCommand command =
                            new SqlCommand(updateOrganizer, connect, transaction);

                        command.Parameters.AddWithValue(
                            "@Name", request.NewName);
                        command.Parameters.AddWithValue(
                            "@UserID", userID);

                        if (command.ExecuteNonQuery() == 0)
                            throw new InvalidOperationException(
                                "Organizer profile not found.");
                    }
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}

