using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace LiterateChainsaw.Converters
{
    public class ConditionMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Any(value => value == DependencyProperty.UnsetValue || value == null))
            {
                return FallbackValue; // Return fallback value when any source value is missing
            }

            bool IsConditionOn = (bool)values[0];
            bool ConditionA = (bool)values[1];
            bool ConditionB = (bool)values[2];
            bool IsUnitInValid = (bool)values[3];


            if (IsUnitInValid)
            {
                return 6; //Invalid
            }

            if (IsConditionOn && ConditionA && !ConditionB)
            {
                return 1; // Condition 1
            }
            else if (IsConditionOn && ConditionA && ConditionB)
            {
                return 2; // Condition 2
            }
            else if (IsConditionOn && !ConditionA && ConditionB)
            {
                return 3; // Condition 3
            }
            else if (IsConditionOn && !ConditionA && !ConditionB)
            {
                return 4; // Condition 4
            }
            else
            {
                return 5; //No checking
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public object FallbackValue { get; set; }
    }
}
