using System.Collections.Generic;
using System.Web.Http;
using System;
using WebApplication1.DAL;
using WebApplication1.Models;

[RoutePrefix("api/reservations")]  // Add this attribute
public class ReservationController : ApiController
{
    private ReservationDAL reservationDAL = new ReservationDAL();

    [HttpGet]
    [Route("")]  // This will respond to GET api/reservations
    public IHttpActionResult GetReservations()
    {
        try
        {
            List<Reservation> reservation = reservationDAL.GetReservations();
            return Ok(reservation);
        }
        catch (Exception ex)
        {
            return InternalServerError(ex);
        }
    }
}