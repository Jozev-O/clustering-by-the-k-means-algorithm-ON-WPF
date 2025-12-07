using Core.Models;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace TEMP_FOR_TESTS
{
    class Program
    {
        static void ShowImg(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                Console.WriteLine("Файл не найден!");
                return;
            }

            // Открываем изображение в программе по умолчанию
            Process.Start(new ProcessStartInfo
            {
                FileName = imagePath,
                UseShellExecute = true
            });

            Console.WriteLine("Изображение открыто в программе просмотра!\nНажмите любую клавишу что бы продолжить");
            //Console.ReadKey();
        }

        static void Main()
        {

            var plt = new ScottPlot.Plot();

            // Подписи осей
            plt.XLabel("X axis");
            plt.YLabel("Y axis");

            var data = new List<DataPoint>();
            for (int i = 0; i < 100; i++)
            {
                var point = new DataPoint(new double[] { i, i * 2 + 5 + RandomNumberGenerator.GetInt32(-10, 10) });
                data.Add(point);
            }

            var xs = data.Select(p => p.Features[0]).ToArray();
            var ys = data.Select(p => p.Features[1]).ToArray();
            var scatter = plt.Add.Scatter(xs, ys);
            plt.SavePng("graph1.png", 600, 400);

            ShowImg("graph1.png");

            for (int i = 0; i < 2; i++)
            {
                double minValue = data.Min(p => p.Features[i]);
                double epsilon = minValue <= 0 ? Math.Abs(minValue) + 0.01 : 0;

                foreach (var point in data)
                {
                    point.Features[i] = Math.Log(point.Features[i] + epsilon + 1);
                }
            }

            var xs2 = data.Select(p => p.Features[0]).ToArray();
            var ys2 = data.Select(p => p.Features[1]).ToArray();
            var scatter2 = plt.Add.Scatter(xs2, ys2);

            var plt2 = new ScottPlot.Plot();
            plt2.XLabel("X axis (normalized)");
            plt2.YLabel("Y axis (normalized)");
            plt2.Add.Scatter(xs2, ys2);
            plt2.SavePng("graph2.png", 600, 400);
            ShowImg("graph2.png");
            










            //int numPoints = 10_000_000;  // 8 млн точек
            //int dimensions = 5;         // количество признаков
            //int k = 10;                 // количество кластеров

            //var data = new List<DataPoint>(numPoints);
            //var random = new Random();

            //Console.WriteLine("Генерация точек...");
            //for (int i = 0; i < numPoints; i++)
            //{
            //    double[] features = new double[dimensions];
            //    for (int j = 0; j < dimensions; j++)
            //        features[j] = random.NextDouble(); // случайные значения 0-1

            //    data.Add(new DataPoint(features));
            //}

            //var clusters = new List<string>();
            //var task1 = Task.Run(() =>
            //{
            //    return MeasureExecutionTime("+++ Cluster", () =>
            //    {
            //        var kmeans = new KMeans(k, 50);
            //        var clusters = kmeans.Cluster(data);
            //    });
            //});

            //var task2 = Task.Run(() =>
            //{
            //    return MeasureExecutionTime("++ Cluster", () =>
            //    {
            //        var kmeans = new KMeans(k, 50);
            //        var clusters = kmeans.Cluster0(data);
            //    });
            //});

            //var task3 = Task.Run(() =>
            //{
            //    return MeasureExecutionTime("Cluster2", () =>
            //    {
            //        var kmeans = new KMeans(k, 50);
            //        var clusters = kmeans.Cluster2(data);
            //    });
            //});

            //Task.WaitAll(task1, task2, task3);
            //Console.WriteLine(task1.Result);
            //Console.WriteLine(task2.Result);
            //Console.WriteLine(task3.Result);
        }

        async static Task<string> MeasureExecutionTime(string description, Action action)
        {
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine("==================================================");
            stringBuilder.AppendLine("Начало кластеризации...");
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            stringBuilder.AppendLine($"{description} выполнено за {stopwatch.Elapsed.TotalSeconds} секунд.");
            stringBuilder.AppendLine("==================================================\n\n");
            return stringBuilder.ToString();
        }
    }
}