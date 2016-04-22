using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace RegexTester.UserControls
{
    /// <summary>
    /// Extension of RichTextBox for highlighting syntax.
    /// </summary>
    public partial class SyntaxHighlightRichTextBox : RichTextBox
    {
        private SyntaxHighlightRichTextBoxModel model = new SyntaxHighlightRichTextBoxModel();
        private int selectedLeftIndex = -1;
        private int selectedRightIndex = -1;

        /// <summary>
        /// C.tor.
        /// </summary>
        public SyntaxHighlightRichTextBox()
        {
            this.KeyUp += setSyntaxHighlighting;
            this.MouseUp += setSyntaxHighlighting;
            this.GotFocus += setSyntaxHighlighting;
            this.LostFocus += lostFocus;
            this.KeyDown += resetSyntaxHighlighting;
            this.KeyDown += handlePaste;
            this.MouseDown += resetSyntaxHighlighting;
        }

        private void SetIndexColor(int index, Color color)
        {
            this.Select(index, index + 1);
            this.SelectionBackColor = color;
        }

        private string currentText = null;
        private int currentIndex = -1;

        private void lostFocus(object sender, EventArgs e)
        {
            resetSyntaxHighlighting(sender, e);
            currentIndex = -1;
        }

        private void handlePaste(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                this.Paste(DataFormats.GetFormat(DataFormats.Text));
                e.Handled = true;
            }
        }

        [DllImport("user32.dll")]
        public static extern bool LockWindowUpdate(IntPtr hWndLock);

        private void resetSyntaxHighlighting(object sender, EventArgs e)
        {
            // Clear any matching parentheses highlighting
            if (this.SelectionLength == 0)
            {
                LockWindowUpdate(this.Handle);
                int index = this.SelectionStart;
                if (selectedLeftIndex != -1)
                {
                    SetIndexColor(selectedLeftIndex, Color.White);
                    selectedLeftIndex = -1;
                }
                if (selectedRightIndex != -1)
                {
                    SetIndexColor(selectedRightIndex, Color.White);
                    selectedRightIndex = -1;
                }
                this.Select(index, 0);
                LockWindowUpdate(IntPtr.Zero);
            }
        }

        private void setSyntaxHighlighting(object sender, EventArgs e)
        {
            // Check if highlighting parentheses is required.
            if (this.SelectionLength == 0 &&
                (currentIndex != this.SelectionStart || this.Text != currentText))
            {
                LockWindowUpdate(this.Handle);
                currentText = this.Text;
                currentIndex = this.SelectionStart;
                int match = model.GetMatchingParentheses(this.Text, currentIndex);
                if (match != -1)
                {
                    if (currentIndex > match)
                    {
                        selectedLeftIndex = match;
                        selectedRightIndex = currentIndex - 1;
                    }
                    else
                    {
                        selectedLeftIndex = currentIndex;
                        selectedRightIndex = match;
                    }
                    this.Select(selectedLeftIndex, 1);
                    this.SelectionBackColor = Color.Cyan;
                    this.Select(selectedRightIndex, 1);
                    this.SelectionBackColor = Color.Cyan;
                }
                this.Select(currentIndex, 0);
                LockWindowUpdate(IntPtr.Zero);
            }
        }
    }
}
