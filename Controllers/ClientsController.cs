using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApplication1.DAL;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [RoutePrefix("api/Clients")]
    public class ClientsController : ApiController
    {
        private ClientsDAL clientsDAL = new ClientsDAL();

        // GET: api/Clients
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAllClients()
        {
            try
            {
                List<Clients> clients = clientsDAL.GetAllClients();
                return Ok(clients);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        // POST: api/Clients
        [HttpPost]
        [Route("")]
        public IHttpActionResult AddClient([FromBody] Clients client)
        {
            if (client == null)
            {
                return BadRequest("Client data is required");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdClient = clientsDAL.AddClient(client);
                return Created($"api/Clients/{createdClient.Id}", createdClient);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/Clients/{id}
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult UpdateClient(int id, [FromBody] Clients client)
        {
            if (client == null)
            {
                return BadRequest("Client data is required");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != client.Id)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                bool updated = clientsDAL.UpdateClient(client);
                if (!updated)
                {
                    return NotFound();
                }
                return Ok(client);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE: api/Clients/{id}
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult DeleteClient(int id)
        {
            try
            {
                bool deleted = clientsDAL.DeleteClient(id);
                if (!deleted)
                {
                    return NotFound();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/Clients/{id}
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetClient(int id)
        {
            try
            {
                // Call the updated method that returns ClientDetails
                ClientDetails clientDetails = clientsDAL.GetClientById(id);

                if (clientDetails == null)
                {
                    return NotFound(); // Return 404 if no client is found
                }

                return Ok(clientDetails); // Return the client details
            }
            catch (Exception ex)
            {
                return InternalServerError(ex); // Return 500 for any exceptions
            }
        }

        // POST: api/Clients/Reserve


        // GET: api/Clients/Report
        [HttpGet]
        [Route("Report")]
        public IHttpActionResult GetClientTypeReport(string type = null)
        {
            try
            {
                // Get counts directly from the database
                var reportData = clientsDAL.GetClientCountsByType(type);

                // If no filter, ensure both types are represented even if count is zero
                if (string.IsNullOrEmpty(type))
                {
                    var allTypes = new List<dynamic>
            {
                new { type = "0", count = reportData.FirstOrDefault(r => r.Type == "0")?.Count ?? 0 },
                new { type = "1", count = reportData.FirstOrDefault(r => r.Type == "1")?.Count ?? 0 }
            };
                    return Ok(allTypes);
                }

                // If filtering, return just the filtered type
                return Ok(reportData);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        // DTO for Reservation Request
        [HttpPost]
        [Route("Reserve")]
        public IHttpActionResult ReserveClient([FromBody] ReserveClientRequest request)
        {
            if (request == null)
            {
                return BadRequest("Reservation data is required");
            }

            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                return BadRequest("Phone number is required");
            }

            try
            {
                bool reservationResult = clientsDAL.ReserveClient(request.ClientId, request.PhoneNumber);

                if (reservationResult)
                {
                    return Ok(); // Successfully reserved
                }
                else
                {
                    return BadRequest("Unable to reserve client. The phone number is already in use.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Error reserving client: {ex.Message}");
                return InternalServerError(ex); // Return a 500 Internal Server Error
            }
        }
        [HttpPost]
        [Route("{id:int}/Unreserve")]
        public IHttpActionResult UnreserveClient(int id)
        {
            try
            {
                bool unreservationResult = clientsDAL.UnreserveClient(id);

                if (unreservationResult)
                {
                    return Ok(new { message = "Client unreserved successfully" });
                }
                else
                {
                    return BadRequest("Unable to unreserve client. The client may not be currently reserved.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Unreserve API Error: {ex.Message}");
                return InternalServerError(ex);
            }
        }


    }
}