using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using DataFormats = System.Windows.Forms.DataFormats;
using Forms = System.Windows.Forms;

namespace AndreasAndersen.Regular_Expression_Tester_Extension
{
    /// <summary>
    /// Interaction logic for RichTextEditor.xaml
    /// </summary>
    public partial class RichTextEditor : Window
    {
        private Forms.RichTextBox richTextEditorBox;

        public RichTextEditor()
        {
            InitializeComponent();
            richTextEditorBox = new Forms.RichTextBox();
            richTextEditorBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            Color background = (this.Background as SolidColorBrush).Color;
            richTextEditorBox.BackColor = System.Drawing.Color.FromArgb(background.R, background.G, background.B);
            richTextEditorBox.KeyDown += PasteHandler;
            RichTextHost.Child = richTextEditorBox;
        }

        private void PasteHandler(object sender, Forms.KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                richTextEditorBox.Paste(DataFormats.GetFormat(DataFormats.Text));
                e.Handled = true;
            }
        }

        public string Text
        {
            get { return richTextEditorBox.Rtf; }
            set { richTextEditorBox.Rtf = value; }
        }

        public bool IsEditable
        {
            get { return !richTextEditorBox.ReadOnly; }
            set { 
                richTextEditorBox.ReadOnly = !value;
                Title = IsEditable ? "Edit" : "View";
                richTextEditorBox.BorderStyle = IsEditable ? Forms.BorderStyle.Fixed3D : Forms.BorderStyle.None;
            }
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }


    }
}
