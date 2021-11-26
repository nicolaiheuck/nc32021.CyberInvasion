using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace CyberInvasion.OwnVersion
{
    public partial class Config : Window, IComponentConnector
    {
        public Config() => InitializeComponent();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow(ServerIpFromUser.Text).Show();
            Close();
        }
        void Connect(int connectionId, object target)
        {
            if (connectionId != 1)
            {
                if (connectionId == 2)
                    ((ButtonBase)target).Click += new RoutedEventHandler(Button_Click);
                else
                    _contentLoaded = true;
            }
            else
                ServerIpFromUser = (TextBox)target;
        }
    }
}
