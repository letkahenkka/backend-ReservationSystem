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
    public class ItemsController : ControllerBase
    {
        private readonly IItemService _service;
        private readonly IUserAuthenticationService _authenticationService;

        public ItemsController(IItemService service, IUserAuthenticationService authenticationService)
        {
            _service = service;
            _authenticationService = authenticationService;
        }

        // GET: api/Items
        /// <summary>
        /// Gets all the items in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ItemDTO>>> GetItems() //hakee listan kaikista itemeistä
        {
            return Ok(await _service.GetItemsAsync());
        }

        // GET: api/Items/user/username
        /// Gets all the items in the system matching the given username
        /// </summary>
        /// <returns></returns>
        [HttpGet("user/{username}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ItemDTO>>> GetItems(String username) //hakee listan käyttäjänimen tai sen osan perusteella
        {
            return Ok(await _service.GetItemsAsync(username));
        }

        // GET: api/Items/query
        /// <summary>
        /// Gets all the items in the system matching the given query
        /// </summary>
        /// <returns></returns>
        [HttpGet("{query}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ItemDTO>>> QueryItems(String query) //
        {
            return Ok(await _service.QueryItemsAsync(query));
        }


        // GET: api/Items/5
        /// <summary>
        /// Gets a single item based in id
        /// </summary>
        /// <param name="id">Item id</param>
        /// <returns>A single item</returns>
        /// <response code="201">Successfully returns the item</response>
        /// <response code="404">Could not find item</response>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ItemDTO>> GetItem(long id) //hakee id:n perusteella itemin
        {
            var item = await _service.GetItemAsync(id);

            if (item == null)
            {
                return NotFound();
            }
            return item;
        }

        // PUT: api/Items/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutItem(long id, ItemDTO item) //päivittää itemin id:n perusteella
        {
            if (id != item.Id)
            {
                return BadRequest();
            }

            //Tarkista, onko oikeus muokata
            bool isAllowed = await _authenticationService.IsAllowed(this.User.FindFirst(ClaimTypes.Name).Value, item);

            if (!isAllowed)
            {
                return Unauthorized();
            }

            ItemDTO updatedItem = await _service.UpdateItemAsync(item);

            if(updatedItem == null)
            {
                return NotFound();
            }
            return NoContent();
        }

        // POST: api/Items
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]

        public async Task<ActionResult<ItemDTO>> PostItem(ItemDTO item) //lisää uuden itemin
        {
            //Tarkista, onko oikeus muokata
            bool isAllowed = await _authenticationService.IsAllowed(this.User.FindFirst(ClaimTypes.Name).Value, item);

            if (!isAllowed)
            {
                return Unauthorized();
            }

            ItemDTO newItem = await _service.CreateItemAsync(item);
            if (newItem == null)
            {
                return Problem();
            }

            return CreatedAtAction("GetItem", new { id = newItem.Id }, newItem);
        }

        // DELETE: api/Items/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteItem(long id) //poistaa itemin id:n perusteella
        {
            ItemDTO item = new ItemDTO();
            item.Id = id;
            bool isAllowed = await _authenticationService.IsAllowed(this.User.FindFirst(ClaimTypes.Name).Value, item);

            if (!isAllowed)
            {
                return Unauthorized();
            }


            if (await _service.DeleteItemAsync(id))
            {
                return Ok();
            }
            return NotFound();
        }
    }
}
