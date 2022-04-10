using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Overlay.UI.Views;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework.Graphics;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Blish_HUD;
using Blish_HUD.Input;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.BuildsManager
{
    public class iTab : Panel
    {
        public TabbedWindow2 Parent;
        public Texture2D Icon;
        public string Name;
        public Panel Panel;

        public iTab()
        {
            MouseEntered += delegate
            {
                BuildsManager.Logger.Debug("MOUSE ENTERED");
                Parent.Invalidate();
            };
        }

        public void Draw(Control tabbedControl, SpriteBatch spriteBatch, Rectangle bounds, bool selected, bool hovered)
        {
            if (this.Icon == null) return;

            // TODO: If not enabled, draw darker to indicate it is disabled

            spriteBatch.DrawOnCtrl(tabbedControl,
                                   Icon,
                                   new Rectangle(bounds.Right - bounds.Width / 2 - this.Icon.Width / 2,
                                                 bounds.Bottom - bounds.Height / 2 - this.Icon.Height / 2,
                                                 this.Icon.Width,
                                                 this.Icon.Height),
                                   selected || hovered
                                        ? Color.White
                                        : ContentService.Colors.DullColor);
        }
    }

    public class iMainWindow : TabbedWindow2
    {
        iDataManager DataManager;
        public FlowPanel Gear_View;
        public FlowPanel Build_View;
        public iTab Gear_Tab;
        public iTab Build_Tab;
        public iTab active_Tab;
        public iTab hovered_Tab;
        public List<iTab> iTabs = new List<iTab>();

        public iMainWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion, iDataManager dataManager, Container parent) : base (background, windowRegion, contentRegion)
        {
            DataManager = dataManager;
            Parent = parent;
            Title = "Builds Manager";
            Emblem = DataManager.getEmblem(_Emblems.SwordAndShield);
            Subtitle = "Gear";
            SavesPosition = true;
            Id = $"BuildsManager";

            Gear_View = new FlowPanel()
            {
                Parent = this,
                Size = ContentRegion.Size,
                Location = new Point(5, 5),
                Visible = true,
                CanScroll = true,
            };
            Gear_Tab = new iTab()
            {
                Icon = DataManager.getIcon(_Icons.Helmet),
                Name = "Gear",
                Panel = Gear_View,
                Parent = this,
                CanScroll = true,
            };
            iTabs.Add(Gear_Tab);
            active_Tab = Gear_Tab;

            Build_View = new FlowPanel()
            {
                Parent = this,
                Size = ContentRegion.Size,
                Location = new Point(5,5),
                Visible = false,
            };
            Build_Tab = new iTab()
            {
                Icon = DataManager.getIcon(_Icons.Template),
                Name = "Build",
                Panel = Build_View,
                Parent = this,
            };
            iTabs.Add(Build_Tab);
            hovered_Tab = Build_Tab;    
        }

        private const int TAB_VERTICALOFFSET = 40;

        private const int TAB_HEIGHT = 50;
        private const int TAB_WIDTH = 84;

        private void UpdateTabStates()
        {
            SideBarHeight = TAB_VERTICALOFFSET + TAB_HEIGHT * iTabs.Count;
            hovered_Tab = null;

                for (int i = 0; i < iTabs.Count; i++)
                {
                    var rect = new Rectangle(new Point(SidebarActiveBounds.X, SidebarActiveBounds.Y + (TAB_HEIGHT * i) + TAB_VERTICALOFFSET), new Point(TAB_WIDTH, TAB_HEIGHT));
                    if (rect.Contains(RelativeMousePosition))
                    {
                        hovered_Tab = iTabs[i];
                        BasicTooltipText = hovered_Tab?.Name;
                    }
                }
        }
        protected override void OnClick(MouseEventArgs e)
        {
            if (this.hovered_Tab != null)
            {
                this.active_Tab.Panel.Hide();

                this.active_Tab = this.hovered_Tab;
                this.active_Tab.Panel.Show();

                Subtitle = active_Tab.Name;
            }

            base.OnClick(e);
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            UpdateTabStates();

            base.UpdateContainer(gameTime);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            int tabIndex = 0;
            foreach (iTab tab in this.iTabs)
            {
                int tabTop = this.SidebarActiveBounds.Top + TAB_VERTICALOFFSET + tabIndex * TAB_HEIGHT;

                bool selected = tab == active_Tab;
                bool hovered = tab == hovered_Tab;

                if (selected)
                {
                    var tabBounds = new Rectangle(this.SidebarActiveBounds.Left - (TAB_WIDTH - this.SidebarActiveBounds.Width) + 2,
                                                  tabTop,
                                                  TAB_WIDTH,
                                                  TAB_HEIGHT);

                    spriteBatch.DrawOnCtrl(this,
                                           this.WindowBackground,
                                           tabBounds,
                                           new Rectangle(this.WindowRegion.Left + tabBounds.X,
                                                         tabBounds.Y - (int)this.Padding.Top,
                                                         tabBounds.Width,
                                                         tabBounds.Height));

                    spriteBatch.DrawOnCtrl(this, DataManager.getControlTexture(_Controls.TabActive), tabBounds);
                }

                tab.Draw(this,
                         spriteBatch,
                         new Rectangle(this.SidebarActiveBounds.X,
                                       tabTop,
                                       this.SidebarActiveBounds.Width,
                                       TAB_HEIGHT),
                         selected,
                         hovered);

                tabIndex++;
            }
        }
    }

    public class iGearView : View
    {
        public FlowPanel ContentPanel;
        public iMainWindow MainWindow;
        public iGearView(iMainWindow mainWindow)
        {
            MainWindow = mainWindow;
            
        }

        protected override void Build(Container buildPanel)
        {
            ContentPanel = new FlowPanel
            {
                Location = new Point(5, 5),
                Size = buildPanel.ContentRegion.Size,
                CanScroll = true,
                Parent = buildPanel
            };

        }
    }
    
    public class iBuildVIew : View
    {
        public iMainWindow MainWindow;
        public iBuildVIew(iMainWindow mainWindow)
        {
            MainWindow = mainWindow;

        }

    }
}
