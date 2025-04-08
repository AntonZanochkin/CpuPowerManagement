using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace CpuPowerManagement.Views.UserControls
{
  /// <summary>
  /// Interaction logic for ThermalControl.xaml
  /// </summary>
  public partial class ThermalControl : UserControl
  {
    public ThermalControl()
    {
      InitializeComponent();

      if (!DesignerProperties.GetIsInDesignMode(this))
        MainCardExpander.IsExpanded = false;
    }
  }
}
