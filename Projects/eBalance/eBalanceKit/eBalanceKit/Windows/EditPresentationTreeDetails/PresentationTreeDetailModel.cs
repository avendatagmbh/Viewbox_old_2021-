// (C)opyright 2011 AvenDATA GmbH
// ----------------------------------------
// author: Mirko Dibbert
// since : 2011-09-04

using System;
using Taxonomy.Enums;
using eBalanceKitBusiness;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.EditPresentationTreeDetails {
    public class PresentationTreeDetailModel {

        internal PresentationTreeDetailModel(Document document, IValueTreeEntry value) {
            Document = document;
            Value = value;
        }

        public Document Document { get; private set; }

        public IValueTreeEntry Value { get; private set; }
    }
}