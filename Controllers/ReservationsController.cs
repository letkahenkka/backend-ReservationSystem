using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.Middleware;
using ReservationSystem.Models;
using ReservationSystem.Services;

namespace ReservationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly ReservationContext _context;
        private readonly IReservationService _service;
        private readonly IUserAuthenticationService _authenticationService;

        public ReservationsController(ReservationContext context, IReservationService service, IUserAuthenticationService authenticationService)
        {
            _context = context;
            _service = service;
            _authenticationService = authenticationService;
        }

        // GET: api/Reservations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationDTO>>> GetReservations() // hakee listan kaikista varauksista
        {
            return Ok(await _service.GetReservationsAsync());
        }

        // GET: api/Reservations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(long id) //hakee varauksen id:n mukaan
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
            {
                return NotFound();
            }
            return reservation;
        }

        // PUT: api/Reservations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutReservation(long id, ReservationDTO reservation) //päivittää varauksen annetun id:n mukaan
        {
            if (id != reservation.Id)
            {
                return BadRequest();
            }

            //Tarkista, onko oikeus muokata
            bool isAllowed = await _authenticationService.IsAllowed(this.User.FindFirst(ClaimTypes.Name).Value, reservation);

            if (!isAllowed)
            {
                return Unauthorized();
            }

            ReservationDTO updatedReservation = await _service.UpdateReservationAsync(reservation);

            return NoContent();
        }

        // POST: api/Reservations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReservationDTO>> PostReservation(ReservationDTO reservation) //lisää uuden varauksen
        {

            bool isAllowed = await _authenticationService.IsAllowed(this.User.FindFirst(ClaimTypes.Name).Value, reservation);
            if (!isAllowed)
            {
                return Unauthorized();
            }
            reservation = await _service.CreateReservationAsync(reservation);
            if(reservation == null)
            {
                return BadRequest();
            }

            return CreatedAtAction("GetReservation", new { id = reservation.Id }, reservation);
        }

        // DELETE: api/Reservations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(long id) //poistaa varauksen id:n mukaan
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReservationExists(long id) //tarkistaa löytyykö varausta hakuehdoilla
        {
            return _context.Reservations.Any(e => e.Id == id);
        }
    }
}
