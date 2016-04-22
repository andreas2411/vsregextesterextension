using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RegexTester.UserControls
{
    /// <summary>
    /// Control wrapping other control to allow it to show validity.
    /// A border is put around the wrapped control, showing green if valid
    /// and red otherwise.
    /// </summary>
    public partial class ShowValidDecorator : UserControl
    {
        private Color ColorValid = Color.Green;
        private Color ColorInvalid = Color.Red;
        private bool isValid = true;
        private int borderWidth = 1;
        private Control wrappedControl;

        public ShowValidDecorator()
        {
            this.SizeChanged += WrappedControlResized;
        }

        public ShowValidDecorator(UserControl wrappedControl)
            : base()
        {
            this.WrappedControl = wrappedControl;
            this.SizeChanged += WrappedControlResized;
            SetProperties();
        }

        private void SetProperties()
        {
            if (WrappedControl != null)
            {
                WrappedControl.Location = new Point(borderWidth, borderWidth);
                WrappedControl.Height = Height - 2 * borderWidth;
                WrappedControl.Width = Width - 2 * borderWidth;
            }
        }

        private void WrappedControlResized(object sender, EventArgs e)
        {
            SetProperties();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            Color color = IsValid ? ColorValid : ColorInvalid;
            Brush brush = new SolidBrush(color);
            g.FillRectangle(brush, 0, 0, this.Width, borderWidth);
            g.FillRectangle(brush, 0, 0, borderWidth, this.Height);
            g.FillRectangle(brush, 0, this.Height - borderWidth, this.Width, borderWidth);
            g.FillRectangle(brush, this.Width - borderWidth, 0, borderWidth, this.Height);
        }

        public bool IsValid
        {
            get { return isValid; }
            set { isValid = value; Invalidate(); }
        }

        public int BorderWidth
        {
            get { return borderWidth; }
            set { borderWidth = value; SetProperties(); }
        }

        public Control WrappedControl
        {
            get { return wrappedControl; }
            set
            {
                wrappedControl = value;
                this.Controls.Clear();
                this.Controls.Add(wrappedControl);
                SetProperties();
            }
        }
    }
}
