using System.Threading.Tasks;

namespace DbSearchLogic.SearchCore.Threading {
    public interface IThreadExecutor {
        void CreateThread(TaskCreationOptions options = TaskCreationOptions.LongRunning);
        void StartThread();
        void Abort();
        Task Task { get; }
        int ThreadPriority { get; }

        string QueryName { get; }
        string Description { get; }
        string CurrentTable { get; }
        string CurrentColumn { get; }
        string TableCount { get; }
        string TimeWorked { get; }
    }
}
