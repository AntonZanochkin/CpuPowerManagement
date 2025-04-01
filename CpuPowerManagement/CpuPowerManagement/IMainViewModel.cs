using System.Windows.Input;

namespace CpuPowerManagement
{
  public interface IMainViewModel
  {
    ICommand SaveCommand { get; }
  }
}
