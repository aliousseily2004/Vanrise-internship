using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WebApplication1.DAL;
using WebApplication1.Models;
using static WebApplication1.DAL.PhonesDAL;

namespace WebApplication1.Controllers
{
    [RoutePrefix("api/phones")]
    public class PhonesController : ApiController
    {
        private PhonesDAL phonesDAL = new PhonesDAL();

        // GET: api/phones
        // In PhonesController.cs
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAllPhones()
        {
            try
            {
                List<Phones> phones = phonesDAL.GetAllPhoneNumbers();
                return Ok(phones); // Return full phone objects
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpGet]
        [Route("numbers")]
        public IHttpActionResult GetAllPhoneNumbers()
        {
            try
            {
                List<Phones> phones = phonesDAL.GetAllPhoneNumbers();
                var phoneNumbers = phones.Select(p => p.Number).ToList();
                return Ok(phoneNumbers); // Return only numbers
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        [HttpGet]
        [Route("availablenumbers")]
        public IHttpActionResult GetAllAvailablePhoneNumbers()
        {
            try
            {
                List<Phones> phones = phonesDAL.GetAllAvailablePhoneNumbers();
                var phoneNumbers = phones.Select(p => p.Number).ToList();
                return Ok(phoneNumbers); // Return only numbers
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("PhoneInfo")]
        public IHttpActionResult GetPhoneInfo()
        {
            try
            {
                List<PhoneReportItem> reportData = phonesDAL.GetPhoneInfo();
                return Ok(reportData);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }




        // POST: api/phones
        [HttpPost]
        [Route("")]
        public IHttpActionResult AddPhone([FromBody] Phones phone)
        {
            if (phone == null)
            {
                return BadRequest("Phone data is required");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // You'll need to add a method in PhoneDAL to add a phone
                int newPhoneId = phonesDAL.AddPhone(phone);

                // Return the created phone with its new ID
                return Created($"api/phones/{newPhoneId}", phone);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/phones/{id}
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult UpdatePhone(int id, [FromBody] Phones phone)
        {
            if (phone == null)
            {
                return BadRequest("Phone data is required");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != phone.Id)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                // You'll need to add a method in PhoneDAL to update a phone
                bool updated = phonesDAL.UpdatePhone(phone);

                if (!updated)
                {
                    return NotFound();
                }

                return Ok(phone);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE: api/phones/{id}
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult DeletePhone(int id)
        {
            try
            {
                // You'll need to add a method in PhoneDAL to delete a phone
                bool deleted = phonesDAL.DeletePhone(id);

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
        // GET: api/phones/{id}
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetPhone(int id)
        {
            try
            {
                Phones phone = phonesDAL.GetPhoneById(id);

                if (phone == null)
                {
                    return NotFound();
                }

                return Ok(phone);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


    }
}