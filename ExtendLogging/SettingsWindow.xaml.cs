using System.Windows;

namespace ExtendLogging
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private PluginSettings PSettings { get; }

        public SettingsWindow(PluginSettings pSettings)
        {
            PSettings = pSettings;
            InitializeComponent();
            this.DataContext = pSettings;
        }

        internal void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ShowLevelBox.Focus();
            e.Cancel = true;
            Hide();
        }

        private void LevelShieldTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ShowLevelBox.Focus();
            LevelShieldTextBox.Focus();
        }
    }
}
