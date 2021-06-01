using System;
using System.Windows.Automation;

namespace ViewBuilderCommon.Test
{
    public class UITestHelper
    {
        private static void ClickButton(AutomationElement profileSelectionUI, string elementName)
        {
            var okButton = profileSelectionUI.FindFirst(TreeScope.Subtree,
                                                        new PropertyCondition(
                                                            AutomationElement.AutomationIdProperty,
                                                            elementName));

            var clickOkButton = okButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
            clickOkButton.Invoke();
        }

        private static SelectionItemPattern GetSelectionItemFromList(AutomationElement list, int listIndex, object controlType)
        {
            AutomationElementCollection listItems = list.FindAll(TreeScope.Children,
                                                                 new PropertyCondition(
                                                                     AutomationElement.ControlTypeProperty,
                                                                     controlType));

            AutomationElement itemToSelect = null;
            if(listItems.Count >0)
            {
                itemToSelect = listItems[listIndex];
            }

            Object selectPattern = null;
            if (itemToSelect != null && itemToSelect.TryGetCurrentPattern(SelectionItemPattern.Pattern, out selectPattern))
            {
                return ((SelectionItemPattern) selectPattern);
            }
            return null;
        }
    }
}