using Core.Models;

namespace Core.Algorithms
{
    /// <summary>
    /// Класс для нормализации данных GWAS (Genome-Wide Association Study).
    /// </summary>
    /// <remarks>
    /// Поддерживает общие методы нормализации всех признаков, а также специфичные методы нормализации
    /// для различных типов GWAS-признаков, таких как PVAL, BETA, ZSCORE, A1FREQ, MAF и INFO.
    /// </remarks>
    public class GwasDataNormalizer
    {
        /// <summary>
        /// Методы нормализации данных.
        /// </summary>
        public enum NormalizationMethod
        {
            /// <summary>
            /// Z-score нормализация: (x - mean) / std
            /// </summary>
            Standard,

            /// <summary>
            /// Масштабирование в диапазон [0,1]
            /// </summary>
            MinMax,

            /// <summary>
            /// Устойчивая нормализация с использованием медианы и IQR
            /// </summary>
            Robust,

            /// <summary>
            /// Логарифмическое преобразование данных
            /// </summary>
            LogTransform,

            /// <summary>
            /// Преобразование значений в ранги (Rank-Based)
            /// </summary>
            RankBased
        }

        /// <summary>
        /// Нормализация списка точек данных GWAS с использованием выбранного метода.
        /// </summary>
        /// <param name="data">Список точек данных для нормализации.</param>
        /// <param name="method">Метод нормализации (Standard, MinMax, Robust, LogTransform, RankBased).</param>
        /// <returns>Список нормализованных точек данных.</returns>
        /// <remarks>
        /// Каждый признак каждой точки нормализуется в соответствии с выбранным методом.
        /// Z-score используется для стандартизации распределений с нормальной формой.
        /// MinMax масштабирует признаки в диапазон [0,1].
        /// Robust использует медиану и межквартильный размах, устойчивый к выбросам.
        /// LogTransform применяет логарифм к положительным значениям признаков.
        /// </remarks>
        public static List<DataPoint> NormalizeGwasData(List<DataPoint> data, NormalizationMethod method)
        {
            if (data.Count == 0) return data;

            int dimensions = data[0].Features.Length;
            var normalizedData = new List<DataPoint>();

            switch (method)
            {
                case NormalizationMethod.Standard:
                    // Z-score: (x - mean) / std
                    for (int i = 0; i < dimensions; i++)
                    {
                        double mean = data.Average(p => p.Features[i]);
                        double std = Math.Sqrt(data.Average(p => Math.Pow(p.Features[i] - mean, 2)));

                        foreach (var point in data)
                        {
                            if (std != 0)
                                point.Features[i] = (point.Features[i] - mean) / std;
                        }
                    }
                    break;

                case NormalizationMethod.MinMax:
                    // [0, 1] scaling
                    for (int i = 0; i < dimensions; i++)
                    {
                        double min = data.Min(p => p.Features[i]);
                        double max = data.Max(p => p.Features[i]);
                        double range = max - min;

                        foreach (var point in data)
                        {
                            if (range != 0)
                                point.Features[i] = (point.Features[i] - min) / range;
                        }
                    }
                    break;

                case NormalizationMethod.Robust:
                    // Robust scaling: (x - median) / IQR
                    for (int i = 0; i < dimensions; i++)
                    {
                        var values = data.Select(p => p.Features[i]).OrderBy(v => v).ToList();
                        double median = values[values.Count / 2];

                        int q1Index = values.Count / 4;
                        int q3Index = values.Count * 3 / 4;
                        double q1 = values[q1Index];
                        double q3 = values[q3Index];
                        double iqr = q3 - q1;

                        foreach (var point in data)
                        {
                            if (iqr != 0)
                                point.Features[i] = (point.Features[i] - median) / iqr;
                        }
                    }
                    break;

                case NormalizationMethod.LogTransform:
                    // log(x + epsilon) для положительных значений
                    for (int i = 0; i < dimensions; i++)
                    {
                        double minValue = data.Min(p => p.Features[i]);
                        double epsilon = minValue <= 0 ? Math.Abs(minValue) + 0.01 : 0;

                        foreach (var point in data)
                        {
                            point.Features[i] = Math.Log(point.Features[i] + epsilon + 1);
                        }
                    }
                    break;
            }

            return data;
        }

        /// <summary>
        /// Нормализация GWAS-признаков с учётом специфики каждого признака.
        /// </summary>
        /// <param name="data">Список точек данных GWAS для нормализации.</param>
        /// <param name="featureNames">Массив названий признаков, соответствующих каждой размерности Features.</param>
        /// <returns>Список нормализованных точек данных.</returns>
        /// <remarks>
        /// Специфичные преобразования:
        /// - PVAL, LOGP: -log10(p-value)
        /// - BETA, ZSCORE: Z-score нормализация
        /// - A1FREQ, MAF: arcsin(sqrt(frequency))
        /// - INFO: значения уже в диапазоне [0,1], дополнительная нормализация опциональна
        /// </remarks>
        public static List<DataPoint> NormalizeGwasFeatures(List<DataPoint> data, string[] featureNames)
        {
            if (data.Count == 0) return data;

            for (int i = 0; i < featureNames.Length; i++)
            {
                var feature = featureNames[i];
                var values = data.Select(p => p.Features[i]).ToList();

                switch (feature.ToUpper())
                {
                    case "PVAL":
                    case "LOGP":
                        // Для p-values часто используют -log10(p)
                        foreach (var point in data)
                        {
                            point.Features[i] = -Math.Log10(point.Features[i] + 1e-300);
                        }
                        break;

                    case "BETA":
                    case "ZSCORE":
                        // Для эффектов и Z-score используем стандартизацию
                        double mean = values.Average();
                        double std = Math.Sqrt(values.Average(v => Math.Pow(v - mean, 2)));

                        if (std > 0)
                        {
                            foreach (var point in data)
                            {
                                point.Features[i] = (point.Features[i] - mean) / std;
                            }
                        }
                        break;

                    case "A1FREQ":
                    case "MAF":
                        // Для частот аллелей используем arcsin sqrt преобразование
                        foreach (var point in data)
                        {
                            double freq = Math.Max(0.001, Math.Min(0.999, point.Features[i]));
                            point.Features[i] = Math.Asin(Math.Sqrt(freq));
                        }
                        break;

                    case "INFO":
                        // INFO score уже в диапазоне [0, 1]
                        // Можно дополнительно трансформировать
                        break;
                }
            }
            return data;
        }
    }
}
