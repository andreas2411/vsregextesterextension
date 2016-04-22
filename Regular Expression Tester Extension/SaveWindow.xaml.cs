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
    /// Interaction logic for SaveRegexWindow.xaml
    /// </summary>
    public partial class SaveWindow : Window
    {
        public SaveWindow(string title)
        {
            InitializeComponent();
            this.Title = title;
        }

        public new string Name
        {
            get { return textBoxName.Text; }
            set { textBoxName.Text = value; }
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.textBoxName.Focus();
        }
    }
}
