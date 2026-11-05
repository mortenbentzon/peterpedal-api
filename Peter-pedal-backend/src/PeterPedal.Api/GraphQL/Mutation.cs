using HotChocolate;
using Microsoft.EntityFrameworkCore;
using PeterPedal.Api.Data;
using PeterPedal.Api.Models;

namespace PeterPedal.Api.GraphQL;

public class Mutation
{
    public async Task<RepairCase> CreateCase(int customerId, string frameNumber, string problem,
        [Service] PeterPedalDbContext db)
    {
        var repairCase = new RepairCase { CustomerId = customerId, FrameNumber = frameNumber, Problem = problem };
        db.RepairCases.Add(repairCase);
        await db.SaveChangesAsync();
        await db.Entry(repairCase).Reference(c => c.Customer).LoadAsync();
        return repairCase;
    }

    public async Task<RepairCase?> AddPart(int caseId, int partId, [Service] PeterPedalDbContext db)
    {
        var repairCase = await Load(db, caseId);
        var part = await db.SpareParts.FindAsync(partId);
        
        if (repairCase == null || part == null) 
        {
            return null;
        }

        // Tilføj kun, hvis delen ikke allerede er på sagen
        if (!repairCase.Parts.Any(p => p.Id == partId))
        {
            repairCase.Parts.Add(part);
            await db.SaveChangesAsync();
        }
        
        return repairCase;
    }

    public async Task<RepairCase?> CalculateOffer(int caseId, [Service] PeterPedalDbContext db)
    {
        var repairCase = await Load(db, caseId);
        
        if (repairCase == null) 
        {
            return null;
        }

        repairCase.Price = PriceCalculator.CalculateOffer(repairCase.Parts);
        await db.SaveChangesAsync();
        
        return repairCase;
    }

    public async Task<RepairCase?> SetStatus(int caseId, CaseStatus status, [Service] PeterPedalDbContext db)
    {
        var repairCase = await Load(db, caseId);
        if (repairCase is null)
        {
            return null;
        }
        repairCase.Status = status;
        await db.SaveChangesAsync();
        return repairCase;
    }

    public async Task<bool> DeleteCase(int caseId, [Service] PeterPedalDbContext db)
    {
        var repairCase = await Load(db, caseId);
        
        if (repairCase == null) 
        {
            return false;
        }

        db.RepairCases.Remove(repairCase);
        await db.SaveChangesAsync();
        
        return true;
    }

    private static Task<RepairCase?> Load(PeterPedalDbContext db, int caseId) =>
        db.RepairCases.Include(c => c.Customer).Include(c => c.Parts)
            .FirstOrDefaultAsync(c => c.Id == caseId);
}