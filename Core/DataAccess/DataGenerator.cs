using Core.Models;

namespace Core.DataAccess
{
    public static class DataGenerator
    {
        private static Random _random = new();

        // Генерация случайных точек с нормальным распределением вокруг центров
        public static List<DataPoint> GenerateClusteredData(int clustersCount, int pointsPerCluster, int dimensions)
        {
            var points = new List<DataPoint>();

            // Создаем случайные центры кластеров
            var clusterCenters = new List<DataPoint>();
            for (int i = 0; i < clustersCount; i++)
            {
                var centerFeatures = new double[dimensions];
                for (int j = 0; j < dimensions; j++)
                {
                    centerFeatures[j] = _random.NextDouble() * 100; // Центры в диапазоне [0, 100]
                }
                clusterCenters.Add(new DataPoint(centerFeatures));
            }

            // Генерируем точки вокруг центров
            for (int i = 0; i < clustersCount; i++)
            {
                for (int j = 0; j < pointsPerCluster; j++)
                {
                    var features = new double[dimensions];
                    for (int k = 0; k < dimensions; k++)
                    {
                        // Нормальное распределение вокруг центра с std = 10
                        double value = clusterCenters[i].Features[k] + (_random.NextDouble() - 0.5) * 20;
                        features[k] = value;
                    }
                    points.Add(new DataPoint(features));
                }
            }

            return points;
        }

        // Генерация случайных точек без кластерной структуры
        public static List<DataPoint> GenerateRandomData(int pointsCount, int dimensions)
        {
            var points = new List<DataPoint>();

            for (int i = 0; i < pointsCount; i++)
            {
                var features = new double[dimensions];
                for (int j = 0; j < dimensions; j++)
                {
                    features[j] = _random.NextDouble() * 100;
                }
                points.Add(new DataPoint(features));
            }

            return points;
        }
    }
}