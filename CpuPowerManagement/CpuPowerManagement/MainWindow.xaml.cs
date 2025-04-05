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

    private void SliderPl1TimeWindowSec_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      var slider = sender as Slider;
      if (slider == null) return;

      if(slider.Value == 0)
        return;

      double closest = ViewModel.ValidTimeStepsDouble.OrderBy(x => Math.Abs(x - slider.Value)).FirstOrDefault();
      slider.Value = closest;
    }
  }
}