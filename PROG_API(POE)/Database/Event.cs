

using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using PROG_API_POE_.Models;

namespace PROG_API_POE_.Database
{
    public class Event
    {
        private readonly string connectionString =
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Race_DB;Integrated Security=True;";

        // GET: api/events
        public List<EventModel> GetAllEvents()
        {
            var events = new List<EventModel>();

            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(@"
                SELECT EventID, Title, Description, EventDate,
                       Location, Distance, EventType
                FROM dbo.Event
                ORDER BY EventID;", connection);

            connection.Open();

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                events.Add(ReadEvent(reader));
            }

            return events;
        }

        // GET: api/events/{id}
        public EventDetailsModel? GetEventById(int id)
        {
            EventModel? item = null;

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(@"
                SELECT EventID, Title, Description, EventDate,
                       Location, Distance, EventType
                FROM dbo.Event
                WHERE EventID = @ID;", connection))
            {
                command.Parameters.Add("@ID", SqlDbType.Int).Value = id;

                connection.Open();

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    item = ReadEvent(reader);
                }
            }

            if (item == null)
            {
                return null;
            }

            return new EventDetailsModel
            {
                EventID = item.EventID,
                Title = item.Title,
                Description = item.Description,
                Date = item.Date,
                Location = item.Location,
                Distance = item.Distance,
                EventType = item.EventType,
                Routes = GetRoutes(id),
                Categories = GetCategories(id)
            };
        }

        // Converts a database row into an EventModel.
        private EventModel ReadEvent(SqlDataReader reader)
        {
            return new EventModel
            {
                EventID = reader.GetInt32(0),
                Title = reader.IsDBNull(1) ? "" : reader.GetString(1),
                Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                Date = reader.IsDBNull(3)
                    ? DateTime.MinValue
                    : reader.GetDateTime(3),
                Location = reader.IsDBNull(4) ? "" : reader.GetString(4),
                Distance = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                EventType = reader.IsDBNull(6)
                    ? EventType.Run
                    : ParseEventType(reader.GetString(6))
            };
        }

        private EventType ParseEventType(string value)
        {
            return Enum.TryParse<EventType>(
                    value, true, out var result)
                && Enum.IsDefined(typeof(EventType), result)
                    ? result
                    : EventType.Run;
        }

        // Gets all routes belonging to an event.
        private List<RouteModel> GetRoutes(int eventId)
        {
            var routes = new List<RouteModel>();

            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(@"
                SELECT RouteID, RouteName, Description, Distance
                FROM dbo.Route
                WHERE EventID = @EventID
                ORDER BY RouteID;", connection);

            command.Parameters.Add("@EventID", SqlDbType.Int).Value = eventId;

            connection.Open();

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                routes.Add(new RouteModel
                {
                    RouteID = reader.GetInt32(0),
                    RouteName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Distance = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3)
                });
            }

            return routes;
        }

        // Gets all categories belonging to an event.
        private List<CategoryModel> GetCategories(int eventId)
        {
            var categories = new List<CategoryModel>();

            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(@"
                SELECT CategoryID, CategoryName
                FROM dbo.Category
                WHERE EventID = @EventID
                ORDER BY CategoryID;", connection);

            command.Parameters.Add("@EventID", SqlDbType.Int).Value = eventId;

            connection.Open();

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                categories.Add(new CategoryModel
                {
                    CategoryID = reader.GetInt32(0),
                    CategoryName = reader.IsDBNull(1)
                        ? ""
                        : reader.GetString(1)
                });
            }

            return categories;
        }

