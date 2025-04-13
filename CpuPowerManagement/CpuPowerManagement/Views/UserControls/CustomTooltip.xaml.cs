using LiveCharts;
using LiveCharts.Wpf;
using System.ComponentModel;
using System.Windows.Controls;

namespace CpuPowerManagement.Views.UserControls
{
  public partial class CustomTooltip : UserControl, IChartTooltip
  {
    public CustomTooltip()
    {
      InitializeComponent();
      DataContext = InnerTooltip;
    }

    public TooltipData Data
    {
      get => InnerTooltip.Data;
      set => InnerTooltip.Data = value;
    }

    public TooltipSelectionMode? SelectionMode
    {
      get => InnerTooltip.SelectionMode;
      set => InnerTooltip.SelectionMode = value;
    }

    public event PropertyChangedEventHandler? PropertyChanged
    {
      add => InnerTooltip.PropertyChanged += value;
      remove => InnerTooltip.PropertyChanged -= value;
    }
  }
}