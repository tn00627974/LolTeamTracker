using LolTeamTracker.Api.Models.Entities;

namespace LolTeamTracker.Api.Repositories
{
    public interface IEfQueueDefinitionRepository
    {
        Task<List<QueueDefinition>> LoadQueueDefinitionDataAsync();
        Task InsertIfNotExistsAsync(QueueDefinition queueDefinition);

    }
}