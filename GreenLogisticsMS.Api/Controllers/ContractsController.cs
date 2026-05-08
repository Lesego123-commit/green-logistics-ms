// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Controllers/ContractsController.cs
//  ----------------------------------------------------------------------
//  Core SOA endpoints required by the brief:
//    GET    /api/contracts                 (with LINQ filtering)
//    GET    /api/contracts/{id}
//    POST   /api/contracts
//    PATCH  /api/contracts/{id}/status     (approve / decline / hold)
//    POST   /api/contracts/{id}/agreement  (PDF upload)
//    GET    /api/contracts/{id}/agreement  (PDF download)
//  Fires the Observer pattern when a contract moves to Expired / OnHold.
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Create web APIs with ASP.NET Core. [Online]. Available at:
//     https://learn.microsoft.com/en-us/aspnet/core/web-api/ [Accessed 31 May 2026].
//   - Microsoft. 2024. Language Integrated Query (LINQ) in C#. [Online]. Available at:
//     https://learn.microsoft.com/en-us/dotnet/csharp/linq/ [Accessed 31 May 2026].
//   - Gamma, E., Helm, R., Johnson, R. & Vlissides, J. 1995. Design Patterns.
//     Boston: Addison-Wesley. (Observer pattern.)
// ==========================================================================

using GreenLogisticsMS.Api.Data;
using GreenLogisticsMS.Api.DTOs;
using GreenLogisticsMS.Api.Models;
using GreenLogisticsMS.Api.Services;
using GreenLogisticsMS.Api.Services.Patterns.Observer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenLogisticsMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContractsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<ContractsController> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public ContractsController(
            ApplicationDbContext context,
            IFileService fileService,
            ILogger<ContractsController> logger,
            ILoggerFactory loggerFactory)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// GET /api/contracts?status=Active&amp;clientId=1&amp;startFrom=2026-01-01&amp;startTo=2026-12-31
        /// All filter parameters are optional (LINQ filtering).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ContractDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ContractDto>>> GetContracts(
            [FromQuery] ContractStatus? status,
            [FromQuery] int? clientId,
            [FromQuery] DateTime? startFrom,
            [FromQuery] DateTime? startTo)
        {
            IQueryable<Contract> query = _context.Contracts.Include(c => c.Client);

            if (status.HasValue) query = query.Where(c => c.Status == status.Value);
            if (clientId.HasValue) query = query.Where(c => c.ClientId == clientId.Value);
            if (startFrom.HasValue) query = query.Where(c => c.StartDate >= startFrom.Value);
            if (startTo.HasValue) query = query.Where(c => c.StartDate <= startTo.Value);

            var contracts = await query
                .OrderByDescending(c => c.StartDate)
                .Select(c => ToDto(c))
                .ToListAsync();

            return Ok(contracts);
        }

        /// <summary>GET /api/contracts/{id}.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContractDto>> GetContract(int id)
        {
            var contract = await _context.Contracts.Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (contract == null) return NotFound();
            return Ok(ToDto(contract));
        }

        /// <summary>POST /api/contracts.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ContractDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ContractDto>> CreateContract([FromBody] ContractCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!await _context.Clients.AnyAsync(c => c.Id == dto.ClientId))
                return BadRequest(new { message = $"Client #{dto.ClientId} does not exist." });

            if (dto.EndDate < dto.StartDate)
                return BadRequest(new { message = "End date cannot be before the start date." });

            var contract = new Contract
            {
                ClientId = dto.ClientId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status,
                ServiceLevel = dto.ServiceLevel
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();
            FireObserverIfNeeded(contract);
            _logger.LogInformation("[ContractsController] Created contract #{Id}.", contract.Id);

            await _context.Entry(contract).Reference(c => c.Client).LoadAsync();
            return CreatedAtAction(nameof(GetContract), new { id = contract.Id }, ToDto(contract));
        }

        /// <summary>
        /// PATCH /api/contracts/{id}/status - approve, decline, expire or hold a contract.
        /// Enforces a simple workflow: a Completed/Expired contract cannot be reactivated to Draft.
        /// </summary>
        [HttpPatch("{id:int}/status")]
        [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContractDto>> UpdateStatus(int id, [FromBody] ContractStatusUpdateDto dto)
        {
            var contract = await _context.Contracts.Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (contract == null) return NotFound();

            // ── Workflow rule: cannot move an Expired contract back to Draft ──
            if (contract.Status == ContractStatus.Expired && dto.Status == ContractStatus.Draft)
                return BadRequest(new { message = "An expired contract cannot be returned to Draft." });

            contract.Status = dto.Status;
            await _context.SaveChangesAsync();
            FireObserverIfNeeded(contract);
            _logger.LogInformation("[ContractsController] Contract #{Id} status -> {Status}.", id, dto.Status);

            return Ok(ToDto(contract));
        }

        /// <summary>DELETE /api/contracts/{id}.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteContract(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();
            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>POST /api/contracts/{id}/agreement - upload the signed PDF.</summary>
        [HttpPost("{id:int}/agreement")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadAgreement(int id, IFormFile file)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            try
            {
                var (path, name) = await _fileService.SavePdfAsync(file);
                contract.SignedAgreementPath = path;
                contract.SignedAgreementFileName = name;
                await _context.SaveChangesAsync();
                return Ok(new { contract.SignedAgreementFileName });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>GET /api/contracts/{id}/agreement - download the signed PDF.</summary>
        [HttpGet("{id:int}/agreement")]
        public async Task<IActionResult> DownloadAgreement(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementPath))
                return NotFound(new { message = "No signed agreement found for this contract." });

            var fullPath = _fileService.GetAbsolutePath(contract.SignedAgreementPath);
            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { message = "File not found on server." });

            var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(bytes, "application/pdf", contract.SignedAgreementFileName ?? "agreement.pdf");
        }

        // ── Helpers ─────────────────────────────────────────────────────
        private static ContractDto ToDto(Contract c) => new()
        {
            Id = c.Id,
            ClientId = c.ClientId,
            ClientName = c.Client?.Name ?? string.Empty,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            Status = c.Status,
            ServiceLevel = c.ServiceLevel,
            SignedAgreementFileName = c.SignedAgreementFileName
        };

        private void FireObserverIfNeeded(Contract contract)
        {
            if (contract.Status is ContractStatus.Expired or ContractStatus.OnHold)
            {
                var notifier = new ContractStatusNotifier();
                var alert = new ExpiryAlertService(_loggerFactory.CreateLogger<ExpiryAlertService>());
                notifier.RegisterObserver(alert);
                notifier.NotifyObservers(contract);
            }
        }
    }
}
