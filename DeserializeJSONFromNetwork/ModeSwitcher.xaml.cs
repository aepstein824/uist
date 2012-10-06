using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        }

        public class EditModeWrapper
        {
            public EditMode mode
            {
                get
                {
                    return _mode;
                }
                set
                {
                    var prevval = _mode;
                    _mode = value;
                    if (modeSwitcher != null && _mode != prevval)
                    {
                        modeSwitcher.Dispatcher.BeginInvoke(new ThreadStart(() =>
                        {
                            modeSwitcher.setModeCheckbox(_mode);
                        }));
                    }
                }
            }
            private EditMode _mode = EditMode.Add;
            public ModeSwitcher modeSwitcher = null;
        }

        public EditModeWrapper currentMode = new EditModeWrapper();

        public void setModeCheckbox(EditMode mode)
        {
            if (mode == EditMode.Add)
                this.radioButtonAdd.IsChecked = true;
            if (mode == EditMode.Subtract)
                this.radioButtonSubtract.IsChecked = true;
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
