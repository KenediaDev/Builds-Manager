using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Models;
using Microsoft.Xna.Framework;

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class ControlTemplateSelection : FlowPanel
    {
        private bool ResizeChilds = false;
        public TextBox FilterBox;
        public FlowPanel ContentPanel;
        private readonly Control_ProfessionSelector _ProfessionSelector;
        private readonly List<Control_TemplateEntry> Templates = new();

        public ControlTemplateSelection(Container parent)
        {
            Parent = parent;
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            ControlPadding = new Vector2(0, 3);
            FilterBox = new TextBox()
            {
                Location = new Point(5, 0),
                Parent = this,
                Width = Width - 5,
                PlaceholderText = Strings.common.Search + " ...",
            };

            _ProfessionSelector = new Control_ProfessionSelector()
            {
                Parent = this,
                Size = new Point(Width - 5, FilterBox.Height),
                Location = new Point(5, FilterBox.Bottom + 5),
            };
            ContentPanel = new FlowPanel()
            {
                Parent = this,
                Size = new Point(Width, Height - AbsoluteBounds.Y - 5),
                Location = new Point(5, _ProfessionSelector.Bottom + 5),
                CanScroll = true,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
            };

            Location = new Point(FilterBox.LocalBounds.Left, _ProfessionSelector.LocalBounds.Bottom + 5);
            Size = new Point(255, parent.Height - (AbsoluteBounds.Y - 5));

            // BackgroundColor = Color.Magenta;
            Refresh();
            FilterBox.TextChanged += FilterBox_TextChanged;
            _ProfessionSelector.Changed += _ProfessionSelector_Changed;

            BuildsManager.s_moduleInstance.LanguageChanged += ModuleInstance_LanguageChanged;
            BuildsManager.s_moduleInstance.Templates_Loaded += ModuleInstance_Templates_Loaded;
            BuildsManager.s_moduleInstance.Template_Deleted += ModuleInstance_Template_Deleted;
            ContentPanel.ChildAdded += ContentPanel_ChildsChanged;
            ContentPanel.ChildRemoved += ContentPanel_ChildsChanged;
        }

        private void _ProfessionSelector_Changed(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void ModuleInstance_Template_Deleted(object sender, EventArgs e)
        {
            Refresh();
        }

        public void SetSelection()
        {
            Blish_HUD.Gw2Mumble.PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;
            _ProfessionSelector.Professions.Clear();
            _ProfessionSelector.Professions.Add(BuildsManager.s_moduleInstance.Data.Professions.Find(e => e.Id == player.Profession.ToString()));
            RefreshList();
        }

        private void ModuleInstance_LanguageChanged(object sender, EventArgs e)
        {
            FilterBox.PlaceholderText = Strings.common.Search + " ...";
        }

        private void ContentPanel_ChildsChanged(object sender, ChildChangedEventArgs e)
        {
            ResizeChilds = true;
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (ResizeChilds)
            {
                bool not_fitting = ContentPanel.Height < (Templates.Where(e => e.Visible).ToList().Count * 38);
                foreach (Control_TemplateEntry template in Templates)
                {
                    template.Width = not_fitting ? Width - 20 : Width - 5;
                }

                ResizeChilds = false;
            }
        }

        private void ModuleInstance_Templates_Loaded(object sender, EventArgs e)
        {
            Refresh();
        }

        protected override void OnMoved(MovedEventArgs e)
        {
            base.OnMoved(e);

        }

        public void RefreshList()
        {
            ContentPanel.SuspendLayout();
            string filter = FilterBox.Text.ToLower();
            API.Profession prof = BuildsManager.s_moduleInstance.CurrentProfession;

            ContentPanel.SortChildren<Control_TemplateEntry>((a, b) =>
            {
                int ret = (b.Template.Build.Profession == prof).CompareTo(a.Template.Build.Profession == prof);
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

            foreach (Control_TemplateEntry template in Templates)
            {
                if (template.Template != null)
                {
                    string name = template.Template.Name.ToLower();

                    if ((_ProfessionSelector.Professions.Count == 0 || _ProfessionSelector.Professions.Contains(template.Template.Profession)) && name.Contains(filter))
                    {
                        template.Show();
                    }
                    else
                    {
                        template.Hide();
                    }
                }
            }

            ResizeChilds = true;
            ContentPanel.Invalidate();
            ContentPanel.ResumeLayout();
        }

        private void FilterBox_TextChanged(object sender, EventArgs e)
        {
            RefreshList();
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
            FilterBox.Width = Width - 5;
            _ProfessionSelector.Width = Width - 5;
            ContentPanel.Size = new Point(Width, Height - LocalBounds.Y);
        }

        public void Refresh()
        {
            SuspendLayout();
            ContentPanel.SuspendLayout();

            foreach (Control_TemplateEntry template in new List<Control_TemplateEntry>(Templates))
            {
                if (BuildsManager.s_moduleInstance.Templates.Find(e => e == template.Template) == null)
                {
                    template.Dispose();
                    Templates.Remove(template);
                }
            }

            foreach (Template template in BuildsManager.s_moduleInstance.Templates)
            {
                if (Templates.Find(e => e.Template == template) == null)
                {
                    Control_TemplateEntry ctrl = new(ContentPanel, template) { Size = new Point(Width - 20, 38) };
                    ctrl.TemplateChanged += OnTemplateChangedEvent;
                    Templates.Add(ctrl);

                    template.Deleted += Template_Deleted;
                }
            }

            ResumeLayout();
            ContentPanel.ResumeLayout();

            RefreshList();
        }

        private void Template_Deleted(object sender, EventArgs e)
        {
            Template template = (Template)sender;
            Control_TemplateEntry ctrl = Templates.Find(a => a.Template == template);
            ctrl?.Dispose();
            if (ctrl != null)
            {
                Templates.Remove(ctrl);
            }
        }

        public void Clear()
        {
            foreach (Control_TemplateEntry ctrl in Templates)
            {
                ctrl.Dispose();
            }

            Templates.Clear();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            foreach (Template template in BuildsManager.s_moduleInstance.Templates)
            {
                template.Deleted -= Template_Deleted;
            }

            foreach (Control_TemplateEntry template in Templates)
            {
                template.TemplateChanged -= OnTemplateChangedEvent;
            }

            Templates.DisposeAll();

            ContentPanel?.Dispose();
            FilterBox?.Dispose();
            _ProfessionSelector?.Dispose();

            FilterBox.TextChanged -= FilterBox_TextChanged;
            _ProfessionSelector.Changed -= _ProfessionSelector_Changed;

            BuildsManager.s_moduleInstance.LanguageChanged -= ModuleInstance_LanguageChanged;
            BuildsManager.s_moduleInstance.Templates_Loaded -= ModuleInstance_Templates_Loaded;
            BuildsManager.s_moduleInstance.Template_Deleted -= ModuleInstance_Template_Deleted;
            ContentPanel.ChildAdded -= ContentPanel_ChildsChanged;
            ContentPanel.ChildRemoved -= ContentPanel_ChildsChanged;
        }

        public event EventHandler<TemplateChangedEvent> TemplateChanged;

        private void OnTemplateChangedEvent(object sender, TemplateChangedEvent e)
        {
            TemplateChanged?.Invoke(this, new TemplateChangedEvent(e.Template));
        }
    }
}
