using Business.Interfaces;
using DbAccess.Interaction;

namespace Business.Interfaces {
    public interface ITransferTableProgressWrapper {
        ITransferTableProgress Progress { get; set; }
    }
}