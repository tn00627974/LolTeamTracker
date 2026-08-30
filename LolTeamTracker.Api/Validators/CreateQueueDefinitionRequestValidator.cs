using FluentValidation;
using LolTeamTracker.Api.Models.Requests;

namespace LolTeamTracker.Api.Validators
{
    /// <summary>
    /// 新增遊戲模式定義的驗證規則。
    /// 長度上限與資料表的 HasMaxLength 對齊——不擋的話會在 SaveChanges 才丟
    /// DbUpdateException，變成 500；擋在邊界才回得了 400 與具體訊息。
    /// </summary>
    public class CreateQueueDefinitionRequestValidator : AbstractValidator<CreateQueueDefinitionRequest>
    {
        public CreateQueueDefinitionRequestValidator()
        {
            // Riot 的 queueId 都是正整數（400、420、440…），0 與負數必為誤傳
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("QueueId 必須是正整數");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name 不可為空")
                .MaximumLength(50).WithMessage("Name 長度不可超過 50");

            // Description 可為 null（queues.json 部分模式沒有描述），
            // 但有值時仍受資料表長度限制
            RuleFor(x => x.Description)
                .MaximumLength(255).WithMessage("Description 長度不可超過 255");
        }
    }
}
