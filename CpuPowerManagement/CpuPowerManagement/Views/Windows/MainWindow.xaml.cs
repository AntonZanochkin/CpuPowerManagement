using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CpuPowerManagement.Intel.MSR;
using CpuPowerManagement.ViewModels.Windows;

namespace CpuPowerManagement.Views.Windows
{
  public partial class MainWindow : Window
  {
    private MainWindowViewModel WindowViewModel => (MainWindowViewModel)DataContext;
    public MainWindow()
    {
      InitializeComponent();
    }

    private void SizeSlider_TouchDown(object? sender, TouchEventArgs e)
    {

    }
  }
}