// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-09-20
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using DbAccess;

namespace TransDATA.TemplateSelectors {
    public class ConnectionStringParameterSelector : DataTemplateSelector {
        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container) {
            var cp = container as ContentPresenter;
            var p = cp.Content as IConnectionStringParam;
            if (p == null) return null;
            switch (p.Type) {
                case ConnectionStringParamType.String:
                    return cp.FindResource("StringParameter") as DataTemplate;

                case ConnectionStringParamType.Integer:
                    return cp.FindResource("IntegerParameter") as DataTemplate;

                case ConnectionStringParamType.Boolean:
                    return cp.FindResource("BooleanParameter") as DataTemplate;

                case ConnectionStringParamType.Password:
                    return cp.FindResource("PasswordParameter") as DataTemplate;

                default:
                    return null;
            }
        }
    }
}