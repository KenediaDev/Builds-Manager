using Kenedia.Modules.BuildsManager.Controls;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class SkillChangedEvent
    {
        public API.Skill Skill;
        public Skill_Control Skill_Control;

        public SkillChangedEvent(API.Skill skill, Skill_Control skill_Control)
        {
            Skill = skill;
            Skill_Control = skill_Control;
        }
    }
}