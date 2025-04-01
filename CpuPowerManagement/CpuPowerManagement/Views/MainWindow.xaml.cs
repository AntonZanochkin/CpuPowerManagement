using System.Windows;

namespace CpuPowerManagement.Views
{
  public partial class MainWindow : Window
  {
    public MainWindow(IMainViewModel viewModel)
    {
      InitializeComponent();
      DataContext = viewModel;
    }
  }
}