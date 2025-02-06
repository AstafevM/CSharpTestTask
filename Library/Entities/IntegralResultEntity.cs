using System.ComponentModel.DataAnnotations;

namespace Library.Entities
{
    /// <summary>
    /// Представляет сущность с результатами интегрального вычисления, агрегированными для конкретного файла.
    /// </summary>
    public class IntegralResultEntity
    {
        /// <summary>
        /// Имя CSV-файла, для которого рассчитаны результаты интегрального вычисления.
        /// Является первичным ключом.
        /// </summary>
        [Key]
        public string FileName { get; set; }

        /// <summary>
        /// Разница во времени между самой ранней и самой поздней датой в исходном CSV-файле, в секундах.
        /// </summary>
        public int TimeDelta { get; set; }

        /// <summary>
        /// Самая ранняя дата и время, найденные в исходном CSV-файле.
        /// </summary>
        public DateTime MinDate { get; set; }

        /// <summary>
        /// Среднее время выполнения операций (или измерений) для данного файла.
        /// </summary>
        public double AverageExecutionTime { get; set; }

        /// <summary>
        /// Среднее значение для данного файла.
        /// </summary>
        public double AverageValue { get; set; }

        /// <summary>
        /// Медианное значение для данного файла.
        /// </summary>
        public double MedianValue { get; set; }

        /// <summary>
        /// Минимальное значение для данного файла.
        /// </summary>
        public double MinValue { get; set; }

        /// <summary>
        /// Максимальное значение для данного файла.
        /// </summary>
        public double MaxValue { get; set; }
    }
}
