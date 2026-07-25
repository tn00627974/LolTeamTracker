using FluentValidation;
using LolTeamTracker.Api.Models.Requests;

namespace LolTeamTracker.Api.Validators
{
    public class GetPuuidRequestValidator : AbstractValidator<GetPuuidRequest>
    {
        public GetPuuidRequestValidator()
        {
            RuleFor(x => x.GameName).ValidGameName();

            RuleFor(x => x.TagLine).ValidTagLine();
        }
    }
}

