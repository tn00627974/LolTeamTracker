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

        public static IRuleBuilderOptions<T, string> ValidGameName<T>(this IRuleBuilder<T, string> rule)
        {
            return rule.NotEmpty().WithMessage("GameName 不可為空");
        }

        public static IRuleBuilderOptions<T, string> ValidTagLine<T>(this IRuleBuilder<T, string> rule)
        {
            return rule.NotEmpty().WithMessage("TagLine 不可為空");
        }

        /// <summary>
        /// 驗證 Count 是否落在指定範圍內
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rule"></param>
        /// <param name="minCount">允許的最小值</param>
        /// <param name="maxCount">允許的最大值</param>
        /// <returns></returns>
        public static IRuleBuilderOptions<T, int> ValidCount<T>(this IRuleBuilder<T, int> rule, int minCount, int maxCount)
        {
            return rule.InclusiveBetween(minCount, maxCount).WithMessage($"Count must be between {minCount} and {maxCount}.");
        }
    }
}
