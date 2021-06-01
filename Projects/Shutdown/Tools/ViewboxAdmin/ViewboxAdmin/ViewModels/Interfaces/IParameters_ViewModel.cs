using ViewboxAdmin.ViewModels.Collections;

namespace ViewboxAdmin.ViewModels {
    public interface IParameters_ViewModel {
        void Remove(ICollectionModel colletionModel);
        void Edited(ICollectionModel collectionModel);
        void CreateNewCollection(ICollectionModel collectionModel);
    }
}