        // Authenticates an organizer and returns their OrganizerID.
        // Users.PasswordHash must contain a BCrypt password hash.
        public int GetOrganizerId(string email, string password)
        {
            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(@"
                SELECT o.OrganizerID, u.PasswordHash
                FROM dbo.Users AS u
                INNER JOIN dbo.EventOrganizer AS o
                    ON o.UserID = u.UserID
                WHERE u.Email = @Email
                  AND u.Role = 'Organizer';", connection);

            command.Parameters.Add("@Email", SqlDbType.VarChar, 255)
                .Value = email.Trim();

            connection.Open();

            int organizerId;
            string passwordHash;

            using (var reader = command.ExecuteReader())
            {
                if (!reader.Read())
                {
                    return 0;
                }

                organizerId = reader.GetInt32(0);
                passwordHash = reader.GetString(1);
            }

            if (!BCrypt.Net.BCrypt.Verify(password, passwordHash))
            {
                return 0;
            }

            return organizerId;
        }

        // POST: api/events
        public int CreateEvent(
            CreateEventRequest request, int organizerId)
        {
            ValidateEvent(request);

            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(@"
                INSERT INTO dbo.Event
                    (EventName, Title, Description, EventDate,
                     Location, Distance, EventType, OrganizerID)
                OUTPUT INSERTED.EventID
                VALUES
                    (@Title, @Title, @Description, @Date,
                     @Location, @Distance, @EventType, @OrganizerID);",
                connection);

            AddEventParameters(command, request);

            command.Parameters.Add("@OrganizerID", SqlDbType.Int)
                .Value = organizerId;

            connection.Open();

            return Convert.ToInt32(command.ExecuteScalar());
        }

        // PUT: api/events/{id}
        public bool UpdateEvent(
            int id, UpdateEventRequest request, int organizerId)
        {
            ValidateEvent(request);

            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(@"
                UPDATE dbo.Event
                SET EventName = @Title,
                    Title = @Title,
                    Description = @Description,
                    EventDate = @Date,
                    Location = @Location,
                    Distance = @Distance,
                    EventType = @EventType
                WHERE EventID = @ID
                  AND OrganizerID = @OrganizerID;", connection);

            AddEventParameters(command, request);

            command.Parameters.Add("@ID", SqlDbType.Int).Value = id;

            command.Parameters.Add("@OrganizerID", SqlDbType.Int)
                .Value = organizerId;

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        // DELETE: api/events/{id}
        public bool DeleteEvent(int id, int organizerId)
        {
            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(@"
                DELETE FROM dbo.Event
                WHERE EventID = @ID
                  AND OrganizerID = @OrganizerID;", connection);

            command.Parameters.Add("@ID", SqlDbType.Int).Value = id;

            command.Parameters.Add("@OrganizerID", SqlDbType.Int)
                .Value = organizerId;

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        // Validation for creating an event.
        private void ValidateEvent(CreateEventRequest request)
        {
            ValidateEventDetails(
                request.Title,
                request.Description,
                request.Date,
                request.Location,
                request.Distance,
                request.EventType);
        }

        // Validation for updating an event.
        private void ValidateEvent(UpdateEventRequest request)
        {
            ValidateEventDetails(
                request.Title,
                request.Description,
                request.Date,
                request.Location,
                request.Distance,
                request.EventType);
        }

        // Shared validation avoids duplicating the rules.
        private void ValidateEventDetails(
            string title,
            string description,
            DateTime date,
            string location,
            decimal distance,
            EventType eventType)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Title is required.");
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Description is required.");
            }

            if (date == DateTime.MinValue)
            {
                throw new ArgumentException("A valid event date is required.");
            }

            if (string.IsNullOrWhiteSpace(location))
            {
                throw new ArgumentException("Location is required.");
            }

            if (distance <= 0)
            {
                throw new ArgumentException(
                    "Distance must be greater than zero.");
            }

            if (!Enum.IsDefined(typeof(EventType), eventType))
            {
                throw new ArgumentException(
                    "Event type must be Run, Walk, or Cycle.");
            }
        }

        // Parameters for CreateEventRequest.
        private void AddEventParameters(
            SqlCommand command, CreateEventRequest request)
        {
            AddEventParameters(
                command,
                request.Title,
                request.Description,
                request.Date,
                request.Location,
                request.Distance,
                request.EventType);
        }

        // Parameters for UpdateEventRequest.
        private void AddEventParameters(
            SqlCommand command, UpdateEventRequest request)
        {
            AddEventParameters(
                command,
                request.Title,
                request.Description,
                request.Date,
                request.Location,
                request.Distance,
                request.EventType);
        }

        // Shared SQL parameter setup for both requests.
        private void AddEventParameters(
            SqlCommand command,
            string title,
            string description,
            DateTime date,
            string location,
            decimal distance,
            EventType eventType)
        {
            command.Parameters.Add("@Title", SqlDbType.VarChar, 255)
                .Value = title.Trim();

            command.Parameters.Add("@Description", SqlDbType.VarChar, 1000)
                .Value = description.Trim();

            command.Parameters.Add("@Date", SqlDbType.DateTime2)
                .Value = date;

            command.Parameters.Add("@Location", SqlDbType.VarChar, 255)
                .Value = location.Trim();

            var distanceParameter =
                command.Parameters.Add("@Distance", SqlDbType.Decimal);

            distanceParameter.Precision = 10;
            distanceParameter.Scale = 2;
            distanceParameter.Value = distance;

            command.Parameters.Add("@EventType", SqlDbType.VarChar, 10)
                .Value = eventType.ToString();
        }
    }
}
