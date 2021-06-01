namespace Utils
{
    public interface IProgressCalculator
    {
        string Description { get; set; }
        void SetWorkSteps(long num, bool hasChildren);
        void StepDone();
    }
}