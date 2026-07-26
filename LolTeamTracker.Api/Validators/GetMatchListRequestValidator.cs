using FluentValidation;
using LolTeamTracker.Api.Models.Requests;

namespace LolTeamTracker.Api.Validators
{
    public class GetMatchListRequestValidator : AbstractValidator<GetMatchListRequest>
    {
        public GetMatchListRequestValidator()
        {
            RuleFor(x => x.GameName).ValidGameName();
            RuleFor(x => x.TagLine).ValidTagLine();
            RuleFor(x => x.Count).ValidCount(20,50);
        }
    }
}

