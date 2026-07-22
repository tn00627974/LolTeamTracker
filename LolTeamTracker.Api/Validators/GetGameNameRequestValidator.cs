using FluentValidation;
using LolTeamTracker.Api.Models.Requests;

namespace LolTeamTracker.Api.Validators
{
    public class GetGameNameRequestValidator : AbstractValidator<GetGameNameRequest>
    {
        public GetGameNameRequestValidator()
        {
            RuleFor(x => x.Puuid).ValidPuuid();
        }
    }
}

