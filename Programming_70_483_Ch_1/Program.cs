using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
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
            if (ConsoleTools.Run("7. Thread - Queuing some work to the thread pool"))
            {
                ThreadPool.QueueUserWorkItem((s) =>
                {
                    Console.WriteLine("Working on a thread from threadpool");
                });
            }
            if (ConsoleTools.Run("8. Tasks - How to start a new Task and wait until it’s finished"))
            {
                Task t = Task.Run(() =>
                {
                    for (int x = 0; x < 100; x++)
                    {
                        Console.Write('>');
                    }
                });

                t.Wait();
            }
            if (ConsoleTools.Run("9. Tasks - If a Task should return a value"))
            {
                Task<int> t = Task.Run(() =>
                {
                    return 42;
                });

                Console.WriteLine(t.Result);
            }
            if (ConsoleTools.Run("10. Tasks - Add a continuation Task"))
            {
                Task<int> t = Task.Run(() =>
                {
                    return 42;
                }).ContinueWith((i) =>
                {
                    return i.Result * 2;
                });
                Console.WriteLine(t.Result);
            }
            if (ConsoleTools.Run("11. Tasks - Scheduling different continuation Tasks"))
            {
                Task<int> t = Task.Run(() => { return 42; });
                t.ContinueWith((i) =>
                {
                    Console.WriteLine("Canceled");
                }, TaskContinuationOptions.OnlyOnCanceled);
                t.ContinueWith((i) =>
                {
                    Console.WriteLine("Faulted");
                }, TaskContinuationOptions.OnlyOnFaulted);
                var completedTask = t.ContinueWith((i) =>
                {
                    Console.WriteLine("Completed");
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
                completedTask.Wait();
            }
            if (ConsoleTools.Run("12. Tasks - Attaching child Tasks to a parent Task"))
            {
                Task<Int32[]> parent = Task.Run(() =>
                {
                    var results = new Int32[3];
                    new Task(() => results[0] = 0, TaskCreationOptions.AttachedToParent).Start();
                    new Task(() => results[1] = 1, TaskCreationOptions.AttachedToParent).Start();
                    new Task(() => results[2] = 2, TaskCreationOptions.AttachedToParent).Start();
                    return results;
                });
                var finalTask = parent.ContinueWith(parentTask =>
                {
                    foreach (int i in parentTask.Result)
                        Console.WriteLine(i);
                });
                finalTask.Wait();
            }
            if (ConsoleTools.Run("13. Tasks - Using a TaskFactory"))
            {
                Task<Int32[]> parent = Task.Run(() =>
                {
                    var results = new Int32[3];
                    TaskFactory tf = new TaskFactory(TaskCreationOptions.AttachedToParent, TaskContinuationOptions.ExecuteSynchronously);
                    tf.StartNew(() => results[0] = 0);
                    tf.StartNew(() => results[1] = 1);
                    tf.StartNew(() => results[2] = 2);
                    return results;
                });
                var finalTask = parent.ContinueWith(parentTask =>
                {
                    foreach (int i in parentTask.Result)
                        Console.WriteLine(i);
                });
                finalTask.Wait();
            }
            if (ConsoleTools.Run("14. Tasks - Using Task.WaitAll"))
            {
                Task[] tasks = new Task[3];
                tasks[0] = Task.Run(() => { Thread.Sleep(1000); Console.WriteLine("1"); return 1; });
                tasks[1] = Task.Run(() => { Thread.Sleep(1000); Console.WriteLine("2"); return 2; });
                tasks[2] = Task.Run(() => { Thread.Sleep(1000); Console.WriteLine("3"); return 3; });
                Task.WaitAll(tasks);
            }
            if (ConsoleTools.Run("15. Tasks - Using Task.WaitAny"))
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
            if (ConsoleTools.Run("16. Parallel - Using Parallel.For and Parallel.Foreach"))
            {
                Parallel.For(0, 10, i => { Thread.Sleep(1000); });
                var numbers = Enumerable.Range(0, 10);
                Parallel.ForEach(numbers, i => { Thread.Sleep(1000); });
            }
            if (ConsoleTools.Run("17. Parallel - Using Parallel.Break"))
            {
                ParallelLoopResult result = Parallel.For(0, 1000, (int i, ParallelLoopState loopState) =>
                {
                    if (i == 500)
                    {
                        Console.WriteLine("Breaking loop");
                        loopState.Break();
                    }
                    return;
                });
            }
            if (ConsoleTools.Run("18. Async and Await - Simple example"))
            {
                // Uses static method DownloadContent
                string result = DownloadContent().Result;
                Console.WriteLine(result);
            }
            if (ConsoleTools.Run("19. Async and Await - "))
            {

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

        public static async Task<string> DownloadContent()
        {
            using (HttpClient client = new HttpClient())
            {
                // Used by: "18. Async and Await - Simple example"
                string result = await client.GetStringAsync("http://www.microsoft.com");
                return result;
            }
        }

    }
}