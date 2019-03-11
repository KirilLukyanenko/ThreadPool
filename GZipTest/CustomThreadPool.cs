using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GZipTest
{
    class CustomThreadPool : IDisposable
    {
        private readonly int threadPoolSize = Environment.ProcessorCount;
        private List<Thread> threads = new List<Thread>();
        private List<Action> tasks = new List<Action>();
        private bool dispose;

        /// <summary>
        /// Сonstructor
        /// </summary>
        public CustomThreadPool()
        {
            CreateAndStartThreadsInPool();
        }

        public void Dispose()
        {
            lock (tasks)
            {
                while (tasks.Count > 0)
                {
                    Monitor.Wait(tasks);
                }

                dispose = true;
                Monitor.PulseAll(tasks);
            }
            
            foreach (var thread in threads)
                thread.Join();
        }

        /// <summary>
        /// Add new task to threadpool
        /// </summary>
        public void AddTask(Action task)
        {
            lock (tasks)
            {
                if (dispose)
                    return;

                tasks.Add(task);
                Monitor.PulseAll(tasks);
            }
        }

        /// <summary>
        /// Create threadpool and start threads in it
        /// </summary>
        private void CreateAndStartThreadsInPool()
        {
            for (int i = 0; i < threadPoolSize; i++)
            {
                var thread = new Thread(StartThreadLifeCycle);
                threads.Add(thread);
                thread.Start();
            }
        }

        /// <summary>
        /// Start thread
        /// </summary>
        private void StartThreadLifeCycle()
        {
            while (true)
            {
                Action task;
                lock (tasks)
                {
                    while (true)
                    {
                        if (dispose)
                            return;

                        if (tasks.Count > 0 && threads.Count > 0) // when task is add, thread take it
                        {
                            task = tasks[0];
                            tasks.Remove(task);
                            threads.Remove(Thread.CurrentThread);
                            Monitor.PulseAll(tasks);
                            break;
                        }

                        Monitor.Wait(tasks); //waiting for task
                    }
                }

                task();

                lock (tasks)
                {
                    threads.Add(Thread.CurrentThread);
                }
            }
        }
    }
}
