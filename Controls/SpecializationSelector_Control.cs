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

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class SpecializationSelector_Control : Control
    {
        public Specialization_Control Specialization_Control;
        public int Index;
        public bool Elite;
        private API.Specialization _Specialization;

        public API.Specialization Specialization
        {
            get => _Specialization;
            set
            {
                if (value != null)
                {
                    _Specialization = value;
                }
            }
        }

        public Template Template => BuildsManager.s_moduleInstance.Selected_Template;

        public SpecializationSelector_Control()
        {
            BuildsManager.s_moduleInstance.Selected_Template_Changed += ClosePopUp;
            Input.Mouse.LeftMouseButtonPressed += ClosePopUp;
        }

        private void ClosePopUp(object sender, EventArgs e)
        {
            if (!MouseOver)
            {
                Hide();
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            int i = 0;
            int size = 64;
            if (Template.Build.Profession != null)
            {
                foreach (API.Specialization spec in Template.Build.Profession.Specializations)
                {
                    if (!spec.Elite || Elite)
                    {
                        Rectangle rect = new(20 + (i * size), (Height - size) / 2, size, size);

                        if (rect.Contains(RelativeMousePosition))
                        {
                            SpecLine sp = Template.Build.SpecLines.Find(x => x.Specialization != null && x.Specialization.Id == spec.Id);

                            if (sp != null && sp != Template.Build.SpecLines[Index])
                            {
                                // var traits = new List<API.Trait>(sp.Traits);
                                Template.Build.SpecLines[Index].Specialization = sp.Specialization;
                                Template.Build.SpecLines[Index].Traits = sp.Traits;

                                // Template.Build.SpecLines[Index].Control.UpdateLayout();
                                Template.SetChanged();

                                sp.Specialization = null;
                                sp.Traits = new List<API.Trait>();
                            }
                            else
                            {
                                if (Template.Build.SpecLines[Index] != null)
                                {
                                    foreach (SpecLine specLine in Template.Build.SpecLines)
                                    {
                                        if (spec != Specialization && specLine.Specialization == spec)
                                        {
                                            specLine.Specialization = null;
                                            specLine.Traits = new List<API.Trait>();
                                        }
                                    }

                                    if (Template.Build.SpecLines[Index].Specialization != spec)
                                    {
                                        Template.Build.SpecLines[Index].Specialization = spec;
                                        Template.SetChanged();
                                    }
                                }
                            }

                            Hide();
                            return;
                        }

                        i++;
                    }
                }
            }

            Hide();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(
                Parent,
                ContentService.Textures.Pixel,
                bounds.Add(Location),
                bounds,
                new Color(0, 0, 0, 205),
                0f,
                Vector2.Zero);

            if (Template.Build.Profession != null)
            {
                string text = string.Empty;

                int i = 0;
                int size = 64;
                foreach (API.Specialization spec in Template.Build.Profession.Specializations)
                {
                    if (!spec.Elite || Elite)
                    {
                        Rectangle rect = new(20 + (i * size), (Height - size) / 2, size, size);
                        if (rect.Contains(RelativeMousePosition))
                        {
                            text = spec.Name;
                        }

                        spriteBatch.DrawOnCtrl(
                            Parent,
                            spec.Icon._AsyncTexture,
                            rect.Add(Location),
                            spec.Icon._AsyncTexture.Texture.Bounds,
                            Specialization == spec || rect.Contains(RelativeMousePosition) ? Color.White : Color.Gray,
                            0f,
                            Vector2.Zero);
                        i++;
                    }
                }

                if (text != string.Empty)
                {
                    BasicTooltipText = text;
                }
                else
                {
                    BasicTooltipText = null;
                }
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            BuildsManager.s_moduleInstance.Selected_Template_Changed -= ClosePopUp;
            Input.Mouse.LeftMouseButtonPressed -= ClosePopUp;
        }
    }
}