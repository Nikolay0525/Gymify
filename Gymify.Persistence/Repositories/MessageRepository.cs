using Gymify.Data.Entities;
using Gymify.Data.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gymify.Persistence.Repositories;

public class MessageRepository(GymifyDbContext context)
    : Repository<Message>(context), IMessageRepository
{
    private readonly GymifyDbContext _context = context;

    public async Task<List<Message>> GetMessagesByChatIdAsync(Guid chatId, int skip = 0, int take = 50)
    {
        return await _context.Messages
            .AsNoTracking()
            .Where(m => m.ChatId == chatId)
            .Include(m => m.Sender)
                .ThenInclude(u => u.ApplicationUser)
            .Include(m => m.Sender.Equipment.Avatar)
            .OrderByDescending(m => m.CreatedAt) 
            .Skip(skip)
            .Take(take)
            .ToListAsync(); 
    }

    public async Task<Message> FindLastMessageAsync(Guid chatId)
    {
        return await Entities
                .Where(m => m.ChatId == chatId)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();
    }

    public async Task<int> CountUnreadMessagesAsync(Guid chatId, Guid userId)
    {
        return await _context.Messages
            .Where(m => m.ChatId == chatId)
            .Where(m => m.SenderId != userId)
            .Where(m => !_context.MessageReadStatuses
                .Any(s => s.MessageId == m.Id && s.UserProfileId == userId))
            .CountAsync();
    }
}
