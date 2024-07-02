using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Diagnostics;

namespace ZephyrInnovations
{
    public static class LoggingUtility
    {
        public static Logger Logger = LogManager.GetCurrentClassLogger();//change 'private' to public; removed 'readonly'

        public static void LogTimedAction(Action action, string description)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            Logger.Info($"{description} - Elapsed time: {stopwatch.ElapsedMilliseconds} ms");
        }
        public static async Task LogTimedActionAsync(Func<Task> action, string description)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            await action();
            stopwatch.Stop();
            Logger.Info($"{description} - Elapsed time (Async): {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}
