namespace DbAnalyser.Processing
{
    public enum ProcessDataTypes
    {
        Bool = 0, // text
        Null = 1, // text
        String = 2, // varchar, text
        Date = 3, // date
        Time = 4,
        DateTime = 5,
        Blob = 6, // text / file
        Integer = 7, // int
        Decimal = 8 // double
    }
}
