using Library.Entities;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Library
{
    /// <summary>
    /// Контекст базы данных для Web API.  Обеспечивает доступ к данным CSV и результатам интегральных вычислений.
    /// </summary>
    /// <param name="options">Параметры для настройки контекста базы данных.</param>
    public class WebApiDbContext(DbContextOptions<WebApiDbContext> options) : DbContext(options)
    {
        private class CsvDataMap : ClassMap<CsvDataEntity>
        {
            public CsvDataMap()
            {
                Map(m => m.Date)
                    .TypeConverter<CsvHelper.TypeConversion.DateTimeConverter>()
                    .TypeConverterOption.Format("yyyy-MM-dd H:mm:ss.fff");
                Map(m => m.ExecutionTime);
                Map(m => m.Value);
            }
        }

        /// <summary>
        /// Набор данных, представляющий данные из CSV-файлов.
        /// </summary>
        public DbSet<CsvDataEntity> Values { get; set; }

        /// <summary>
        /// Набор данных, представляющий результаты вычислений.
        /// </summary>
        public DbSet<IntegralResultEntity> Results { get; set; }

        /// <summary>
        /// Сохраняет данные из CSV-файла в базу данных асинхронно.
        /// </summary>
        /// <param name="pathToCsv">Путь к CSV-файлу.</param>
        /// <exception cref="Exception">Выбрасывается, если CSV-файл недействителен.</exception>
        public async Task SaveDataFromCsvToDbAsync(string pathToCsv)
        {
            using StreamReader streamReader = new(pathToCsv);
            using CsvReader csvReader = new(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";"
            });
            csvReader.Context.RegisterClassMap<CsvDataMap>();

            List<CsvDataEntity> csvDatas = csvReader.GetRecords<CsvDataEntity>().ToList();

            if (!Validator.IsCsvDataValid(csvDatas))
                throw new Exception("csv file invalid\n");

            string csvFileName = Path.GetFileName(pathToCsv);

            foreach (CsvDataEntity csvDataEntity in csvDatas)
            {
                csvDataEntity.Id = Guid.NewGuid();
                csvDataEntity.FileName = csvFileName;

                // Преобразование Date в UTC
                if (csvDataEntity.Date.Kind != DateTimeKind.Utc)
                {
                    csvDataEntity.Date = csvDataEntity.Date.ToUniversalTime();
                }
            }

            this.Values.AddRange(csvDatas);
            await this.SaveChangesAsync();

            await FillingResultTableAsync(csvFileName, csvDatas);
            Console.WriteLine($"{csvFileName} сохранён!\n");
        }

        private async Task FillingResultTableAsync(string fileName, IEnumerable<CsvDataEntity> csvDatas)
        {
            int timeDelta = (int)(csvDatas.Max(x => x.Date) - csvDatas.Min(x => x.Date)).TotalSeconds;
            DateTime minDate = csvDatas.Min(x => x.Date);
            double averageExecutionTime = csvDatas.Average(x => x.ExecutionTime);
            double averageValue = csvDatas.Average(x => x.Value);
            double minValue = csvDatas.Min(x => x.Value);
            double maxValue = csvDatas.Max(x => x.Value);

            double medianValue = CalculateMedian(csvDatas.Select(x => x.Value));

            IntegralResultEntity? integralResult = await this.Results.FirstOrDefaultAsync(r => r.FileName == fileName);

            if (integralResult == null)
            {
                integralResult = new IntegralResultEntity();
                integralResult.FileName = fileName;
                this.Results.Add(integralResult);
            }

            integralResult.TimeDelta = timeDelta;
            integralResult.MinDate = minDate;
            integralResult.AverageExecutionTime = averageExecutionTime;
            integralResult.AverageValue = averageValue;
            integralResult.MedianValue = medianValue;
            integralResult.MinValue = minValue;
            integralResult.MaxValue = maxValue;

            await this.SaveChangesAsync();
        }

        private static double CalculateMedian(IEnumerable<double> data)
        {
            List<double> values = data.ToList();
            values.Sort();
            int count = values.Count;

            if (count % 2 == 0)
            {
                double middleValue1 = values[count / 2 - 1];
                double middleValue2 = values[count / 2];
                return (middleValue1 + middleValue2) / 2;
            }
            else
            {
                return values[count / 2];
            }
        }

        /// <summary>
        /// Асинхронно получает отфильтрованный список результатов вычисления.
        /// </summary>
        /// <param name="fileName">Фильтр по имени файла (необязательный). Если указан, возвращаются результаты только для указанного файла.</param>
        /// <param name="minStartDate">Фильтр по минимальной дате начала (необязательный). Если указан, возвращаются результаты с датой начала не ранее указанной.</param>
        /// <param name="maxStartDate">Фильтр по максимальной дате начала (необязательный). Если указан, возвращаются результаты с датой начала не позднее указанной.</param>
        /// <param name="minAverageValue">Фильтр по минимальному среднему значению (необязательный). Если указан, возвращаются результаты со средним значением не меньше указанного.</param>
        /// <param name="maxAverageValue">Фильтр по максимальному среднему значению (необязательный). Если указан, возвращаются результаты со средним значением не больше указанного.</param>
        /// <param name="minAverageExecutionTime">Фильтр по минимальному среднему времени выполнения (необязательный). Если указан, возвращаются результаты со средним временем выполнения не меньше указанного.</param>
        /// <param name="maxAverageExecutionTime">Фильтр по максимальному среднему времени выполнения (необязательный). Если указан, возвращаются результаты со средним временем выполнения не больше указанного.</param>
        /// <returns>
        /// Асинхронная задача, возвращающая список <see cref="IntegralResultEntity"/>, соответствующий заданным фильтрам.
        /// Возвращает пустой список, если результаты не найдены или фильтры не соответствуют ни одному результату.
        /// </returns>
        public async Task<List<IntegralResultEntity>> GetFilteredResultsAsync(
            string? fileName = null,
            DateTime? minStartDate = null,
            DateTime? maxStartDate = null,
            double? minAverageValue = null,
            double? maxAverageValue = null,
            double? minAverageExecutionTime = null,
            double? maxAverageExecutionTime = null)
        {
            IQueryable<IntegralResultEntity> query = this.Results.AsQueryable(); // Начинаем с выборки всех результатов

            // Применяем фильтры, если они предоставлены
            if (!string.IsNullOrEmpty(fileName))
            {
                query = query.Where(r => r.FileName == fileName); // Фильтр по имени файла
            }

            if (minStartDate.HasValue)
            {
                query = query.Where(r => r.MinDate >= minStartDate.Value); // Фильтр по минимальной дате начала
            }

            if (maxStartDate.HasValue)
            {
                query = query.Where(r => r.MinDate <= maxStartDate.Value); // Фильтр по максимальной дате начала
            }

            if (minAverageValue.HasValue)
            {
                query = query.Where(r => r.AverageValue >= minAverageValue.Value); // Фильтр по минимальному среднему значению
            }

            if (maxAverageValue.HasValue)
            {
                query = query.Where(r => r.AverageValue <= maxAverageValue.Value); // Фильтр по максимальному среднему значению
            }

            if (minAverageExecutionTime.HasValue)
            {
                query = query.Where(r => r.AverageExecutionTime >= minAverageExecutionTime.Value); // Фильтр по минимальному среднему времени выполнения
            }

            if (maxAverageExecutionTime.HasValue)
            {
                query = query.Where(r => r.AverageExecutionTime <= maxAverageExecutionTime.Value); // Фильтр по максимальному среднему времени выполнения
            }

            return await query.ToListAsync(); // Выполняем запрос и возвращаем результаты в виде списка
        }

        /// <summary>
        /// Асинхронно получает последние 10 результатов вычисления для указанного файла, отсортированных по дате начала в порядке убывания (от новых к старым).
        /// </summary>
        /// <param name="fileName">Имя файла, для которого необходимо получить результаты.</param>
        /// <returns>
        /// Асинхронная задача, возвращающая список <see cref="IntegralResultEntity"/>, содержащий последние 10 результатов для указанного файла.
        /// Если результатов меньше 10, возвращает список, содержащий все доступные результаты для данного файла.
        /// Если результаты не найдены, возвращает пустой список.
        /// </returns>
        public async Task<List<CsvDataEntity>> GetLastTenResultsByFileNameAsync(string fileName)
        {
            return await this.Values
                .Where(r => r.FileName == fileName) // Фильтруем по имени файла
                .OrderByDescending(r => r.Date) // Сортируем по дате начала (MinDate) в порядке убывания (от новых к старым)
                .Take(10) // Берем первые 10 результатов (самые новые)
                .ToListAsync(); // Выполняем запрос асинхронно и преобразуем результат в список
        }
    }
}
