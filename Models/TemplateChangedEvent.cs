namespace Kenedia.Modules.BuildsManager.Models
{
    public class TemplateChangedEvent
    {
        public Template Template;

        public TemplateChangedEvent(Template template)
        {
            Template = template;
        }
    }
}
