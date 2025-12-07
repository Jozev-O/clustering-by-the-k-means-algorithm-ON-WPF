using Core.Models;

namespace Core.DataAccess
{
    /// <summary>
    /// Класс для потоковой загрузки и фильтрации GWAS данных.
    /// </summary>
    public class GwasDataLoader
    {
        private readonly string _FILE_PATH;
        private bool _hasHeader = true;

        /// <summary>
        /// Конструктор класса <see cref="GwasDataLoader"/>.
        /// </summary>
        /// <param name="filePath">Путь к файлу с GWAS-данными.</param>
        /// <param name="hasHeader">
        /// Указывает, содержит ли файл заголовок. 
        /// Если true, первая строка будет пропущена при чтении.
        /// </param>
        /// <exception cref="FileNotFoundException">
        /// Генерируется, если указанный файл не существует.
        /// </exception>
        /// <remarks>
        /// Конструктор проверяет существование файла по указанному пути.
        /// </remarks>
        public GwasDataLoader(string filePath, bool hasHeader)
        {
            if (File.Exists(filePath))
                _FILE_PATH = filePath;
            else
                throw new FileNotFoundException("Файл не найден", filePath);

            _hasHeader = hasHeader;
        }


        /// <summary>
        /// Загружает SNP из файла GWAS построчно с фильтрацией.
        /// </summary>
        /// <param name="filePath">Путь к файлу GWAS данных.</param>
        /// <param name="hasHeader">Указывает, есть ли заголовок в файле.</param>
        /// <param name="minInfoScore">Минимальное значение INFO score для фильтрации.</param>
        /// <param name="maxPValue">Максимальное значение P-value для фильтрации.</param>
        /// <param name="minMaf">Минимальная частота минорного аллеля (MAF) для фильтрации.</param>
        /// <returns>Поток объектов GwasSnp, удовлетворяющих условиям фильтрации.</returns>
        public IEnumerable<GwasSnp> LoadGwasDataStream(
            double minInfoScore = 0.8,
            double maxPValue = 0.05,
            double minMaf = 0.01)
        {
            using var reader = new StreamReader(_FILE_PATH);
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                if (_hasHeader)
                {
                    _hasHeader = false;
                    continue;
                }

                line = line.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var fields = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

                if (fields.Length >= 10)
                {
                    var snp = new GwasSnp(fields);

                    // Фильтрация сразу
                    double maf = Math.Min(snp.A1FREQ, 1 - snp.A1FREQ);
                    if (snp.INFO_SCORE >= minInfoScore &&
                        snp.PVAL <= maxPValue &&
                        maf >= minMaf)
                    {
                        yield return snp;
                    }
                }
            }
        }
    }
}
