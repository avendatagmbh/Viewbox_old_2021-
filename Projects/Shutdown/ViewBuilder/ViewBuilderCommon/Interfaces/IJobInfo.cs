namespace ViewBuilderCommon.Interfaces
{
    public interface IJobInfo
    {
        string Name { get; }
        ViewscriptStates State { get; }
        string DurationDisplayString { get; }
        void StartStopwatch();
        void StopStopwatch();
    }
}