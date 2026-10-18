1. Project Overview

RaceDay SA is an ASP.NET Core Web API developed using C# and SQL Server. The system is designed to manage running, walking, and cycling events. It allows users to register and log in as participants or event organisers.

The API provides endpoints for managing user profiles and race events. Swagger UI is used to test the API endpoints during development.

2. Technologies Used

C# – Main programming language.

ASP.NET Core Web API (.NET 8) – Used to build the API.

Microsoft SQL Server LocalDB – Stores application data.

Microsoft.Data.SqlClient – Connects the application to the database.

BCrypt – Used to hash passwords and verify login credentials.

Swagger / OpenAPI – Provides an interface for testing API endpoints.

3. How the Program Works

User Registration

New users can register by providing their required details, including their name, email address, password, and account role.

The system supports two account roles:

Participant: A user who takes part in race events.

Organizer: A user who creates and manages race events.

Passwords are hashed before being stored in the database.

User Login

Registered users can log in using their email address and password. The system checks the submitted credentials against the stored user information.

For password verification, the system uses BCrypt to compare the submitted password with the stored password hash.

User Profile Management

Users can retrieve and update their profile information through the API.

Profile information may include their name, email address, contact number, and password, depending on the fields supported by the endpoint.

Event Management

Event organisers can manage race events after their organiser credentials have been verified.

The event management functionality includes:

GET: Retrieve all available events.

GET by ID: Retrieve details of a specific event.

POST: Create a new event.

PUT: Update an existing event.

DELETE: Delete an event, provided that the operation is permitted and the event has no database constraints preventing deletion.

The API checks the organiser's credentials and uses the organiser's database ID to help ensure that organisers can only update or delete their own events.

Events may contain information such as the event title, description, date, location, distance, and event type, depending on the database schema.

4. Database Structure

The application uses a SQL Server database named Race_DB.

The main database tables include:

Users: Stores user account details, email addresses, password hashes, and roles.

Participant: Stores participant-specific information and links participants to their user accounts.

EventOrganizer: Stores organiser information and links organisers to their user accounts.

Event: Stores race event information and identifies the organiser responsible for each event.

Category: Stores categories associated with race events.

Route: Stores route information associated with race events.

The database uses primary keys and foreign keys to identify records and maintain relationships between related tables.

5. API Testing with Swagger

Swagger UI allows developers to test the API without first creating a separate frontend application.

To run the project:

Open the solution in Visual Studio.

Ensure that the required .NET 8 SDK and SQL Server LocalDB are installed.

Confirm that the database Race_DB exists and that the connection string matches your local database setup.

Run the project using Visual Studio.

Open the Swagger UI page displayed by the application.

Select an endpoint, click Try it out, enter the required information, and click Execute.

Review the HTTP response to determine whether the request succeeded.

For endpoints that accept form data, enter the required values in the Swagger form fields.

6. Database Connection

The application connects to SQL Server LocalDB using a connection string similar to:

Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Race_DB;Integrated Security=True;

Ensure that your database and table names match those used in the application code.

7. HTTP Response Codes

The API uses HTTP status codes to communicate the result of requests.

Status code

Meaning

200 OK

The request was successful.

201 Created

A new event was created successfully.

400 Bad Request

The request contains invalid information.

401 Unauthorized

Valid credentials are required.

404 Not Found

The requested record was not found or the operation was not permitted for that record.

500 Internal Server Error

An unexpected server-side error occurred.

8. Security

The application uses BCrypt password hashing to avoid storing passwords as plain text. Organiser credentials are checked before protected event management operations are performed.

Database commands should use SQL parameters to reduce the risk of SQL injection. In a production environment, credentials should be stored securely, and detailed exception messages should not be exposed to API clients.

9. Future Improvements

Possible improvements include:

Building a frontend for participants and organisers.

Adding participant event registration and enrolment functionality.

Implementing race result management.

Expanding event category and route management.

Introducing token-based authentication and role-based authorisation.

Adding automated tests and improved API documentation.

10. Conclusion

RaceDay SA demonstrates the development of a RESTful API using C#, ASP.NET Core Web API, and SQL Server. It provides a foundation for user account management and race event administration while allowing developers to test the functionality through Swagger UI.

## CI/CD Workflow Screenshot

![CI/CD Workflow](screenshots/ProgWorkflow.png)
