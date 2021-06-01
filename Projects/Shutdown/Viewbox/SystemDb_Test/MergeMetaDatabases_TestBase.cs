using System;
using System.Collections.Generic;
using SystemDb.Internal;

namespace SystemDb_Test
{
    internal class MergeMetaDatabases_TestBase
    {
        public Dictionary<string, Type> tablenameType;

        public MergeMetaDatabases_TestBase()
        {
            FillUpDictionary();
        }

        protected void FillUpDictionary()
        {
            tablenameType = new Dictionary<string, Type>();
            tablenameType.Add("parameter_texts", typeof (ParameterText));
            tablenameType.Add("collection_texts", typeof (ParameterValueSetText));
            tablenameType.Add("order_areas", typeof (OrderArea));
            tablenameType.Add("issue_extensions", typeof (IssueExtension));
            tablenameType.Add("parameter", typeof (Parameter));
            tablenameType.Add("parameter_collections", typeof (ParameterCollectionMapping));
            tablenameType.Add("collections", typeof (ParameterValue));
            tablenameType.Add("optimization_texts", typeof (OptimizationText));
            tablenameType.Add("optimization_groups", typeof (OptimizationGroup));
            tablenameType.Add("optimization_group_texts", typeof (OptimizationGroupText));
            tablenameType.Add("tables", typeof (TableObject));
            tablenameType.Add("table_roles", typeof (TableRoleMapping));
            tablenameType.Add("category_roles", typeof (CategoryRoleMapping));
            tablenameType.Add("optimization_roles", typeof (OptimizationRoleMapping));
            tablenameType.Add("optimizations", typeof (Optimization));
            tablenameType.Add("optimization_users", typeof (OptimizationUserMapping));
            tablenameType.Add("table_texts", typeof (TableObjectText));
            tablenameType.Add("columns", typeof (Column));
            tablenameType.Add("categories", typeof (Category));
            tablenameType.Add("table_users", typeof (TableUserMapping));
            tablenameType.Add("roles", typeof (Role));
            tablenameType.Add("user_roles", typeof (UserRoleMapping));
            tablenameType.Add("users", typeof (User));
            tablenameType.Add("column_texts", typeof (ColumnText));
            tablenameType.Add("relations", typeof (Relation));
            tablenameType.Add("category_texts", typeof (CategoryText));
            tablenameType.Add("column_roles", typeof (ColumnRoleMapping));
            tablenameType.Add("column_users", typeof (ColumnUserMapping));
            tablenameType.Add("category_users", typeof (CategoryUserMapping));
        }
    }
}