// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-08-07
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace eBalanceKitBusiness.MappingTemplate
{
    public class AccountGroupChildInfo
    {
        public AccountGroupChildInfo() { 
            IsSelected = false;
            IsVisible = true;
        }

        public bool IsSelected { get; set; }

        public bool IsVisible { get; set; }

        public string Name { get; set; }

        public string Number { get; set; }

        public string Comment { get; set; }

        // used in TaxonomyAndBalanceListBase.xaml
        public bool HasComment { get { return string.IsNullOrEmpty(Comment); } }

        public string DisplayString { get { return (Number != null ? Number + " - " : "") + Name; } }

        /// <summary>
        /// at drag & drop effect the account can be dropped on a child info, but the parent info have to be changed.
        /// the DataContext is the child info, so we have to store the reference of the parent.
        /// The drag and drop is in the DpgApplyGroupingTemplate.xaml
        /// </summary>
        public AccountGroupInfo Parent { get; set; }
    }
}
