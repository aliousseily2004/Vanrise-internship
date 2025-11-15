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
    [RoutePrefix("api/Devices")]
    public class DevicesController : ApiController
    {
        private DevicesDAL DeviceDAL = new DevicesDAL();

        // GET: api/Devices
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAllDevices()
        {
            try
            {
                List<Devices> devices = DeviceDAL.GetAllDevices();
                return Ok(devices);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/Devices
        [HttpPost]
        [Route("")]
        public IHttpActionResult AddDevice([FromBody] Devices device)
        {
            if (device == null)
            {
                return BadRequest("Device data is required");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                DeviceDAL.AddDevice(device);
                return Created($"api/Devices/{device.Id}", device);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/Devices/{id}
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult UpdateDevice(int id, [FromBody] Devices device)
        {
            if (device == null)
            {
                return BadRequest("Device data is required");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != device.Id)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                DeviceDAL.UpdateDevice(device);
                return Ok(device);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE: api/Devices/{id}
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult DeleteDevice(int id)
        {
            try
            {
                DeviceDAL.DeleteDevice(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/Devices/{id}
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetDevice(int id)
        {
            try
            {
                List<Devices> devices = DeviceDAL.GetAllDevices();
                Devices device = devices.FirstOrDefault(d => d.Id == id);

                if (device == null)
                {
                    return NotFound();
                }

                return Ok(device);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}