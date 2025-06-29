﻿using System.Windows;
using System.Windows.Controls;

namespace CpuPowerManagement.Components
{
  public class DiscreteValueSlider : Slider
  {
    static DiscreteValueSlider()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(DiscreteValueSlider),
          new FrameworkPropertyMetadata(typeof(DiscreteValueSlider)));
    }

    public double[] AllowedValues
    {
      get => (double[])GetValue(AllowedValuesProperty);
      set => SetValue(AllowedValuesProperty, value);
    }

    public static readonly DependencyProperty AllowedValuesProperty =
        DependencyProperty.Register("AllowedValues", typeof(double[]), typeof(DiscreteValueSlider),
            new PropertyMetadata(Array.Empty<double>(), OnAllowedValuesChanged));

    private static void OnAllowedValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is not DiscreteValueSlider slider || slider.AllowedValues == null || slider.AllowedValues.Length <= 0) return;

      slider.Minimum = 0;
      slider.Maximum = slider.AllowedValues.Length - 1;
      slider.TickFrequency = 1;
      slider.IsSnapToTickEnabled = true;
    }

    protected override void OnValueChanged(double oldValue, double newValue)
    {
      base.OnValueChanged(oldValue, newValue);

      if (AllowedValues == null || AllowedValues.Length == 0) return;

      var index = (int)Math.Round(Value);
      index = Math.Max(0, Math.Min(AllowedValues.Length - 1, index));

      var snappedValue = AllowedValues[index];

      // Update the actual value being represented by the slider
      SelectedRealValue = snappedValue;

      // Optionally, log or debug to check for updates
      Console.WriteLine($"Slider Value Changed: {snappedValue}");
    }

    public double MinimumValue
    {
      get => AllowedValues.Min();
    }

    public double MaximumValue
    {
      get => AllowedValues.Max();
    }

    public double SelectedRealValue
    {
      get => (double)GetValue(SelectedRealValueProperty);
      set => SetValue(SelectedRealValueProperty, value);
    }

    public static readonly DependencyProperty SelectedRealValueProperty =
        DependencyProperty.Register("SelectedRealValue", typeof(double), typeof(DiscreteValueSlider), new PropertyMetadata(0.0, OnSelectedRealValueChanged));

    // Ensure that the slider reacts when the value changes programmatically
    private static void OnSelectedRealValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is not DiscreteValueSlider slider || slider.AllowedValues == null || slider.AllowedValues.Length == 0) return;

      // Manually trigger the value change logic for the slider
      var snappedValue = (double)e.NewValue;
      var index = Array.IndexOf(slider.AllowedValues, snappedValue);
      if (index != -1)
      {
        slider.Value = index; // Set the slider value
      }
    }
  }


}
