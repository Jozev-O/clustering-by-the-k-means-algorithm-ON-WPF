using System.Globalization;

namespace Core.Models
{
    /// <summary>
    /// Представляет один SNP (Single Nucleotide Polymorphism) из GWAS-данных.
    /// Содержит исходные данные SNP и вычисляемые статистические показатели.
    /// </summary>
    public class GwasSnp
    {
        /// <summary>
        /// Идентификатор SNP (например, rsID).
        /// </summary>
        public readonly string SNP;

        /// <summary>
        /// Номер хромосомы (1-22, 23 = X, 24 = Y).
        /// </summary>
        public readonly int CHR;

        /// <summary>
        /// Позиция SNP на хромосоме.
        /// </summary>
        public readonly long Position;

        /// <summary>
        /// Эффектный аллель (A1).
        /// </summary>
        public readonly string ALLELE1;

        /// <summary>
        /// Альтернативный аллель (A0).
        /// </summary>
        public readonly string ALLELE0;

        /// <summary>
        /// Частота аллеля A1 в выборке (A1Freq).
        /// </summary>
        public readonly double A1FREQ;

        /// <summary>
        /// Показатель качества иммутации (INFO score).
        /// </summary>
        public readonly double INFO_SCORE;

        /// <summary>
        /// Эффект размера (Beta).
        /// </summary>
        public readonly double BETA;

        /// <summary>
        /// Стандартная ошибка эффекта (SE).
        /// </summary>
        public readonly double SE;

        /// <summary>
        /// P-значение ассоциации SNP с признаком.
        /// </summary>
        public readonly double PVAL;

        /// <summary>
        /// Z-статистика (BETA / SE).
        /// </summary>
        public readonly double ZScore;

        /// <summary>
        /// Отрицательный логарифм P-значения (-log10(PVAL)).
        /// </summary>
        public readonly double LogP;

        /// <summary>
        /// Создает пустой объект GwasSnp.
        /// </summary>
        public GwasSnp() { }

        /// <summary>
        /// Создает объект GwasSnp на основе массива строк, полученного из GWAS-файла.
        /// </summary>
        /// <param name="fields">
        /// Массив строк с данными SNP:
        /// [0] - SNP, [1] - CHR, [2] - Position, [3] - ALLELE1, [4] - ALLELE0,
        /// [5] - A1FREQ, [6] - INFO_SCORE, [7] - BETA, [8] - SE, [9] - PVAL
        /// </param>
        public GwasSnp(string[] fields)
        {
            if (fields.Length < 10)
                throw new ArgumentException("Недостаточно полей для создания GwasSnp.");
            SNP = fields[0];
            CHR = int.Parse(fields[1]);
            Position = long.Parse(fields[2]);
            ALLELE1 = fields[3];
            ALLELE0 = fields[4];
            A1FREQ = double.Parse(fields[5], CultureInfo.InvariantCulture);
            INFO_SCORE = double.Parse(fields[6], CultureInfo.InvariantCulture);
            BETA = double.Parse(fields[7], CultureInfo.InvariantCulture);
            SE = double.Parse(fields[8], CultureInfo.InvariantCulture);
            PVAL = double.Parse(fields[9], CultureInfo.InvariantCulture);

            // Вычисляем производные показатели
            ZScore = BETA / SE;
            LogP = -Math.Log10(PVAL);
        }

        /// <summary>
        /// Преобразует SNP в числовой вектор признаков для машинного обучения или кластеризации.
        /// </summary>
        /// <param name="selectedFeatures">
        /// Список признаков, которые нужно включить в DataPoint.
        /// Допустимые значения:
        /// "A1FREQ", "INFO", "BETA", "SE", "ZSCORE", "LOGP", "PVAL",
        /// "POSITION_NORM" (позиция нормализованная 0–1),
        /// "CHR_NORM" (номер хромосомы нормализован 0–1),
        /// "MAF" (частота редкого аллеля)
        /// </param>
        /// <returns>
        /// Объект <see cref="DataPoint"/> с выбранными признаками.
        /// </returns>
        public DataPoint ToDataPoint(params string[] selectedFeatures)
        {
            var features = new List<double>();

            foreach (var feature in selectedFeatures)
            {
                switch (feature.ToUpper())
                {
                    case "A1FREQ":
                        features.Add(A1FREQ);
                        break;
                    case "INFO":
                        features.Add(INFO_SCORE);
                        break;
                    case "BETA":
                        features.Add(BETA);
                        break;
                    case "SE":
                        features.Add(SE);
                        break;
                    case "ZSCORE":
                        features.Add(ZScore);
                        break;
                    case "LOGP":
                        features.Add(LogP);
                        break;
                    case "PVAL":
                        features.Add(PVAL);
                        break;
                    case "POSITION_NORM":
                        features.Add(Position / 1_000_000_000.0);
                        break;
                    case "CHR_NORM":
                        features.Add(CHR / 24.0);
                        break;
                    case "MAF":
                        features.Add(Math.Min(A1FREQ, 1 - A1FREQ));
                        break;
                }
            }
            return new DataPoint(features.ToArray());
        }
    }
}