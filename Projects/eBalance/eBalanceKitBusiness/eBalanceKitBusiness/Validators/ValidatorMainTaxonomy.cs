// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-21
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Taxonomy.Enums;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Validators {
    public class ValidatorMainTaxonomy : IValidator {

        internal ValidatorMainTaxonomy(Document document) {

            Document = document;

            switch (document.MainTaxonomyInfo.Type) {
                case TaxonomyType.Unknown:
                    throw new NotImplementedException();
                    
                case TaxonomyType.GCD:
                    throw new NotImplementedException();
                    
                case TaxonomyType.GAAP:
                    Validator = new ValidatorGAAP(document);
                    break;
                
                case TaxonomyType.OtherBusinessClass:
                    //Validator = new ValidatorBra(document);
                    Validator = new ValidatorGAAP(document);
                    break;
                
                case TaxonomyType.Financial:
                    //Validator = new ValidatorFi(document);
                    Validator = new ValidatorGAAP(document);
                    break;
                
                case TaxonomyType.Insurance:
                    //Validator = new ValidatorIns(document);
                    Validator = new ValidatorGAAP(document);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected Document Document { get; set; }

        private IValidator Validator { get; set; }
        
        public bool Validate() { return Validator.Validate(); }
        public List<string> GeneralErrorMessages { get { return Validator.GeneralErrorMessages; } }
        public void ResetValidationValues() { Validator.ResetValidationValues(); }
    }
}