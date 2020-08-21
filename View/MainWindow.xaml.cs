using System;
using HexLoader.ViewModel;

namespace HexLoader.View
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow 
    {
        private MainWindowVm ViewModel;
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = (MainWindowVm)DataContext;
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            ViewModel.CloseProgram();
        }

        private void ComboBox_OnDropDownOpened(object sender, EventArgs e)
        {
            ViewModel.UpdateComPortsSource();
        }
    }
}
