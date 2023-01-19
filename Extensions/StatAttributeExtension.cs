using Kenedia.Modules.BuildsManager.Enums;

namespace Kenedia.Modules.BuildsManager.Extensions
{
    internal static class StatAttributeExtension
    {
        public static string getLocalName(this API.StatAttribute attribute)
        {
            string text = attribute.Name;

            switch (attribute.Id)
            {
                case (int)Stats.Power:
                    return Strings.common.Power;

                case (int)Stats.Precision:
                    return Strings.common.Precision;

                case (int)Stats.Toughness:
                    return Strings.common.Toughness;

                case (int)Stats.Vitality:
                    return Strings.common.Vitality;

                case (int)Stats.Ferocity:
                    return Strings.common.Ferocity;

                case (int)Stats.HealingPower:
                    return Strings.common.HealingPower;

                case (int)Stats.ConditionDamage:
                    return Strings.common.ConditionDamage;

                case (int)Stats.Concentration:
                    return Strings.common.Concentration;

                case (int)Stats.Expertise:
                    return Strings.common.Expertise;
            }

            return text;
        }
    }
}
