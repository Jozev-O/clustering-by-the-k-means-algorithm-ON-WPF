using Core.Models;
using Core.DataAccess;
using Core.Algorithms;

namespace Core.Services
{
    public class ClusteringService
    {
        private readonly GwasDataLoader _dataLoader;
        private readonly int MAX_CLUSTERING_ITERATIONS = 200;
        public List<Cluster> Clusters { get; private set; }
        public Dictionary<int, double> InertiaValues { get; private set; }
        public Dictionary<int, double> SilhouetteScores { get; private set; }

        public ClusteringService(GwasDataLoader dataLoader)
        {
            _dataLoader = dataLoader;
        }


        //Загружает данные с помощью GwasDataLoader.LoadGwasData, 
        //    применяет фильтрацию(FilterSnps) и преобразует в List<DataPoint>
        //    через GwasSnp.ToDataPoint.Затем нормализует с GwasDataNormalizer.NormalizeGwasFeatures
        //        и NormalizeGwasData.Возвращает подготовленные данные.
        public async Task<List<DataPoint>> LoadAndPrepareDataAsync(string filePath, FilterOptions options)
        {
            var dataLoader = new GwasDataLoader(filePath, options.HasHeader);

            return await Task.Run(() =>
            {
                var filteredSnps = _dataLoader.LoadGwasDataStream(
                    minInfoScore: options.MinInfoScore,
                    maxPValue: options.MaxPValue,
                    minMaf: options.MinMaf
                ).ToList();

                var dataPoints = filteredSnps.Select(snp => snp.ToDataPoint(options.SelectedFeatures)).ToList();

                dataPoints = GwasDataNormalizer.NormalizeGwasFeatures(dataPoints, options.SelectedFeatures);
                dataPoints = GwasDataNormalizer.NormalizeGwasData(dataPoints, options.NormalizationMethod);

                return dataPoints;
            });
        }


        //Итеративно тестирует значения K от minK до maxK.Для каждого K запускает 
        //KMeans.Cluster, рассчитывает инерцию (сумма квадратов расстояний) и 
        //силуэтный коэффициент.Выбирает оптимальное K на основе "изгиба" 
        //инерции или максимального силуэта.Возвращает int optimalK.
        public int DetermineOptimalK(List<DataPoint> data, int minK, int maxK)
        {
            for (int k = minK; k <= maxK; k++)
            {
                var kmeans = new KMeans(k);
                var clusters = kmeans.Cluster(data);

                double inertia = CalculateInertia(clusters);
                double silhouette = CalculateSilhouetteScore(clusters);

                InertiaValues[k] = inertia;
                SilhouetteScores[k] = silhouette;
            }

            return SelectOptimalK(InertiaValues, SilhouetteScores);
        }


        //Инициализирует KMeans с заданным k и maxIterations.
        //Выполняет кластеризацию и возвращает List<Cluster>.
        public List<Cluster> PerformClustering(List<DataPoint> data, int k, int maxIterations = -1)
        {
            maxIterations = maxIterations > 0 ? maxIterations : MAX_CLUSTERING_ITERATIONS;
            var finalKmeans = new KMeans(k, maxIterations);
            Clusters = finalKmeans.Cluster(data);
            return Clusters;
        }


        //Рассчитывает дополнительные метрики (например, средние значения признаков по кластерам) 
        //и возвращает аналитический отчет(как объект или словарь).
        //Async-варианты: Для больших данных(например, 8 млн строк) добавьте асинхронные методы, 
        //такие как async Task<List<Cluster>> PerformClusteringAsync, используя Task.Run 
        //для параллельного выполнения.
        void AnalyzeClusters(List<Cluster> clusters)
        {

        }

        static int SelectOptimalK(Dictionary<int, double> inertiaValues,
                              Dictionary<int, double> silhouetteScores)
        {
            // Метод локтя + максимизация силуэтного коэффициента

            // Находим "изгиб" в кривой инерции
            var sortedInertia = inertiaValues.OrderBy(x => x.Key).ToList();
            double maxElbowRatio = 0;
            int elbowK = 2;

            for (int i = 1; i < sortedInertia.Count - 1; i++)
            {
                double prevDiff = sortedInertia[i - 1].Value - sortedInertia[i].Value;
                double nextDiff = sortedInertia[i].Value - sortedInertia[i + 1].Value;

                if (nextDiff != 0)
                {
                    double ratio = prevDiff / nextDiff;
                    if (ratio > maxElbowRatio)
                    {
                        maxElbowRatio = ratio;
                        elbowK = sortedInertia[i].Key;
                    }
                }
            }

            // Находим K с максимальным силуэтным коэффициентом
            var bestSilhouette = silhouetteScores.OrderByDescending(x => x.Value).First();

            // Компромиссное решение: если они близки, берем меньшее K
            if (Math.Abs(bestSilhouette.Key - elbowK) <= 2)
            {
                return Math.Min(bestSilhouette.Key, elbowK);
            }

            return bestSilhouette.Key; // Приоритет силуэтному коэффициенту
        }

        static double CalculateSilhouetteScore(List<Cluster> clusters)
        {
            if (clusters.Count <= 1) return 0;

            double totalSilhouette = 0;
            int totalPoints = clusters.Sum(c => c.POINTS.Count);

            foreach (var cluster in clusters)
            {
                foreach (var point in cluster.POINTS)
                {
                    // a(i): среднее расстояние до других точек в том же кластере
                    double a = cluster.POINTS.Where(p => p != point)
                        .Average(p => p.DistanceTo(point));

                    // b(i): минимальное среднее расстояние до точек другого кластера
                    double b = double.MaxValue;

                    foreach (var otherCluster in clusters.Where(c => c != cluster))
                    {
                        double avgDistance = otherCluster.POINTS
                            .Average(p => p.DistanceTo(point));

                        if (avgDistance < b) b = avgDistance;
                    }

                    // Силуэт для точки i
                    if (Math.Max(a, b) > 0)
                    {
                        totalSilhouette += (b - a) / Math.Max(a, b);
                    }
                }
            }

            return totalPoints > 0 ? totalSilhouette / totalPoints : 0;
        }

        static double CalculateInertia(List<Cluster> clusters)
        {
            double inertia = 0;
            foreach (var cluster in clusters)
            {
                foreach (var point in cluster.POINTS)
                {
                    inertia += Math.Pow(point.DistanceTo(cluster.Centroid), 2);
                }
            }
            return inertia;
        }
    }
}