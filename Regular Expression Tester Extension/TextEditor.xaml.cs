using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AndreasAndersen.Regular_Expression_Tester_Extension
{
    /// <summary>
    /// Interaction logic for TextEditor.xaml
    /// </summary>
    public partial class TextEditor : Window
    {
        public TextEditor()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return TextEditorBox.Text; }
            set { TextEditorBox.Text = value; }
        }

        public bool IsEditable
        {
            get { return !TextEditorBox.IsReadOnly; }
            set { 
                TextEditorBox.IsReadOnly = !value;
                Title = IsEditable ? "Edit" : "View";
                TextEditorBox.BorderThickness = IsEditable ? new Thickness(1) : new Thickness(0);
            }
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
