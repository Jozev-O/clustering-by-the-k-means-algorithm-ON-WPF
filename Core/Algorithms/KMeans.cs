using Core.Models;

namespace Core.Algorithms
{
    /// <summary>
    /// Реализация алгоритма k-means кластеризации.
    /// </summary>
    /// <remarks>
    /// Поддерживает инициализацию центроидов методом k-means++ и параллельное распределение точек по кластерам.
    /// Для больших наборов данных рекомендуется использовать оптимизированную версию с предварительным распределением точек.
    /// </remarks>  
    /// <remarks>
    /// Создает новый объект KMeans.
    /// </remarks>
    /// <param name="k">Количество кластеров.</param>
    /// <param name="maxIterations">Максимальное количество итераций алгоритма.</param>
    public class KMeans(int k, int maxIterations = 100)
    {
        private readonly int _K = k;
        private readonly int _MAX_ITERATIONS = maxIterations;

        /// <summary>
        /// Основной метод кластеризации.
        /// </summary>
        /// <param name="data">Список точек для кластеризации.</param>
        /// <returns>Список кластеров с назначенными точками и пересчитанными центроидами.</returns>
        /// <remarks>
        /// Алгоритм выполняет следующие шаги:
        /// 1. Инициализация центроидов методом k-means++.
        /// 2. Повторное назначение точек ближайшим центроидам.
        /// 3. Пересчет центроидов каждого кластера.
        /// 4. Остановка при отсутствии изменений или достижении максимального числа итераций.
        /// </remarks>
        public List<Cluster> Cluster(List<DataPoint> data)
        {
            var clusters = CreateClusters(InitializeCentroids(data));

            bool changed;
            int iteration = 0;

            do
            {
                changed = false;

                Parallel.ForEach(clusters, c => c.POINTS.Clear());

                // Назначение точек ближайшим центроидам
                Parallel.For(0, data.Count, i =>
                {
                    var point = data[i];
                    var nearestCluster = FindNearestCluster(point, clusters);

                    if (point.ClusterId != nearestCluster.ID)
                    {
                        point.ClusterId = nearestCluster.ID;
                        changed = true; // Можно использовать lock или Interlocked для потокобезопасного флага, но хз надо нет
                    }

                    lock (nearestCluster.POINTS) // защита добавления точки
                    {
                        nearestCluster.POINTS.Add(point);
                    }
                });


                Parallel.ForEach(clusters, cluster => cluster.UpdateCentroid());

                iteration++;
            } while (changed && iteration < _MAX_ITERATIONS);

            return clusters;
        }

        /// <summary>
        /// Инициализация центроидов методом k-means++.
        /// </summary>
        /// <param name="data">Список точек для кластеризации.</param>
        /// <returns>Список начальных центроидов.</returns>
        /// <remarks>
        /// Выбирает первый центроид случайно, остальные — с вероятностью, пропорциональной квадрату расстояния до ближайшего существующего центроида.
        /// </remarks>
        private List<DataPoint> InitializeCentroids(List<DataPoint> data)
        {
            var random = new Random();
            var centroids = new List<DataPoint>();
            var indices = new HashSet<int>();

            // Первый центроид случайно
            int firstIndex = random.Next(data.Count);
            centroids.Add(new DataPoint((double[])data[firstIndex].Features.Clone()));
            indices.Add(firstIndex);

            // k-means++ для остальных центроидов
            for (int c = 1; c < _K; c++)
            {
                double[] distances = new double[data.Count];
                double totalDistanceSquared = 0;

                Parallel.For(0, data.Count, i =>
                {
                    if (indices.Contains(i)) return;

                    double minDist = double.MaxValue;
                    foreach (var centroid in centroids)
                    {
                        double dist = data[i].DistanceTo(centroid);
                        if (dist < minDist) minDist = dist;
                    }

                    distances[i] = minDist * minDist;
                });


                // Выбираем следующую точку с вероятностью, пропорциональной квадрату расстояния
                foreach (var d in distances)
                    totalDistanceSquared += d;

                double randomValue = random.NextDouble() * totalDistanceSquared;
                double cumulative = 0;
                for (int i = 0; i < data.Count; i++)
                {
                    if (indices.Contains(i)) continue;

                    cumulative += distances[i];
                    if (cumulative >= randomValue)
                    {
                        centroids.Add(new DataPoint((double[])data[i].Features.Clone()));
                        indices.Add(i);
                        break;
                    }
                }
            }

            return centroids;
        }

        /// <summary>
        /// Создает кластеры с заданными центроидами.
        /// </summary>
        /// <param name="centroids">Список центроидов для кластеров.</param>
        /// <returns>Список кластеров.</returns>
        private static List<Cluster> CreateClusters(List<DataPoint> centroids)
        {
            var clusters = new List<Cluster>();
            for (int i = 0; i < centroids.Count; i++)
            {
                clusters.Add(new Cluster(i, centroids[i]));
            }
            return clusters;
        }

        /// <summary>
        /// Находит ближайший кластер для заданной точки.
        /// </summary>
        /// <param name="point">Точка, для которой ищется ближайший кластер.</param>
        /// <param name="clusters">Список кластеров.</param>
        /// <returns>Кластер с минимальным расстоянием до точки.</returns>
        private static Cluster FindNearestCluster(DataPoint point, List<Cluster> clusters)
        {
            Cluster nearest = null;
            double minDistance = double.MaxValue;

            foreach (var cluster in clusters)
            {
                double distance = point.DistanceTo(cluster.Centroid);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = cluster;
                }
            }

            return nearest;
        }
    }
}