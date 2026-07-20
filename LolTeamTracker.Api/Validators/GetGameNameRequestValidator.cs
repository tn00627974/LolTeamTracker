using FluentValidation;
using LolTeamTracker.Api.Models.Requests;

namespace LolTeamTracker.Api.Validators
{
    public class GetGameNameRequestValidator : AbstractValidator<GetGameNameRequest>
    {
        public GetGameNameRequestValidator()
        {
            RuleFor(x => x.Puuid)
                .NotEmpty().WithMessage("puuid 不可為空")
                .Matches(@"^[a-zA-Z0-9_-]{78}$").WithMessage("puuid 格式不正確");
        }
    }
}
