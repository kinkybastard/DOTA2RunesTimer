using MahApps.Metro.Controls;
using System.Windows.Input;

namespace DOTA2RunesTimer
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            // Ссылка на файл с логикой таймера (и передаем ему данные главного окна)
            TimerLogic timerLogic = new(this);
            this.DataContext = timerLogic;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}