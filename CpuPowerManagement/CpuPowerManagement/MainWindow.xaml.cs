using System.Text;
using System.Windows;
using System.Windows.Controls;
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

    private void SliderTimeWindowSec_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      var slider = sender as Slider;
      if (slider == null) return;

      var closest = ViewModel.ValidTimeSteps.OrderBy(x => Math.Abs(x - slider.Value)).FirstOrDefault();
      slider.Value = closest;
    }
  }
}