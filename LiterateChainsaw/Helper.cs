using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiterateChainsaw
{
    public static class Helper
    {
        /// <summary>
        /// int result = Helper.ConvertStringTo<int>("123");
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static T ConvertStringTo<T>(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException(nameof(input), "Input string cannot be null or empty.");
            }

            Type targetType = typeof(T);

            try
            {
                if (targetType.IsEnum)
                {
                    return (T)Enum.Parse(targetType, input);
                }
                if (targetType == typeof(Guid))
                {
                    return (T)(object)Guid.Parse(input);
                }
                if (targetType == typeof(DateTime))
                {
                    return (T)(object)DateTime.Parse(input, CultureInfo.InvariantCulture);
                }
                if (targetType == typeof(TimeSpan))
                {
                    return (T)(object)TimeSpan.Parse(input, CultureInfo.InvariantCulture);
                }
                if (targetType == typeof(bool))
                {
                    return (T)(object)bool.Parse(input);
                }

                // For types that implement IConvertible (int, double, etc.)
                return (T)Convert.ChangeType(input, targetType, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to convert '{input}' to {targetType.Name}.", ex);

                if (targetType == typeof(bool))
                {
                    return (T)(object)false;
                }
            }
        }

        /// <summary>
        /// Helper.TimeAction(() => SomeMethod(), "SomeMethod Execution");
        /// </summary>
        /// <param name="action"></param>
        /// <param name="description"></param>
        public static void TimeAction(Action action, string description)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
        }
    }
}
