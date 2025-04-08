using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CpuPowerManagement.Views.UserControls
{
  public partial class PowerLimitControl : UserControl
  {
    public PowerLimitControl()
    {
      InitializeComponent();

      if (!DesignerProperties.GetIsInDesignMode(this))
        MainCardExpander.IsExpanded = false;
    }
  }
}
