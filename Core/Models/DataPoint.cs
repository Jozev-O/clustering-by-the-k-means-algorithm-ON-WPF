namespace Core.Models
{
    /// <summary>
    /// Представляет точку данных в многомерном пространстве для кластеризации K-Means.
    /// </summary>
    public class DataPoint
    {
        /// <summary>
        /// Массив значений признаков точки данных.
        /// </summary>
        public double[] Features { get; set; }

        /// <summary>
        /// Идентификатор кластера, к которому принадлежит точка данных.
        /// Инициализируется значением -1, что означает непринадлежность к какому-либо кластеру.
        /// </summary>
        public int ClusterId { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DataPoint"/> с заданными признаками.
        /// </summary>
        /// <param name="features">Массив значений признаков точки данных.</param>
        public DataPoint(double[] features)
        {
            Features = features ?? throw new ArgumentNullException(nameof(features));
            ClusterId = -1; // -1 означает, что точка еще не кластеризована
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DataPoint"/> с двумерными координатами.
        /// Для удобства работы с двумерными данными.
        /// </summary>
        /// <param name="x">Координата X точки данных.</param>
        /// <param name="y">Координата Y точки данных.</param>
        public DataPoint(double x, double y)
        {
            Features = [x, y];
            ClusterId = -1;
        }


        /// <summary>
        /// Вычисляет евклидово расстояние от данной точки до другой точки данных.
        /// Используется в алгоритме K-Means для определения ближайшего центроида.
        /// </summary>
        /// <param name="other">Целевая точка данных, до которой вычисляется расстояние.</param>
        /// <returns>Евклидово расстояние между точками.</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если otherPoint равен null.</exception>
        /// <exception cref="ArgumentException">Выбрасывается, если размерности признаков не совпадают.</exception>
        public double DistanceTo(DataPoint other)
        {
            ArgumentNullException.ThrowIfNull(other);

            if (Features.Length != other.Features.Length)
                throw new ArgumentException("Размерности признаков точек данных не совпадают.");

            double sum = 0;

            for (int i = 0; i < Features.Length; i++)
            {
                double diff = Features[i] - other.Features[i];
                sum += diff * diff;
            }
            return Math.Sqrt(sum);
        }
    }
}