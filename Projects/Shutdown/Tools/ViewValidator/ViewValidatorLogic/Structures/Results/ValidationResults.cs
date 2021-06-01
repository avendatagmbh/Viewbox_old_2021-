// -----------------------------------------------------------
// Created by Benjamin Held - 15.08.2011 11:41:15
// Copyright AvenDATA 2011
// -----------------------------------------------------------

using System.Collections.Generic;
using ViewValidatorLogic.Structures.InitialSetup;

namespace ViewValidatorLogic.Structures.Results{
    public class ValidationResults  {
        public ValidationSetup Setup{get;private set;}
        
        public Dictionary<TableMapping, TableValidationResult> TableValidationResults { get; private set; }

        public ValidationResults(ValidationSetup setup) {
            this.Setup = setup;
            this.TableValidationResults = new Dictionary<TableMapping, TableValidationResult>();
            //for (int i = 0; i < Setup.TableMappings.Count; ++i) {
            //    if(Setup.TableMappings[i].Used)
            //        TableValidationResults.Add(new TableValidationResult(Setup.TableMappings[i], Setup.ErrorLimit));
            //    else TableValidationResults.Add(null);
            //}
        }

        public int Count { get { return TableValidationResults.Count; } }

        public TableValidationResult this[TableMapping tableMapping]{
            get {
                foreach (var resultKey in TableValidationResults) {
                    if (resultKey.Key.UniqueName == tableMapping.UniqueName)
                        return resultKey.Value;
                }
                TableValidationResults[tableMapping] = new TableValidationResult(tableMapping, Setup.ErrorLimit);
                return TableValidationResults[tableMapping];

                //if (!TableValidationResults.ContainsKey(tableMapping)) 
                //    TableValidationResults[tableMapping] = new TableValidationResult(tableMapping, Setup.ErrorLimit);
                //return TableValidationResults[tableMapping];
            }
        }
    }
}
