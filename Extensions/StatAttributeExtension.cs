using Kenedia.Modules.BuildsManager.Enums;

namespace Kenedia.Modules.BuildsManager.Extensions
{
    internal static class StatAttributeExtension
    {
        public static string getLocalName(this API.StatAttribute attribute)
        {
            string text = attribute.Name;

            return attribute.Id switch
            {
                (int)Stats.Power => Strings.common.Power,
                (int)Stats.Precision => Strings.common.Precision,
                (int)Stats.Toughness => Strings.common.Toughness,
                (int)Stats.Vitality => Strings.common.Vitality,
                (int)Stats.Ferocity => Strings.common.Ferocity,
                (int)Stats.HealingPower => Strings.common.HealingPower,
                (int)Stats.ConditionDamage => Strings.common.ConditionDamage,
                (int)Stats.Concentration => Strings.common.Concentration,
                (int)Stats.Expertise => Strings.common.Expertise,
                _ => text,
            };
        }
    }
}
