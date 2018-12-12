using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ExtendLogging
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            ShowLevelBox.SetBinding(CheckBox.IsCheckedProperty, new Binding("LogLevel") { Mode = BindingMode.TwoWay, Source = MainProgram.Instance });
            ShowMedalBox.SetBinding(CheckBox.IsCheckedProperty, new Binding("LogMedal") { Mode = BindingMode.TwoWay, Source = MainProgram.Instance });
            ShowTitleBox.SetBinding(CheckBox.IsCheckedProperty, new Binding("LogTitle") { Mode = BindingMode.TwoWay, Source = MainProgram.Instance });
            ShowExternBox.SetBinding(CheckBox.IsCheckedProperty, new Binding("LogExternInfo") { Mode = BindingMode.TwoWay, Source = MainProgram.Instance });
        }

        internal void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
