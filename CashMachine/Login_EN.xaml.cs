using System.Windows;

namespace CashMachine
{
    /// <summary>
    /// Login_EN.xaml 的交互逻辑
    /// </summary>
    public partial class Login_EN : Window
    {
        public Login_EN()
        {
            InitializeComponent();
            this.ResizeMode = ResizeMode.CanMinimize;//禁用“最大化”按钮
        }
        /// <summary>
        /// 本地登录
        /// </summary>
        private void LoginLocal_Click(object sender, RoutedEventArgs e)
        {
            MainWindow_Local_EN mainWindow_Local_en = new MainWindow_Local_EN();
            mainWindow_Local_en.Show();
            this.Close();
        }
    }
}
