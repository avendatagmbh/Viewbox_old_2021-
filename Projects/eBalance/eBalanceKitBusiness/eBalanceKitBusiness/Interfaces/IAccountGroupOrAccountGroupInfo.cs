// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-08-09
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace eBalanceKitBusiness.Interfaces
{
    /// <summary>
    /// class for put AccountGroup and AccountGroupInfo in the same type
    /// the class is for be able to put both AccountGroup and AccountGroupInfo in the same ObservableCollection in AccountGroupingModel.cs
    /// </summary>
    public interface IAccountGroupOrAccountGroupInfo : IsSelectable
    {
    }
}
