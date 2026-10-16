using Microsoft.Data.SqlClient;
using PROG_API_POE_.Models;

namespace PROG_API_POE_.Database
{//start of namespace
    public class Authentication
    {//start of class

        // Connection string
        string connection_string = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Race_DB;Integrated Security=True;";


        // REGISTER

        public string Register( string name,string email, string password, UserRole role, string surname = "", string cellPhoneNumber = "")
        {
            if (!Enum.IsDefined(typeof(UserRole), role))
                return "Invalid role selected";

            if (role == UserRole.Participant &&
                (string.IsNullOrWhiteSpace(surname) ||
                 string.IsNullOrWhiteSpace(cellPhoneNumber)))
            {
                return "Surname and cellphone number are required for participants.";
            }

            string passwordHash =
                BCrypt.Net.BCrypt.HashPassword(password);

            using (SqlConnection connect =
                   new SqlConnection(connection_string))
            {
                connect.Open();

                using (SqlTransaction transaction =
                       connect.BeginTransaction(
                           System.Data.IsolationLevel.Serializable))
                {
                    try
                    {
                        int userID;

                        string userQuery = @"
                    INSERT INTO dbo.Users
                        (Name, Email, PasswordHash, Role)
                    OUTPUT INSERTED.UserID
                    VALUES
                        (@Name, @Email, @PasswordHash, @Role);";

                        using (SqlCommand command =
                               new SqlCommand(
                                   userQuery, connect, transaction))
                        {
                            command.Parameters.AddWithValue("@Name", name);
                            command.Parameters.AddWithValue("@Email", email);
                            command.Parameters.AddWithValue(
                                "@PasswordHash", passwordHash);
                            command.Parameters.AddWithValue(
                                "@Role", role.ToString());

                            userID = Convert.ToInt32(
                                command.ExecuteScalar());
                        }

                        if (role == UserRole.Participant)
                        {
                            string participantQuery = @"
                        DECLARE @NewID INT;

                        SELECT @NewID =
                            ISNULL(MAX(ParticipantID), 0) + 1
                        FROM dbo.Participant WITH (TABLOCKX, HOLDLOCK);

                        INSERT INTO dbo.Participant
                            (ParticipantID, UserID, Name, Surname,
                             CellPhoneNumber)
                        VALUES
                            (@NewID, @UserID, @Name, @Surname,
                             @CellPhoneNumber);";

                            using (SqlCommand command =
                                   new SqlCommand(
                                       participantQuery, connect, transaction))
                            {
                                command.Parameters.AddWithValue("@UserID", userID);
                                command.Parameters.AddWithValue("@Name", name);
                                command.Parameters.AddWithValue("@Surname", surname);
                                command.Parameters.AddWithValue(
                                    "@CellPhoneNumber", cellPhoneNumber);

                                command.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            string organizerQuery = @"
                        DECLARE @NewID INT;

                        SELECT @NewID =
                            ISNULL(MAX(OrganizerID), 0) + 1
                        FROM dbo.EventOrganizer WITH (TABLOCKX, HOLDLOCK);

                        INSERT INTO dbo.EventOrganizer
                            (OrganizerID, UserID, OrganizerName)
                        VALUES
                            (@NewID, @UserID, @Name);";

                            using (SqlCommand command =
                                   new SqlCommand(
                                       organizerQuery, connect, transaction))
                            {
                                command.Parameters.AddWithValue("@UserID", userID);
                                command.Parameters.AddWithValue("@Name", name);

                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return "Registration successful";
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }


        // LOGIN
        public bool Login(
            string email,
            string password,
           PROG_API_POE_.Models.UserRole role)
        {
            if (!Enum.IsDefined(typeof(UserRole), role))
            {
                return false;
            }

            string query = @"
                SELECT PasswordHash, Role
                FROM Users
                WHERE Email = @Email;";

            using (SqlConnection connect =
                   new SqlConnection(connection_string))
            {
                connect.Open();

                using (SqlCommand command =
                       new SqlCommand(query, connect))
                {
                    command.Parameters.AddWithValue("@Email", email);

                    using (SqlDataReader reader =
                           command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return false;
                        }

                        string storedHash =
                            reader["PasswordHash"].ToString() ?? "";

                        string storedRole =
                            reader["Role"].ToString() ?? "";

                        bool passwordCorrect =
                            BCrypt.Net.BCrypt.Verify(
                                password, storedHash);

                        bool roleCorrect =
                            string.Equals(
                                storedRole,
                                role.ToString(),
                                StringComparison.OrdinalIgnoreCase);

                        return passwordCorrect && roleCorrect;
                    }
                }
            }
        }
    }//end of class
}//end of namespace
