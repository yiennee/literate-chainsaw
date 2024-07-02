using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZephyrInnovations;

namespace LiterateChainsaw
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            int length = 20;
            for (int i = 0; i < length; i++)
            {
                Console.WriteLine($"Enter {i + 1}");

                LoggingUtility.LogTimedAction(() => SomeMethod($"Index={i + 1}"), "SomeMethod");
                Console.WriteLine($"Exit {i + 1}\n");
            }

            LoggingUtility.Logger.Info($"----------------------");
            ExecuteMethodsAsync();
        }


        //[TimingAspect]
        private static void SomeMethod(string print)
        {
            LoggingUtility.Logger.Info($"{print} ");
            Thread.Sleep(100);
            LoggingUtility.Logger.Info($"{print} ////");
        }

        public static void AnotherMethod(string print)
        {
            LoggingUtility.Logger.Info($"{print} ");
            // Simulate work
            System.Threading.Thread.Sleep(300);
            LoggingUtility.Logger.Info($"{print} ////");
        }

        public static async Task SomeMethodAsync(string print)
        {
            LoggingUtility.Logger.Info($"{print} ");
            // Simulate work
            await Task.Delay(500);
            LoggingUtility.Logger.Info($"{print} ////");
        }

        public static async Task AnotherMethodAsync(string print)
        {
            LoggingUtility.Logger.Info($"{print} ");
            // Simulate work
            await Task.Delay(300);
            LoggingUtility.Logger.Info($"{print} ////");
        }

        public static async Task ExecuteMethodsAsync()
        {
            var tasks = new Task[]
            {
                Task.Run(() => LoggingUtility.LogTimedAction(() => SomeMethod("SomeMethod"), "SomeMethod Execution")),
                Task.Run(() => LoggingUtility.LogTimedAction(() => AnotherMethod("AnotherMethod"), "AnotherMethod Execution")),
                Task.Run(() => LoggingUtility.LogTimedActionAsync(async () => await SomeMethodAsync("SomeMethodAsync"), "SomeMethodAsync Execution")),
                Task.Run(() => LoggingUtility.LogTimedActionAsync(async () => await AnotherMethodAsync("AnotherMethodAsync"), "AnotherMethodAsync Execution"))
            };

            await Task.WhenAll(tasks);
        }

    }
}
