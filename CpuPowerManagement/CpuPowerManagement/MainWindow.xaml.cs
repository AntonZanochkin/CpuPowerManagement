using System.Text;
using System.Windows;
using System.Windows.Input;
using CpuPowerManagement.ViewModels;

namespace CpuPowerManagement
{
  public partial class MainWindow : Window
  {
    private MainViewModel ViewModel => (MainViewModel)DataContext;
    public MainWindow()
    {
      InitializeComponent();


    }

    private void SizeSlider_TouchDown(object? sender, TouchEventArgs e)
    {

    }
  }
}