using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RuleKernel.Core.Data;
using RuleKernel.Core.Models;

namespace RuleKernel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly RuleKernelDbContext _context;

    public TenantsController(RuleKernelDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Tenant>>> GetAll()
    {
        return await _context.Tenants.ToListAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Tenant>> GetById(Guid id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        return tenant is null ? NotFound() : Ok(tenant);
    }

    [HttpPost]
    public async Task<ActionResult<Tenant>> Create(CreateTenantRequest request)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.Name
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = tenant.Id }, tenant);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant is null) return NotFound();

        _context.Tenants.Remove(tenant);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public record CreateTenantRequest(string Name);
