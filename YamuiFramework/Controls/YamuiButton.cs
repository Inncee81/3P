﻿using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using YamuiFramework.Fonts;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {
    [Designer("YamuiFramework.Controls.YamuiButtonDesigner")]
    [ToolboxBitmap(typeof (Button))]
    [DefaultEvent("ButtonPressed")]
    public class YamuiButton : Button {

        #region Fields

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseCustomBackColor { get; set; }

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool UseCustomForeColor { get; set; }

        [DefaultValue(false)]
        [Category("Yamui")]
        public bool Highlight { get; set; }

        private event EventHandler<ButtonPressedEventArgs> OnButtonPressed;

        /// <summary>
        /// You should register to this event to know when the button has been pressed (clicked or enter or space)
        /// </summary>
        [Category("Yamui")]
        public event EventHandler<ButtonPressedEventArgs> ButtonPressed {
            add { OnButtonPressed += value; }
            remove { OnButtonPressed -= value; }
        }

        /// <summary>
        /// This public prop is only defined so we can set it from the transitions (animation component)
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DoPressed {
            get { return IsPressed; }
            set {
                IsPressed = value;
                Invalidate();
            }
        }

        public bool IsHovered;
        public bool IsPressed;
        public bool IsFocused;

        #endregion

        #region Constructor

        public YamuiButton() {
            // why those styles? check here: https://sites.google.com/site/craigandera/craigs-stuff/windows-forms/flicker-free-control-drawing
            SetStyle(
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.Selectable |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Opaque, true);
        }

        #endregion

        #region Overridden Methods

        protected override void OnEnabledChanged(EventArgs e) {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        #endregion

        #region Paint Methods

        protected void PaintTransparentBackground(Graphics graphics, Rectangle clipRect) {
            graphics.Clear(Color.Transparent);
            if ((Parent != null)) {
                clipRect.Offset(Location);
                var e = new PaintEventArgs(graphics, clipRect);
                var state = graphics.Save();
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
                try {
                    graphics.TranslateTransform(-Location.X, -Location.Y);
                    InvokePaintBackground(Parent, e);
                    InvokePaint(Parent, e);
                } finally {
                    graphics.Restore(state);
                    clipRect.Offset(-Location.X, -Location.Y);
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e) {}

        protected virtual void CustomOnPaintBackground(PaintEventArgs e) {
            var backColor = ThemeManager.ButtonColors.BackGround(BackColor, UseCustomBackColor, IsFocused, IsHovered, IsPressed, Enabled);
            if (backColor != Color.Transparent)
                e.Graphics.Clear(backColor);
            else
                PaintTransparentBackground(e.Graphics, DisplayRectangle);
        }

        protected override void OnPaint(PaintEventArgs e) {
            try {
                CustomOnPaintBackground(e);
                OnPaintForeground(e);
            } catch {
                Invalidate();
            }
        }

        protected virtual void OnPaintForeground(PaintEventArgs e) {
            var borderColor = ThemeManager.ButtonColors.BorderColor(IsFocused, IsHovered, IsPressed, Enabled);
            var foreColor = ThemeManager.ButtonColors.ForeGround(ForeColor, UseCustomForeColor, IsFocused, IsHovered, IsPressed, Enabled);

            if (borderColor != Color.Transparent)
                using (var p = new Pen(borderColor)) {
                    var borderRect = new Rectangle(0, 0, Width - 1, Height - 1);
                    e.Graphics.DrawRectangle(p, borderRect);
                }

            // highlight is a border with more width
            if (Highlight && !IsHovered && !IsPressed && Enabled) {
                using (var p = new Pen(ThemeManager.AccentColor, 4)) {
                    var borderRect = new Rectangle(2, 2, Width - 4, Height - 4);
                    e.Graphics.DrawRectangle(p, borderRect);
                }
            }

            TextRenderer.DrawText(e.Graphics, Text, FontManager.GetStandardFont(), ClientRectangle, foreColor, FontManager.GetTextFormatFlags(TextAlign));
        }

        private void HandlePressedButton() {
            if (OnButtonPressed != null) {
                var args = new ButtonPressedEventArgs(Tag);
                OnButtonPressed(this, args);
            }
        }

        #endregion

        #region Managing isHovered, isPressed, isFocused

        #region Focus Methods

        protected override void OnGotFocus(EventArgs e) {
            IsFocused = true;
            Invalidate();

            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e) {
            IsFocused = false;
            Invalidate();

            base.OnLostFocus(e);
        }

        protected override void OnEnter(EventArgs e) {
            IsFocused = true;
            Invalidate();

            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e) {
            IsFocused = false;
            Invalidate();

            base.OnLeave(e);
        }

        #endregion

        #region Keyboard Methods

        // This is mandatory to be able to handle the ENTER key in key events!!
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e) {
            if (e.KeyCode == Keys.Enter) e.IsInputKey = true;
            base.OnPreviewKeyDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            e.Handled = true;
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter) {
                HandlePressedButton();
                IsPressed = true;
                Invalidate();
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            //Remove this code cause this prevents the focus color
            IsPressed = false;
            Invalidate();
            base.OnKeyUp(e);
        }

        #endregion

        #region Mouse Methods

        protected override void OnMouseEnter(EventArgs e) {
            IsHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                HandlePressedButton();
                IsPressed = true;
                Invalidate();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            IsPressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(EventArgs e) {
            IsPressed = false;
            IsHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        #endregion

        #endregion
    }

    public sealed class ButtonPressedEventArgs : EventArgs {
        public readonly object ButtonTag;

        public ButtonPressedEventArgs(object buttonTag) {
            ButtonTag = buttonTag;
        }
    }

    internal class YamuiButtonDesigner : ControlDesigner {
        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("ImeMode");
            properties.Remove("Padding");
            properties.Remove("FlatAppearance");
            properties.Remove("FlatStyle");
            properties.Remove("AutoEllipsis");
            properties.Remove("UseCompatibleTextRendering");
            properties.Remove("Image");
            properties.Remove("ImageAlign");
            properties.Remove("ImageIndex");
            properties.Remove("ImageKey");
            properties.Remove("ImageList");
            properties.Remove("TextImageRelation");
            properties.Remove("UseVisualStyleBackColor");
            properties.Remove("Font");
            properties.Remove("RightToLeft");
            base.PreFilterProperties(properties);
        }
    }
}