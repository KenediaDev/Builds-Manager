namespace Kenedia.Modules.BuildsManager.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Blish_HUD;
    using Blish_HUD.Controls;
    using Kenedia.Modules.BuildsManager.Extensions;
    using Kenedia.Modules.BuildsManager.Models;
    using Microsoft.Xna.Framework;

    public class Control_TemplateSelection : FlowPanel
    {
        private bool ResizeChilds = false;
        public TextBox FilterBox;
        public FlowPanel ContentPanel;
        private Control_ProfessionSelector _ProfessionSelector;
        private List<Control_TemplateEntry> Templates = new List<Control_TemplateEntry>();

        public Control_TemplateSelection(Container parent)
        {
            this.Parent = parent;
            this.FlowDirection = ControlFlowDirection.SingleTopToBottom;
            this.ControlPadding = new Vector2(0, 3);
            this.FilterBox = new TextBox()
            {
                Location = new Point(5, 0),
                Parent = this,
                Width = this.Width - 5,
                PlaceholderText = Strings.common.Search + " ...",
            };

            this._ProfessionSelector = new Control_ProfessionSelector()
            {
                Parent = this,
                Size = new Point(this.Width - 5, this.FilterBox.Height),
                Location = new Point(5, this.FilterBox.Bottom + 5),
            };
            this.ContentPanel = new FlowPanel()
            {
                Parent = this,
                Size = new Point(this.Width, this.Height - this.AbsoluteBounds.Y - 5),
                Location = new Point(5, this._ProfessionSelector.Bottom + 5),
                CanScroll = true,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
            };

            this.Location = new Point(this.FilterBox.LocalBounds.Left, this._ProfessionSelector.LocalBounds.Bottom + 5);
            this.Size = new Point(255, parent.Height - (this.AbsoluteBounds.Y - 5));

            // BackgroundColor = Color.Magenta;
            this.Refresh();
            this.FilterBox.TextChanged += this.FilterBox_TextChanged;
            this._ProfessionSelector.Changed += this._ProfessionSelector_Changed;

            BuildsManager.ModuleInstance.LanguageChanged += this.ModuleInstance_LanguageChanged;
            BuildsManager.ModuleInstance.Templates_Loaded += this.ModuleInstance_Templates_Loaded;
            BuildsManager.ModuleInstance.Template_Deleted += this.ModuleInstance_Template_Deleted;
            this.ContentPanel.ChildAdded += this.ContentPanel_ChildsChanged;
            this.ContentPanel.ChildRemoved += this.ContentPanel_ChildsChanged;
        }

        private void _ProfessionSelector_Changed(object sender, EventArgs e)
        {
            this.RefreshList();
        }

        private void ModuleInstance_Template_Deleted(object sender, EventArgs e)
        {
            this.Refresh();
        }

        public void SetSelection()
        {
            var player = GameService.Gw2Mumble.PlayerCharacter;
            this._ProfessionSelector.Professions.Clear();
            this._ProfessionSelector.Professions.Add(BuildsManager.ModuleInstance.Data.Professions.Find(e => e.Id == player.Profession.ToString()));
            this.RefreshList();
        }

        private void ModuleInstance_LanguageChanged(object sender, EventArgs e)
        {
            this.FilterBox.PlaceholderText = Strings.common.Search + " ...";
        }

        private void ContentPanel_ChildsChanged(object sender, ChildChangedEventArgs e)
        {
            this.ResizeChilds = true;
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (this.ResizeChilds)
            {
                var not_fitting = this.ContentPanel.Height < (this.Templates.Where(e => e.Visible).ToList().Count * 38);
                foreach (Control_TemplateEntry template in this.Templates)
                {
                    template.Width = not_fitting ? this.Width - 20 : this.Width - 5;
                }

                this.ResizeChilds = false;
            }
        }

        private void ModuleInstance_Templates_Loaded(object sender, EventArgs e)
        {
            this.Refresh();
        }

        protected override void OnMoved(MovedEventArgs e)
        {
            base.OnMoved(e);

        }

        public void RefreshList()
        {
            this.ContentPanel.SuspendLayout();
            var filter = this.FilterBox.Text.ToLower();
            var prof = BuildsManager.ModuleInstance.CurrentProfession;

            this.ContentPanel.SortChildren<Control_TemplateEntry>((a, b) =>
            {
                var ret = (b.Template.Build.Profession == prof).CompareTo(a.Template.Build.Profession == prof);
                if (ret == 0)
                {
                    ret = a.Template.Build.Profession.Id.CompareTo(b.Template.Build.Profession.Id);
                }

                if (ret == 0 && a.Template.Specialization != null)
                {
                    ret = a.Template.Specialization.Id.CompareTo(b.Template.Specialization?.Id);
                }

                if (ret == 0)
                {
                    ret = a.Template.Name.CompareTo(b.Template.Name);
                }

                return ret;
            });

            foreach (Control_TemplateEntry template in this.Templates)
            {
                if (template.Template != null)
                {
                    var name = template.Template.Name.ToLower();

                    if ((this._ProfessionSelector.Professions.Count == 0 || this._ProfessionSelector.Professions.Contains(template.Template.Profession)) && name.Contains(filter))
                    {
                        template.Show();
                    }
                    else
                    {
                        template.Hide();
                    }
                }
            }

            this.ResizeChilds = true;
            this.ContentPanel.Invalidate();
            this.ContentPanel.ResumeLayout();
        }

        private void FilterBox_TextChanged(object sender, EventArgs e)
        {
            this.RefreshList();
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
            this.FilterBox.Width = this.Width - 5;
            this._ProfessionSelector.Width = this.Width - 5;
            this.ContentPanel.Size = new Point(this.Width, this.Height - this.LocalBounds.Y);
        }

        public void Refresh()
        {
            this.SuspendLayout();
            this.ContentPanel.SuspendLayout();

            foreach (Control_TemplateEntry template in new List<Control_TemplateEntry>(this.Templates))
            {
                if (BuildsManager.ModuleInstance.Templates.Find(e => e == template.Template) == null)
                {
                    template.Dispose();
                    this.Templates.Remove(template);
                }
            }

            foreach (Template template in BuildsManager.ModuleInstance.Templates)
            {
                if (this.Templates.Find(e => e.Template == template) == null)
                {
                    var ctrl = new Control_TemplateEntry(this.ContentPanel, template) { Size = new Point(this.Width - 20, 38) };
                    ctrl.TemplateChanged += this.OnTemplateChangedEvent;
                    this.Templates.Add(ctrl);

                    template.Deleted += this.Template_Deleted;
                }
            }

            this.ResumeLayout();
            this.ContentPanel.ResumeLayout();

            this.RefreshList();
        }

        private void Template_Deleted(object sender, EventArgs e)
        {
            var template = (Template)sender;
            var ctrl = this.Templates.Find(a => a.Template == template);
            ctrl?.Dispose();
            if (ctrl != null)
            {
                this.Templates.Remove(ctrl);
            }
        }

        public void Clear()
        {
            foreach (Control_TemplateEntry ctrl in this.Templates)
            {
                ctrl.Dispose();
            }

            this.Templates.Clear();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            foreach (Template template in BuildsManager.ModuleInstance.Templates)
            {
                template.Deleted -= this.Template_Deleted;
            }

            foreach (Control_TemplateEntry template in this.Templates)
            {
                template.TemplateChanged -= this.OnTemplateChangedEvent;
            }

            this.Templates.DisposeAll();

            this.ContentPanel?.Dispose();
            this.FilterBox?.Dispose();
            this._ProfessionSelector?.Dispose();

            this.FilterBox.TextChanged -= this.FilterBox_TextChanged;
            this._ProfessionSelector.Changed -= this._ProfessionSelector_Changed;

            BuildsManager.ModuleInstance.LanguageChanged -= this.ModuleInstance_LanguageChanged;
            BuildsManager.ModuleInstance.Templates_Loaded -= this.ModuleInstance_Templates_Loaded;
            BuildsManager.ModuleInstance.Template_Deleted -= this.ModuleInstance_Template_Deleted;
            this.ContentPanel.ChildAdded -= this.ContentPanel_ChildsChanged;
            this.ContentPanel.ChildRemoved -= this.ContentPanel_ChildsChanged;
        }

        public event EventHandler<TemplateChangedEvent> TemplateChanged;

        private void OnTemplateChangedEvent(Object sender, TemplateChangedEvent e)
        {
            this.TemplateChanged?.Invoke(this, new TemplateChangedEvent(e.Template));
        }
    }
}
