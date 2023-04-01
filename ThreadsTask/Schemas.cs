using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lab1
{
    static class Schemas
    {
        public struct TaskDetails
        {
            //номер потока, задача
            public int Thread, Task;
            public DateTime start, end;
            public string fileName;


            public TaskDetails(int Thread, int Task, string fileName, DateTime start, DateTime end)
            {
                this.Thread = Thread;
                this.Task = Task;
                this.fileName = fileName;
                this.start = start;
                this.end = end;
            }
        }

        //для сбора информации о работе потоков
        public static ConcurrentBag<TaskDetails> Info;

        //вывод требуемой по заданию информации
        //задачи
        static public void PrintTaskInfo()
        {
            var table = Info.GroupBy(list => new { Task = list.Task, Thread = list.Thread })
                             .OrderBy(t => t.Key.Task)
                             .Select(Sample => new { Task = Sample.Key.Task, Thread = Sample.Key.Thread, Images = Sample.Count(), start = Sample.Min(x => x.start), end = Sample.Max(x => x.end) });
            using (StreamWriter stream = new StreamWriter(@"D:\OneDrive\Parall2\Output.txt", true))
            {
                stream.WriteLine(string.Format("{0,-9}|{1,-15}|{2,-19}|{3,-19}|{4,-8}", "Задача, №", "Число элементов", "Время старта", "Время завершения", "Поток, №") +
    "\n=========|===============|===================|===================|========");
                foreach (var item in table)
                    stream.WriteLine(string.Format("{0,-9}|{1,-15}|{2,-19}|{3,-19}|{4,-8}", item.Task.ToString(), item.Images.ToString(), item.start.ToString(), item.end.ToString(), item.Thread.ToString()));
                stream.WriteLine();
            }
        }
        //информация по схемам
        static public void PrintShemaInfo()
        {


            var threads = Info.Select(t => t.Thread).Distinct().Count();
            var time = (Info.Max(t => t.end) - Info.Min(t => t.end)).TotalMilliseconds;
            var tasks = Info.Select(t => t.Task).Distinct().Count();
            using (StreamWriter stream = new StreamWriter(@"D:\OneDrive\Parall2\Output.txt", true))
            {
                stream.WriteLine(string.Format("{0,-16}|{1,-13}|{2,-11}", "Время работы, мс", "Число потоков", "Число задач") +
"\n================|=============|============");
                stream.WriteLine(string.Format("{0,-16}|{1,-13}|{2,-11}", time.ToString(), threads.ToString(), tasks.ToString()));
                stream.WriteLine();
            }
        }
        //информация по потокам
        static public void PrintThreadsInfo()
        {

            var threads = Info.GroupBy(t => t.Thread).OrderBy(t => t.Key).Select(Sample => new { thread = Sample.Key, start = Sample.Min(x => x.start), end = Sample.Max(x => x.end), Images = Sample.Count() });
            //есть ли среди номеров потоков, номер главного?
            using (StreamWriter stream = new StreamWriter(@"D:\OneDrive\Parall2\Output.txt", true))
            {
                stream.WriteLine("\nОбработано изображений: {0}\n", Info.Count.ToString());
                stream.WriteLine("Главный поток, №: " + Program.Thmain.ToString());
                stream.WriteLine("Участие главного потока: {0}\n", Info.Select(t => t.Thread).Contains(Program.Thmain) ? "Да" : "Нет");
                stream.WriteLine(string.Format("{0,-8}|{1,-15}|{2,-19}|{3,-19}", "Поток, №", "Число элементов", "Время старта", "Время завершения") +
    "\n========|===============|===================|===================");
                foreach (var item in threads)
                    stream.WriteLine(string.Format("{0,-8}|{1,-15}|{2,-19}|{3,-19}", item.thread.ToString(), item.Images.ToString(), item.start.ToString(), item.end.ToString()));
                stream.WriteLine();
            }
        }


        static public void MakeBlack_White(string file)
        {
            //создаём Bitmap из исходного изображения
            var input = new Bitmap(file);
            // создаём Bitmap для черно-белого изображения
            var output = new Bitmap(input.Width, input.Height);
            // перебираем в циклах все пиксели исходного изображения
            for (int x = 0; x < input.Width; x++)
                for (int y = 0; y < input.Height; y++)
                {
                    // получаем (i, j) пиксель
                    var pixel = input.GetPixel(x, y);
                    // получаем компоненты цветов пикселя
                    int R = pixel.R;// красный
                    int G = pixel.G;// зеленый
                    int B = pixel.B;// синий
                    // делаем цвет черно-белым (оттенки серого) - находим среднее арифметическое
                    R = G = B = (R + G + B) / 3;
                    // добавляем его в Bitmap нового изображения
                    output.SetPixel(x, y, Color.FromArgb(R, G, B));
                }
            output.Save(file.Replace("ImagesBefore", "ImagesAfter"));
        }


        public static void Work(string file)
        {
            DateTime start, end;

            start = DateTime.Now;

            //узнать id потока и id задачи
            var thrId = Thread.CurrentThread.ManagedThreadId;
            var taskId = Task.CurrentId.Value;//без value вылазит ошибка в конструкторе
            MakeBlack_White(file);
            end = DateTime.Now;
            //сохраняем данные о работе
            Info.Add(new TaskDetails(thrId, taskId, file, start, end));
        }

    }
}

