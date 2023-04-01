using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lab1
{
    class Program
    {

        public static int Thmain;
        public static int maxParall = Environment.ProcessorCount;

        static void Run(int N, string[] files)
        {
            //Последовательный алгоритм
            using (StreamWriter stream = new StreamWriter(@"D:\OneDrive\Parall2\Output.txt", true))
            {
                stream.WriteLine("Последовательный алгоритм");
            }
            DateTime d1 = DateTime.Now;
            foreach (var file in files)
                Schemas.MakeBlack_White(file);
            DateTime d2 = DateTime.Now;
            using (StreamWriter stream = new StreamWriter(@"D:\OneDrive\Parall2\Output.txt", true))
            {
                stream.WriteLine("\nОбработано изображений: {0}\n", N.ToString());
                stream.WriteLine("Затраченное время: {0} мс\n", (d2 - d1).TotalMilliseconds.ToString());
            }
            Console.WriteLine("Выполнено: Последовательный алгоритм");

            //параллельные версии

            var cts = new CancellationTokenSource();
            Thread thr = new Thread(() => {
                while (true)
                {
                    Thread.Sleep(100);
                    //Возвращает значение, указывающее на событие нажатия клавиши (во входном потоке).
                    if (Console.KeyAvailable)
                    {
                        Console.ReadKey(true);
                        //планировщик отменяет еще не запланированные действия
                        cts.Cancel();
                    }
                }
            });

            thr.Start();
            var Options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = maxParall,
                //получаем сигнал отмены
                CancellationToken = cts.Token
            };
            //parallelFor
            using (StreamWriter stream = new StreamWriter(@"D:\OneDrive\Parall2\Output.txt", true))
            {
                stream.WriteLine("\nПараллельный алгоритм");
            }
            Schemas.Info = new ConcurrentBag<Schemas.TaskDetails>();
            Parallel.For(0, N, Options, i =>
            {
                Schemas.Work(files[i]);

            });

            Schemas.PrintThreadsInfo();
            Schemas.PrintShemaInfo();

            Console.WriteLine("Выполнено: Параллельный алгоритм");
            //стандартная схема
            using (StreamWriter stream = new StreamWriter(@"D:\OneDrive\Parall2\Output.txt", true))
            {
                stream.WriteLine("\nСтандартная схема");
            }
            Schemas.Info = new ConcurrentBag<Schemas.TaskDetails>();
            Parallel.ForEach(files, Options, Schemas.Work);
            Schemas.PrintThreadsInfo();
            Schemas.PrintTaskInfo();
            Schemas.PrintShemaInfo();

            Console.WriteLine("Выполнено: Cтандартная схема");
            //сбалансированная схема
            using (StreamWriter stream = new StreamWriter(@"D:\OneDrive\Parall2\Output.txt", true))
            {
                stream.WriteLine("\nСбалансированная схема");
            }
            Schemas.Info = new ConcurrentBag<Schemas.TaskDetails>();
            var PartedData = Partitioner.Create(files, true);
            Parallel.ForEach(PartedData, Options, Schemas.Work);
            Schemas.PrintThreadsInfo();
            Schemas.PrintTaskInfo();
            Schemas.PrintShemaInfo();
            Console.WriteLine("Выполнено: Сбалансированная схема");
            //Статическая схема
            using (StreamWriter stream = new StreamWriter(@"D:\OneDrive\Parall2\Output.txt", true))
            {
                stream.WriteLine("\nСтатическая схема");
            }
            Schemas.Info = new ConcurrentBag<Schemas.TaskDetails>();
            Parallel.ForEach(Partitioner.Create(0, N), Options, range => {
                for (int i = range.Item1; i < range.Item2; i++)
                    Schemas.Work(files[i]);
            });
            Schemas.PrintThreadsInfo();
            Schemas.PrintTaskInfo();
            Schemas.PrintShemaInfo();

            Console.WriteLine("Выполнено: Статическая схема");
            //Статическая схема с фиксированным размером блока
            using (StreamWriter stream = new StreamWriter(@"D:\OneDrive\Parall2\Output.txt", true))
            {
                stream.WriteLine("\nСтатическая схема с фиксированным размером блока");
            }
            Schemas.Info = new ConcurrentBag<Schemas.TaskDetails>();
            Parallel.ForEach(Partitioner.Create(0, N, N / (maxParall)), Options,
            range => {
                for (int i = range.Item1; i < range.Item2; i++)
                    Schemas.Work(files[i]);
            });
            Schemas.PrintThreadsInfo();
            Schemas.PrintTaskInfo();
            Schemas.PrintShemaInfo();

            Console.WriteLine("Выполнено: Статическая схема с фиксированным размером блока");
        }

        static void Main(string[] args)
        {
            Thmain = Thread.CurrentThread.ManagedThreadId;
            //получаем имена всех изображений
            var Allfiles = Directory.GetFiles(@"D:\OneDrive\Parall2\ImagesBefore", "*.j*");

            string[] files;
            try
            {
                files = new string[10];
                Array.Copy(Allfiles, files, 10);
                Run(10, files);
                files = new string[20];
                Array.Copy(Allfiles, files, 20);
                Run(20, files);
                files = new string[50];
                Array.Copy(Allfiles, files, 50);
                Run(50, files);
                Run(100, Allfiles);
                Console.WriteLine("Готово!");

            }
            catch (OperationCanceledException o)
            {
                Console.WriteLine(o.Message);
            }
            Console.ReadKey();
        }

    }
}


