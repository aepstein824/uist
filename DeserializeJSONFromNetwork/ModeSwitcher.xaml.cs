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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeserializeJSONFromNetwork
{
    /// <summary>
    /// Interaction logic for ModeSwitcher.xaml
    /// </summary>
    public partial class ModeSwitcher : Window
    {
        public ModeSwitcher()
        {
            InitializeComponent();
        }

        public enum EditMode
        {
            Add,
            Subtract,
            Navigate,
        }

        public class EditModeWrapper
        {
            public EditMode mode = EditMode.Add;
        }

        public EditModeWrapper currentMode = new EditModeWrapper();

        private void radioButtonNavigate_Checked(object sender, RoutedEventArgs e)
        {
            currentMode.mode = EditMode.Navigate;
        }

        private void radioButtonSubtract_Checked(object sender, RoutedEventArgs e)
        {
            currentMode.mode = EditMode.Subtract;
        }

        private void radioButtonAdd_Checked(object sender, RoutedEventArgs e)
        {
            currentMode.mode = EditMode.Add;
        }
    }
}
