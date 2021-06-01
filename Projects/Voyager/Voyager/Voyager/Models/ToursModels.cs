using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Domain.Entities.Tables;

namespace Voyager.Models {
    public class ToursModels {

        public ToursModels() {
            UserAddress = new Address();
            Clients = new Dictionary <uint,Client>();
            Firm = new UserFirm();
            UserFirmAddress = new Address();
            Dates = new Dictionary<int, Date>();
            newEditedAddresse = new Address();
            Addressbook = new Dictionary<uint, Address>();
            AddressStatebook = new Dictionary<uint, bool>();
        }


        public Dictionary<uint,Client> Clients { get; set;}
        public Address UserAddress { get; set; }
        public UserFirm Firm{ get;set;}
        public Address UserFirmAddress { get; set; }
        public Datelist Datelist { get; set; }
        public Dictionary<int, Date> Dates {get;set;}
        public Address newEditedAddresse { get; set; }
        public Dictionary<uint, Address> Addressbook { get; set; }
        public Dictionary<uint, bool> AddressStatebook { get; set; }

        private int _counter;
        public int Counter {
            get { return _counter; }
            set {
                if (value > 0) {
                    _counter = value;
                }
            }
        }
        public bool EditState { get; set; }
        public DateTime Time { get; set; }

    }
}