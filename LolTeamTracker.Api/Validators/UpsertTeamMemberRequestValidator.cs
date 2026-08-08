using FluentValidation;
using LolTeamTracker.Api.Models.Requests;

namespace LolTeamTracker.Api.Validators
{
    public class UpsertTeamMemberRequestValidator : AbstractValidator<UpsertTeamMemberRequest>
    {
        public UpsertTeamMemberRequestValidator()
        {
            RuleFor(x => x.GameName).ValidGameName();
            RuleFor(x => x.TagLine).ValidTagLine();
        }
    }
}
