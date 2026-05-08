// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Controllers/ClientsController.cs
//  ----------------------------------------------------------------------
//  RESTful endpoints for Clients. Returns JSON DTOs; all actions require a
//  valid JWT (controller-level [Authorize]).
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Create web APIs with ASP.NET Core. [Online]. Available at:
//     https://learn.microsoft.com/en-us/aspnet/core/web-api/ [Accessed 31 May 2026].
//   - Microsoft. 2024. Entity Framework Core. [Online]. Available at:
//     https://learn.microsoft.com/en-us/ef/core/ [Accessed 31 May 2026].
//   - Microsoft. 2024. Language Integrated Query (LINQ) in C#. [Online]. Available at:
//     https://learn.microsoft.com/en-us/dotnet/csharp/linq/ [Accessed 31 May 2026].
// ==========================================================================

using GreenLogisticsMS.Api.Data;
using GreenLogisticsMS.Api.DTOs;
using GreenLogisticsMS.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenLogisticsMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(ApplicationDbContext context, ILogger<ClientsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>GET /api/clients - all clients.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ClientDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ClientDto>>> GetClients()
        {
            var clients = await _context.Clients
                .Select(c => new ClientDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ContactDetails = c.ContactDetails,
                    Region = c.Region
                })
                .ToListAsync();
            return Ok(clients);
        }

        /// <summary>GET /api/clients/{id}.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClientDto>> GetClient(int id)
        {
            var c = await _context.Clients.FindAsync(id);
            if (c == null) return NotFound();
            return Ok(new ClientDto { Id = c.Id, Name = c.Name, ContactDetails = c.ContactDetails, Region = c.Region });
        }

        /// <summary>POST /api/clients.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ClientDto>> CreateClient([FromBody] ClientCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var client = new Client { Name = dto.Name, ContactDetails = dto.ContactDetails, Region = dto.Region };
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            _logger.LogInformation("[ClientsController] Created client #{Id}.", client.Id);

            var result = new ClientDto { Id = client.Id, Name = client.Name, ContactDetails = client.ContactDetails, Region = client.Region };
            return CreatedAtAction(nameof(GetClient), new { id = client.Id }, result);
        }

        /// <summary>PUT /api/clients/{id}.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateClient(int id, [FromBody] ClientCreateDto dto)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();

            client.Name = dto.Name;
            client.ContactDetails = dto.ContactDetails;
            client.Region = dto.Region;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>DELETE /api/clients/{id}.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
