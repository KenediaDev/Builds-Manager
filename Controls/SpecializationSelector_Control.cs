namespace Kenedia.Modules.BuildsManager.Controls
{
    using System;
    using System.Collections.Generic;
    using Blish_HUD;
    using Blish_HUD.Controls;
    using Blish_HUD.Input;
    using Kenedia.Modules.BuildsManager.Extensions;
    using Kenedia.Modules.BuildsManager.Models;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Color = Microsoft.Xna.Framework.Color;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class SpecializationSelector_Control : Control
    {
        public Specialization_Control Specialization_Control;
        public int Index;
        public bool Elite;
        private API.Specialization _Specialization;

        public API.Specialization Specialization
        {
            get => this._Specialization;
            set
            {
                if (value != null)
                {
                    this._Specialization = value;
                }
            }
        }

        public Template Template
        {
            get => BuildsManager.ModuleInstance.Selected_Template;
        }

        public SpecializationSelector_Control()
        {
            BuildsManager.ModuleInstance.Selected_Template_Changed += this.ClosePopUp;
            Input.Mouse.LeftMouseButtonPressed += this.ClosePopUp;
        }

        private void ClosePopUp(object sender, EventArgs e)
        {
            if (!this.MouseOver)
            {
                this.Hide();
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var i = 0;
            var size = 64;
            if (this.Template.Build.Profession != null)
            {
                foreach (API.Specialization spec in this.Template.Build.Profession.Specializations)
                {
                    if (!spec.Elite || this.Elite)
                    {
                        var rect = new Rectangle(20 + (i * size), (this.Height - size) / 2, size, size);

                        if (rect.Contains(this.RelativeMousePosition))
                        {
                            var sp = this.Template.Build.SpecLines.Find(x => x.Specialization != null && x.Specialization.Id == spec.Id);

                            if (sp != null && sp != this.Template.Build.SpecLines[this.Index])
                            {
                                // var traits = new List<API.Trait>(sp.Traits);
                                this.Template.Build.SpecLines[this.Index].Specialization = sp.Specialization;
                                this.Template.Build.SpecLines[this.Index].Traits = sp.Traits;

                                // Template.Build.SpecLines[Index].Control.UpdateLayout();
                                this.Template.SetChanged();

                                sp.Specialization = null;
                                sp.Traits = new List<API.Trait>();
                            }
                            else
                            {
                                if (this.Template.Build.SpecLines[this.Index] != null)
                                {
                                    foreach (SpecLine specLine in this.Template.Build.SpecLines)
                                    {
                                        if (spec != this.Specialization && specLine.Specialization == spec)
                                        {
                                            specLine.Specialization = null;
                                            specLine.Traits = new List<API.Trait>();
                                        }
                                    }

                                    if (this.Template.Build.SpecLines[this.Index].Specialization != spec)
                                    {
                                        this.Template.Build.SpecLines[this.Index].Specialization = spec;
                                        this.Template.SetChanged();
                                    }
                                }
                            }

                            this.Hide();
                            return;
                        }

                        i++;
                    }
                }
            }

            this.Hide();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(
                this.Parent,
                ContentService.Textures.Pixel,
                bounds.Add(this.Location),
                bounds,
                new Color(0, 0, 0, 205),
                0f,
                Vector2.Zero);

            if (this.Template.Build.Profession != null)
            {
                string text = string.Empty;

                var i = 0;
                var size = 64;
                foreach (API.Specialization spec in this.Template.Build.Profession.Specializations)
                {
                    if (!spec.Elite || this.Elite)
                    {
                        var rect = new Rectangle(20 + (i * size), (this.Height - size) / 2, size, size);
                        if (rect.Contains(this.RelativeMousePosition))
                        {
                            text = spec.Name;
                        }

                        spriteBatch.DrawOnCtrl(
                            this.Parent,
                            spec.Icon._AsyncTexture,
                            rect.Add(this.Location),
                            spec.Icon._AsyncTexture.Texture.Bounds,
                            this.Specialization == spec || rect.Contains(this.RelativeMousePosition) ? Color.White : Color.Gray,
                            0f,
                            Vector2.Zero);
                        i++;
                    }
                }

                if (text != string.Empty)
                {
                    this.BasicTooltipText = text;
                }
                else
                {
                    this.BasicTooltipText = null;
                }
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            BuildsManager.ModuleInstance.Selected_Template_Changed -= this.ClosePopUp;
            Input.Mouse.LeftMouseButtonPressed -= this.ClosePopUp;
        }
    }
}