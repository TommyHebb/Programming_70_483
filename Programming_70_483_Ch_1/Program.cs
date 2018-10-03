using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TommyTools;
namespace Chapter1
{
    public static class Program
    {
        public static void Main()
        {
            if (ConsoleTools.Run("Thread - Basic"))
            {
                ConsoleTools.Devider();
                // Would have used 'ThreadStart' here, but because the ThreadMethod gets used several times here, slightly different, 
                // 'ParameterizedThreadStart' is used here (and further on), as to use params.
                Thread t = new Thread(new ParameterizedThreadStart(ThreadMethod));
                t.Start(1); // '1' is the param offered to the ThreadMethod.
                for (int i = 0; i < 4; i++)
                {
                    Console.WriteLine("Main thread: Do some work.");
                    Thread.Sleep(1);
                }
                t.Join();
                ConsoleTools.Devider();
            }
            if (ConsoleTools.Run("Thread - Background"))
            {
                ConsoleTools.Devider();
                Thread t = new Thread(new ParameterizedThreadStart(ThreadMethod));
                t.IsBackground = false;
                // 'true' only makes sense if it's the last thing the console application has to do.
                // otherwise, the output will get written anyway.
                t.Start(100);
                ConsoleTools.Devider();
            }
            if (ConsoleTools.Run("Tasks"))
            {
                ConsoleTools.Devider();
                Task<int>[] tasks = new Task<int>[3];
                tasks[0] = Task.Run(() => { Thread.Sleep(2000); return 1; });
                tasks[1] = Task.Run(() => { Thread.Sleep(1000); return 2; });
                tasks[2] = Task.Run(() => { Thread.Sleep(3000); return 3; });
                while (tasks.Length > 0)
                {
                    int i = Task.WaitAny(tasks);
                    Task<int> completedTask = tasks[i];
                    Console.WriteLine(completedTask.Result);
                    var temp = tasks.ToList();
                    temp.RemoveAt(i);
                    tasks = temp.ToArray();
                }
                ConsoleTools.Devider();
            }
        }

        public static void ThreadMethod(object o)
        {
            ConsoleTools.Devider('-');
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("ThreadProc: {0}", i);
                Thread.Sleep((int)o);
            }
            ConsoleTools.Devider('-');
        }

    }
}