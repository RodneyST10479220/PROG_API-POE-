
using Microsoft.AspNetCore.Mvc;
using PROG_API_POE_.Database;
using PROG_API_POE_.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace PROG_API_POE_.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventController : ControllerBase
    {
        private readonly Event database = new();

        // GET: api/events
        [HttpGet]
        public IActionResult GetAllEvents()
        {
            try
            {
                return Ok(database.GetAllEvents());
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    Message = "Failed to retrieve events."
                });
            }
        }

        // GET: api/events/1
        [HttpGet("{id}")]
        public IActionResult GetEventById(int id)
        {
            try
            {
                var eventDetails = database.GetEventById(id);

                if (eventDetails == null)
                    return NotFound(new { Message = "Event not found." });

                return Ok(eventDetails);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    Message = "Failed to retrieve event details."
                });
            }
        }

        // POST: api/events
        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult CreateEvent(
            [FromForm] CreateEventRequest request,
            [FromForm, Required] string email,
            [FromForm, Required] string password)
        {
            try
            {
                int organizerId = database.GetOrganizerId(email, password);

                if (organizerId == 0)
                    return Unauthorized(new
                    {
                        Message = "Valid organiser credentials are required."
                    });

                int eventId = database.CreateEvent(request, organizerId);

                return StatusCode(201, new
                {
                    Message = "Event created successfully.",
                    EventID = eventId
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    Message = "Failed to create event."
                });
            }
        }

        // PUT: api/events/1
        [HttpPut("{id}")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult UpdateEvent(
            int id,
            [FromForm] UpdateEventRequest request,
            [FromForm, Required] string email,
            [FromForm, Required] string password)
        {
            try
            {
                int organizerId = database.GetOrganizerId(email, password);

                if (organizerId == 0)
                    return Unauthorized(new
                    {
                        Message = "Valid organiser credentials are required."
                    });

                bool updated = database.UpdateEvent(id, request, organizerId);

                if (!updated)
                    return NotFound(new
                    {
                        Message = "Event not found or you do not own this event."
                    });

                return Ok(new { Message = "Event updated successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    Message = "Failed to update event."
                });
            }
        }

        // DELETE: api/events/1
        [HttpDelete("{id}")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult DeleteEvent(
            int id,
            [FromForm, Required] string email,
            [FromForm, Required] string password)
        {
            try
            {
                int organizerId = database.GetOrganizerId(email, password);

                if (organizerId == 0)
                    return Unauthorized(new
                    {
                        Message = "Valid organiser credentials are required."
                    });

                bool deleted = database.DeleteEvent(id, organizerId);

                if (!deleted)
                    return NotFound(new
                    {
                        Message = "Event not found or you do not own this event."
                    });

                return Ok(new { Message = "Event deleted successfully." });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    Message = "Failed to delete event. The event may have related records."
                });
            }
        }
    }
}