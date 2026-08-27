using Gymify.Data.Entities;
using Gymify.Data.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gymify.Persistence.Repositories;

public class CaseItemRepository(GymifyDbContext context) : ICaseItemRepository
{
    private readonly GymifyDbContext _context = context;

    public async Task<CaseItem> CreateAsync(CaseItem entity)
    {
        await _context.CaseItems.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<ICollection<CaseItem>> GetAllAsync()
    {
        return await _context.CaseItems
            .Include(uc => uc.Case)
            .Include(uc => uc.Item)
            .ToListAsync();
    }

    public async Task<ICollection<CaseItem>> GetAllByCaseIdAsync(Guid caseId)
    {
        var entities = await _context.CaseItems
            .Include(uc => uc.Case)
            .Include(uc => uc.Item)
            .Where(uc => uc.CaseId == caseId)
            .ToListAsync();

        return entities;
    }

    public async Task<CaseItem> UpdateAsync(CaseItem entity)
    {
        _context.CaseItems.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<CaseItem> DeleteByIdAsync(Guid id)
    {
        var entity = await _context.CaseItems.FirstOrDefaultAsync(uc => uc.CaseId == id);
        if (entity == null)
            throw new Exception("CaseItem not found");

        _context.CaseItems.Remove(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
}
