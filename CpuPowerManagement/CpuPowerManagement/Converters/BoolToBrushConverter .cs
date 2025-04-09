using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CpuPowerManagement.Converters
{
  public class BoolToBrushConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      bool isOk = value is bool b && !b; // false means OK (not throttling)
      return isOk ? Brushes.Green : Brushes.Red;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
