using LolTeamTracker.Api.Data;
using LolTeamTracker.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Numerics;

namespace LolTeamTracker.Api.Repositories
{
    public class EfQueueDefinitionRepository : IEfQueueDefinitionRepository
    {
        private readonly AppDbContext _db;
        public EfQueueDefinitionRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<QueueDefinition>> LoadQueueDefinitionDataAsync()
        {
            return await _db.QueueDefinitions
                .AsNoTracking()
                .Select(p => new QueueDefinition
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    UpdatedAt = p.UpdatedAt,
                }).ToListAsync();
        }

        public async Task InsertIfNotExistsAsync(QueueDefinition queueDefinition)
        {
            var queueDefinitionData = await _db.QueueDefinitions.SingleOrDefaultAsync(q => q.Id == queueDefinition.Id);

            if (queueDefinitionData == null)             
            {
                // UpdatedAt 由這一層決定，不信任呼叫端傳進來的時間——
                // 一律存 UTC，時區轉換是呈現層的責任。
                queueDefinition.UpdatedAt = DateTime.UtcNow;
                _db.QueueDefinitions.Add(queueDefinition);
                await _db.SaveChangesAsync();
            }
        }
    }
}

