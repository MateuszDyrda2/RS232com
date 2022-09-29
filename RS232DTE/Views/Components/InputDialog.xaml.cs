using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RS232DTE.Views.Components
{
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        public InputDialog()
        {
            InitializeComponent();
        }

        public string ResponseText
        {
            get => Response.Text;
            set => Response.Text = value;
        }

        public string Text
        {
            get => TextField.Text;
            set => TextField.Text = value;
        }

        private void OKButton_click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
        private void CancelButton_click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

    }
}
