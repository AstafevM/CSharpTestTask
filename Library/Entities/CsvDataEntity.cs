using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Entities
{
    /// <summary>
    /// Представляет сущность данных, полученную из CSV-файла.
    /// </summary>
    public class CsvDataEntity
    {
        /// <summary>
        /// Уникальный идентификатор записи.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Дата и время записи.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Время выполнения операции в секундах.
        /// </summary>
        public int ExecutionTime { get; set; }

        /// <summary>
        /// Значение записи.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Имя CSV-файла, из которого была загружена запись.
        /// </summary>
        public string FileName { get; set; }
    }
}
