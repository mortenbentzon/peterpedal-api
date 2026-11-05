using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeterPedal.Api.Data;
using PeterPedal.Api.Models;

namespace PeterPedal.Api.Controllers;

[ApiController]
[Route("api/spareparts")]
public class SparePartsController : ControllerBase
{
    private readonly PeterPedalDbContext _db;

    public SparePartsController(PeterPedalDbContext db) => _db = db;

    // Tilføjet: Henter alle reservedele
    [HttpGet]
    public async Task<List<SparePart>> GetAll() =>
        await _db.SpareParts.OrderBy(p => p.Id).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<SparePart>> GetById(int id)
    {
        var part = await _db.SpareParts.FindAsync(id);
        if (part == null) 
        {
            return NotFound();
        }
        return part;
    }

    [HttpPost]
    public async Task<ActionResult<SparePart>> Create(SparePart part)
    {
        part.Id = 0; // Databasen genererer selv det nye ID
        _db.SpareParts.Add(part);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = part.Id }, part);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SparePart>> Update(int id, SparePart input)
    {
        var part = await _db.SpareParts.FindAsync(id);
        if (part == null) 
        {
            return NotFound();
        }
    
        part.Name = input.Name;
        part.Price = input.Price;
        await _db.SaveChangesAsync();
    
        return part;
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var part = await _db.SpareParts.FindAsync(id);
        if (part == null) 
        {
            return NotFound();
        }
    
        _db.SpareParts.Remove(part);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}