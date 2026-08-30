using LolTeamTracker.Api.Models.Entities;
using LolTeamTracker.Api.Models.Requests;
using LolTeamTracker.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LolTeamTracker.Api.Controllers
{
    /// <summary>
    /// 遊戲模式對照表（queueId → 名稱）的維護端點。
    /// 這是參照資料：Riot 新增模式時只要 INSERT 一列，不用改程式碼重新部署。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class QueueDefinitionsController : ControllerBase
    {
        private readonly IEfQueueDefinitionRepository _queueDefinitionRepository;
        public QueueDefinitionsController(IEfQueueDefinitionRepository queueDefinitionRepository)
        {
            _queueDefinitionRepository = queueDefinitionRepository;
        }

        /// <summary>
        /// 取得全部遊戲模式資料。
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var results = await _queueDefinitionRepository.LoadQueueDefinitionDataAsync();
            return Ok(results);
        }

        /// <summary>
        /// 新增一筆遊戲模式資料。已存在的 Id 會被忽略（冪等），一律回 204。
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateQueueDefinitionRequest request)
        {
            // Request Model → Entity。
            // 不直接收 Entity：那會把 Matches 導覽屬性開放給呼叫端，
            // EF Core 會真的把巢狀的 Match / MatchPlayer 一起 INSERT 進去。
            var entity = new QueueDefinition
            {
                Id = request.Id,
                Name = request.Name,
                Description = request.Description
                // UpdatedAt 由 Repository 設定，不接受呼叫端指定
            };

            await _queueDefinitionRepository.InsertIfNotExistsAsync(entity);
            return NoContent();
        }
    }
}
