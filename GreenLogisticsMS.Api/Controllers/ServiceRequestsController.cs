// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Controllers/ServiceRequestsController.cs
//  ----------------------------------------------------------------------
//  Uses the Factory pattern to build the right ServiceRequest type and the
//  Currency service to convert USD->ZAR. Enforces the business rule that a
//  request cannot be raised against an Expired or OnHold contract.
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Create web APIs with ASP.NET Core. [Online]. Available at:
//     https://learn.microsoft.com/en-us/aspnet/core/web-api/ [Accessed 31 May 2026].
//   - Gamma, E., Helm, R., Johnson, R. & Vlissides, J. 1995. Design Patterns.
//     Boston: Addison-Wesley. (Factory Method pattern.)
//   - Microsoft. 2024. Asynchronous programming with async and await. [Online].
//     Available at: https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/ [Accessed 31 May 2026].
// ==========================================================================

using GreenLogisticsMS.Api.Data;
using GreenLogisticsMS.Api.DTOs;
using GreenLogisticsMS.Api.Models;
using GreenLogisticsMS.Api.Services;
using GreenLogisticsMS.Api.Services.Patterns.Factory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenLogisticsMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<ServiceRequestsController> _logger;

        public ServiceRequestsController(
            ApplicationDbContext context,
            ICurrencyService currencyService,
            ILogger<ServiceRequestsController> logger)
        {
            _context = context;
            _currencyService = currencyService;
            _logger = logger;
        }

        /// <summary>GET /api/servicerequests?contractId=5 (filter optional).</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ServiceRequestDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ServiceRequestDto>>> GetRequests([FromQuery] int? contractId)
        {
            IQueryable<ServiceRequest> query = _context.ServiceRequests;
            if (contractId.HasValue) query = query.Where(s => s.ContractId == contractId.Value);

            var list = await query.OrderByDescending(s => s.CreatedDate)
                .Select(s => ToDto(s)).ToListAsync();
            return Ok(list);
        }

        /// <summary>GET /api/servicerequests/{id}.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ServiceRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ServiceRequestDto>> GetRequest(int id)
        {
            var sr = await _context.ServiceRequests.FindAsync(id);
            if (sr == null) return NotFound();
            return Ok(ToDto(sr));
        }

        /// <summary>POST /api/servicerequests - creates a request via the Factory + currency conversion.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ServiceRequestDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ServiceRequestDto>> CreateRequest([FromBody] ServiceRequestCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var contract = await _context.Contracts.FirstOrDefaultAsync(c => c.Id == dto.ContractId);
            if (contract == null)
                return BadRequest(new { message = $"Contract #{dto.ContractId} does not exist." });

            // ── Business rule: block requests on Expired / OnHold contracts ──
            if (contract.Status is ContractStatus.Expired or ContractStatus.OnHold)
                return BadRequest(new { message = $"Cannot create a request: contract is '{contract.Status}'." });

            var rate = await _currencyService.GetUsdToZarRateAsync();
            var zar = Math.Round(dto.CostUSD * rate, 2);

            IServiceRequestFactory factory = dto.RequestType == RequestType.International
                ? new InternationalRequestCreator()
                : new DomesticRequestCreator();

            var sr = factory.CreateRequest(dto.ContractId, dto.Description, dto.CostUSD, zar, rate);
            _context.ServiceRequests.Add(sr);
            await _context.SaveChangesAsync();
            _logger.LogInformation("[ServiceRequestsController] Created request #{Id} (USD {Usd} -> ZAR {Zar}).",
                sr.Id, dto.CostUSD, zar);

            return CreatedAtAction(nameof(GetRequest), new { id = sr.Id }, ToDto(sr));
        }

        /// <summary>DELETE /api/servicerequests/{id}.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var sr = await _context.ServiceRequests.FindAsync(id);
            if (sr == null) return NotFound();
            _context.ServiceRequests.Remove(sr);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static ServiceRequestDto ToDto(ServiceRequest s) => new()
        {
            Id = s.Id,
            ContractId = s.ContractId,
            Description = s.Description,
            RequestType = s.RequestType,
            Status = s.Status,
            CostUSD = s.CostUSD,
            CostZAR = s.CostZAR,
            ExchangeRateUsed = s.ExchangeRateUsed,
            CreatedDate = s.CreatedDate
        };
    }
}
