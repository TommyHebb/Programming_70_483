using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TommyTools;
namespace Chapter1
{
    public static class Program
    {
        [ThreadStatic]
        public static int _field01; // Used by: "5. Thread - Using the ThreadStaticAttribute"
        public static ThreadLocal<int> _field02 = new ThreadLocal<int>(() =>
        {
            return Thread.CurrentThread.ManagedThreadId; // Used by: "6. Thread - Using ThreadLocal<T>"
        });

        public static void Main()
        {

            if (ConsoleTools.Run("1. Thread - Basic"))
            {
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
            }
            if (ConsoleTools.Run("2. Thread - Background"))
            {
                Thread t = new Thread(new ParameterizedThreadStart(ThreadMethod));
                t.IsBackground = false;
                // 'true' only makes sense if it's the last thing the console application has to do.
                // otherwise, the output will get written anyway.
                t.Start(100);
            }
            if (ConsoleTools.Run("3. Tasks"))
            {
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
            }
            if (ConsoleTools.Run("4. Tasks - Shared variable to stop a thread"))
            {
                bool stopped = false;
                Thread t = new Thread(new ThreadStart(() =>
                {
                    while (!stopped)
                    {
                        Console.WriteLine("Running...");
                        Thread.Sleep(1000);
                    }
                }));
                t.Start();
                Console.WriteLine("Press any key to exit this exercise");
                Console.ReadKey();
                stopped = true;
                t.Join();
            }
            if (ConsoleTools.Run("5. Thread - Using the ThreadStaticAttribute"))
            {
                new Thread(() =>
                {
                    for (int x = 0; x < 10; x++)
                    {
                        _field01++;
                        Console.WriteLine("Thread A: {0}", _field01);
                    }
                }).Start();
                new Thread(() =>
                {
                    for (int x = 0; x < 10; x++)
                    {
                        _field01++;
                        Console.WriteLine("Thread B: {0}", _field01);
                    }
                }).Start();
            }
            if (ConsoleTools.Run("6. Thread - Using ThreadLocal<T>"))
            {
                new Thread(() =>
                {
                    for (int x = 0; x < _field02.Value; x++)
                    {
                        Console.WriteLine("Thread A: {0}", x);
                    }
                }).Start();

                new Thread(() =>
                {
                    for (int x = 0; x < _field02.Value; x++)
                    {
                        Console.WriteLine("Thread B: {0}", x);
                    }
                }).Start();

            }

            if (ConsoleTools.Run("Volgende is van pagina 26 !!!"))
            {
                var dict = new ConcurrentDictionary<string, int>();
                dict["k1"] = 42;
                int r1 = dict.AddOrUpdate("k1", 3, (s, i) => i * 2);
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