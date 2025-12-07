namespace Core.Models
{
    /// <summary>
    /// Представляет кластер в алгоритме k-means.
    /// Содержит центроид, список точек и уникальный идентификатор.
    /// </summary>
    /// <remarks>
    /// Центроид кластера пересчитывается как среднее всех точек:
    /// <code>
    /// Centroid[i] = (P1[i] + P2[i] + ... + PN[i]) / N
    /// </code>
    /// где <c>i</c> — номер признака, N — количество точек в кластере.
    /// </remarks>
    /// <remarks>
    /// Создает новый кластер с заданным идентификатором и центроидом.
    /// </remarks>
    /// <param name="id">Уникальный идентификатор кластера.</param>
    /// <param name="centroid">Начальный центроид кластера.</param>
    public class Cluster(int id, DataPoint centroid)
    {

        /// <summary>
        /// Центроид кластера.
        /// Это вектор среднего значения всех точек кластера.
        /// </summary>
        public DataPoint Centroid { get; set; } = centroid;

        /// <summary>
        /// Список точек, принадлежащих кластеру.
        /// </summary>
        public List<DataPoint> POINTS = [];

        /// <summary>
        /// Уникальный идентификатор кластера.
        /// </summary>
        public readonly int ID = id;

        /// <summary>
        /// Пересчитывает центроид кластера как среднее значение всех точек.
        /// </summary>
        /// <remarks>
        /// Если кластер пустой, центроид не меняется.
        /// Новый центроид вычисляется по формуле:
        /// <code>
        /// Centroid[i] = (Σ Pj[i]) / N
        /// </code>
        /// где <c>Pj[i]</c> — i-й признак j-й точки, N — количество точек.
        /// </remarks>
        /// <example>
        /// <code>
        /// var cluster = new Cluster(1, new DataPoint(new double[] {0,0}));
        /// cluster.Points.Add(new DataPoint(new double[] {1,2}));
        /// cluster.Points.Add(new DataPoint(new double[] {3,4}));
        /// cluster.UpdateCentroid();
        /// // Centroid.Features = {2,3}
        /// </code>
        /// </example>
        public void UpdateCentroid()
        {
            if (POINTS.Count == 0) return;

            int dimensions = Centroid.Features.Length;
            double[] newFeatures = new double[dimensions];

            foreach (var point in POINTS)
            {
                for (int i = 0; i < dimensions; i++)
                {
                    newFeatures[i] += point.Features[i];
                }
            }

            // Делим на количество точек прямо в этом же массиве
            double invCount = 1.0 / POINTS.Count;

            for (int i = 0; i < dimensions; i++)
            {
                newFeatures[i] *= invCount;
            }

            Centroid.Features = newFeatures;
        }
    }
}
