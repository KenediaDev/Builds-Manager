namespace Kenedia.Modules.BuildsManager.Controls
{
    using Blish_HUD;
    using Blish_HUD.Controls;
    using Microsoft.Xna.Framework.Graphics;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class Control_TemplateTooltip : Control
    {
        public Control_TemplateTooltip()
        {
            this.Parent = GameService.Graphics.SpriteScreen;

        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
        }
    }
}
