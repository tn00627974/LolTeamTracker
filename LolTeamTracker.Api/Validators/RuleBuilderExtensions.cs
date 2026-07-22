using FluentValidation;

namespace LolTeamTracker.Api.Validators
{
    /// <summary>
    /// 規則零件箱 ( 欄位級規則，不綁端點 )
    /// </summary>
    public static class RuleBuilderExtensions
    {
        /// <summary>
        /// 驗證 Puuid 格式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rule"></param>
        /// <returns></returns>
        public static IRuleBuilderOptions<T, string> ValidPuuid<T>(this IRuleBuilder<T, string> rule)
        {
            return rule.NotEmpty().WithMessage("puuid 不可為空")
                .Matches(@"^[a-zA-Z0-9_-]{78}$")
                .WithMessage("puuid 格式不正確");
        }

        /// <summary>
        /// 驗證 MatchId 格式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rule"></param>
        /// <returns></returns>
        public static IRuleBuilderOptions<T, string> ValidMatchId<T>(this IRuleBuilder<T, string> rule)
        {
            return rule.NotEmpty().WithMessage("MatchId 不可為空")
                .Matches(@"^[A-Z0-9]+_\d+$") 
                .WithMessage("MatchIds 格式不正確");
        }
        // TODO(子文): ValidCount 想一下——兩個端點範圍不一樣，這條規則是不是該吃 (int min, int max) 參數？

    }


}
