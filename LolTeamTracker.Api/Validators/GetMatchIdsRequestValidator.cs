using FluentValidation;
using LolTeamTracker.Api.Models.Requests;

namespace LolTeamTracker.Api.Validators
{
    public class GetMatchIdRequestValidator : AbstractValidator<GetMatchIdRequest>
    {
        public GetMatchIdRequestValidator()
        {
            RuleFor(x => x.MatchId).ValidMatchId();
        }
    }
}

