using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZephyrInnovations
{
    /// <summary>
    /// [TimingAspect]
    ////public void SomeMethod()
    ////{
    ////    // Your code here
    ////}
    /// </summary>
    [Serializable]
    public class TimingAspect : OnMethodBoundaryAspect
    {
        private Stopwatch stopwatch;

        public override void OnEntry(MethodExecutionArgs args)
        {
            stopwatch = Stopwatch.StartNew();
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            stopwatch.Stop();
            Console.WriteLine($"[TimingAspect] {args.Method.Name} - Elapsed time: {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}
