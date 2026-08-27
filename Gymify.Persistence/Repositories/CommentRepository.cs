using Gymify.Data.Entities;
using Gymify.Data.Enums;
using Gymify.Data.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gymify.Persistence.Repositories;

public class CommentRepository(GymifyDbContext context)
    : Repository<Comment>(context), ICommentRepository
{
    private readonly GymifyDbContext _context = context;
    public async Task<ICollection<Comment>> GetCommentsByTargetIdAndTypeAsync(Guid targetId, CommentTargetType targetType)
    {
        return await _context.Comments
            .AsNoTracking() 
            .Where(c => c.TargetId == targetId && c.TargetType == targetType)
            .Include(c => c.Author).ThenInclude(u => u.ApplicationUser)
            .Include(c => c.Author).ThenInclude(u => u.Equipment).ThenInclude(ue => ue.Avatar) 
            .OrderByDescending(c => c.CreatedAt)     
            .ToListAsync();
    }

    public async Task<ICollection<Comment>> GetUnapprovedAsync()
    {
        return await Entities
            .Where(c => c.IsApproved == false && c.IsRejected == false)
            .Include(c => c.Author)
            .ThenInclude(a => a.ApplicationUser)
            .ToListAsync();
    }
}
