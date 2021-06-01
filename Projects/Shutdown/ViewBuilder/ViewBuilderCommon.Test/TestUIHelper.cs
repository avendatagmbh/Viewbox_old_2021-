using System;
using System.Windows.Automation;

namespace ViewBuilderCommon.Test
{
    public class TestUIHelper
    {
        /// <summary>
        /// simulates a click event on the element passed as argument withing the subtree of parentUIElement
        /// </summary>
        /// <param name="parentUIElement"></param>
        /// <param name="elementName"></param>
        protected static void ClickButton(AutomationElement parentUIElement, string elementName)
        {
            var okButton = parentUIElement.FindFirst(TreeScope.Subtree,
                                                        new PropertyCondition(
                                                            AutomationElement.AutomationIdProperty,
                                                            elementName));

            var buttonClick = okButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
            if (buttonClick != null) buttonClick.Invoke();
        }
        /// <summary>
        /// Returns the itemlist at the index of listIndex (arg) from a list containing childrens of type controlType (arg).
        /// </summary>
        /// <param name="list"></param>
        /// <param name="listIndex"></param>
        /// <param name="controlType"></param>
        /// <returns></returns>
        protected static SelectionItemPattern GetSelectionItemFromList(AutomationElement list, int listIndex, object controlType)
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