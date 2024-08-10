using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace LiterateChainsaw.Converters
{
    public class YieldConditionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            if (values.Any(value => value == DependencyProperty.UnsetValue || value == null))
            {
                return FallbackValue; // Return fallback value when any source value is missing
            }

            bool IsColorbyCondition = true;
            string VisionEngineState = values[0].ToString();
            switch (VisionEngineState)
            {
                case "READY":
                    {
                        return "Default";
                        IsColorbyCondition = false;
                        break;
                    }
                case "PRODUCTION":
                    {
                        IsColorbyCondition = true;
                        break;
                    }
                default:
                case "ERROR":
                    {
                        return "Error";
                    }
            }

            //if (!IsColorbyCondition)

            double threshold;
            if (!double.TryParse(values[2].ToString(), out threshold))
                return "Error";

            double inputValue;
            if (!double.TryParse(values[1].ToString(), out inputValue))
                return "Error";
            //threshold = inputValue;
            if (threshold > inputValue)
                return "Alarm";
            else
                return "Production";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object FallbackValue { get; set; }
    }
}
