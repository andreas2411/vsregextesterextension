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
    public partial class MessageDecorator : UserControl
    {
        private Control wrappedControl;

        public MessageDecorator()
        {
            InitializeComponent();
        }

        private void ConfigureControl()
        {
            wrappedControl.Dock = DockStyle.Fill;
            panelWrappedControl.Controls.Add(wrappedControl);
        }

        public Control WrappedControl { get { return wrappedControl; } set { wrappedControl = value; ConfigureControl(); } }

        public void SetMessage(string message)
        {
            this.textBoxMessage.Text = message;
        }

        public void ResetMessage()
        {
            this.textBoxMessage.Text = "";
        }
    }
}
