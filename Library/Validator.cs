using Library.Entities;

namespace Library
{
    internal class Validator
    {
        private static readonly ushort MIN_ROWS_COUNT, MAX_ROWS_COUNT;

        static Validator()
        {
            MIN_ROWS_COUNT = 1;
            MAX_ROWS_COUNT = 10000;
        }

        public static bool IsCsvDataValid(IEnumerable<CsvDataEntity> csvDatas)
        {
            if ( csvDatas == null || !IsRowsCountValid(csvDatas.Count()) )
                return false;

            foreach (CsvDataEntity csvDataEntity in csvDatas)
            {
                if ( !IsRowValid(csvDataEntity) )
                    return false;
            }
        
            return true;
        }

        private static bool IsRowsCountValid(int rowsCount)
        {
            return (MIN_ROWS_COUNT <= rowsCount && rowsCount <= MAX_ROWS_COUNT);
        }

        private static bool IsRowValid(CsvDataEntity csvDataEntity)
        {
            return (csvDataEntity != null 
                && IsDateValid(csvDataEntity.Date)
                && IsExecTimeValid(csvDataEntity.ExecutionTime) 
                && IsValueValid(csvDataEntity.Value));
        }

        private static bool IsDateValid(DateTime? Date)
        {
            if (Date > DateTime.Now || Date < new DateTime(2000, 01, 01) || Date == null)
                return false;

            return true;
        }

        private static bool IsExecTimeValid(int ExecutionTime)
        {
            return ExecutionTime >= 0;
        }

        private static bool IsValueValid(double Value)
        {
            return Value >= 0;
        }
    }
}
