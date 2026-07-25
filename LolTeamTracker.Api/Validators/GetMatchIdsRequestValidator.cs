using FluentValidation;
using LolTeamTracker.Api.Models.Requests;

namespace LolTeamTracker.Api.Validators
{
    public class GetMatchIdsRequestValidator : AbstractValidator<GetMatchIdsRequest>
    {
        public GetMatchIdsRequestValidator()
        {
            RuleFor(x => x.Puuid).ValidPuuid();

            RuleFor(x => x.Count).InclusiveBetween(10, 100).WithMessage("Count must be between 10 and 100.");
        }
    }
}

