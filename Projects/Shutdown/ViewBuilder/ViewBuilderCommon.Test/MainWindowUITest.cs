using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ViewBuilder;
using ViewBuilder.Windows;
using ViewBuilder.Windows.Controls;
using ViewBuilderBusiness.Manager;
using ViewBuilderBusiness.Structures.Config;
using System.Threading;

namespace ViewBuilderCommon.Test
{
    [TestClass]
    public class MainWindowUiTest : TestUIHelper
    {
        /// <summary>
        /// Simulates user navigating in viewbuilder until the viewTab and selecting an datagrid row + making a CTRL+C
        /// </summary>
        [TestMethod]
        public void Ctrl_C_Keys_On_Datagrid_Should_Fill_The_Clipboard()
        {
            using (Process process = Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ViewBuilder.exe")))
            {
                var sleepCount = 0;
                do
                {
                    Thread.Sleep(10);
                    sleepCount++;
                } while (process == null && sleepCount < 50);

                Thread.Sleep(1500);

                AutomationElement profileSelectionUI = AutomationElement.RootElement;
                AutomationElement lstProfilenames = profileSelectionUI.FindFirst(TreeScope.Subtree,
                                                                   new PropertyCondition(
                                                                       AutomationElement.AutomationIdProperty,
                                                                       "lstProfilenames"));

                AutomationElement list = lstProfilenames.FindFirst(TreeScope.Subtree,
                                                                   new PropertyCondition(
                                                                       AutomationElement.ControlTypeProperty,
                                                                       ControlType.List));
                // select the first item in the list
                var selection = GetSelectionItemFromList(list, 0, ControlType.ListItem);
                if (selection != null) selection.Select();

                ClickButton(profileSelectionUI, "btnOk");

                // leaves some time for the UI to initialize
                Thread.Sleep(2000);

                // response to the popup window
                //SendKeys.SendWait("{TAB}"); // goes to the No button
                SendKeys.SendWait("~"); // ckicks the No button
                Thread.Sleep(5000);

                var tabViews = AutomationElement.RootElement.FindFirst(TreeScope.Subtree,
                                                                       new PropertyCondition(
                                                                           AutomationElement.AutomationIdProperty,
                                                                           "tabViews"));

                var clickTabViews = tabViews.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
                ((SelectionItemPattern) clickTabViews).Select();
                Thread.Sleep(2000);

                AutomationElement dgViews = tabViews.FindFirst(TreeScope.Subtree,
                                                               new PropertyCondition(
                                                                   AutomationElement.AutomationIdProperty, "dgViews"));

                selection = GetSelectionItemFromList(dgViews, 0, ControlType.DataItem);
                if (selection != null) selection.Select();

                Thread.Sleep(1000);
                SendKeys.SendWait("^C"); // sends CTRL+C

                Assert.IsTrue(!string.IsNullOrWhiteSpace(Clipboard.GetText()),
                              "Clipboard doesnt contain the selected row in text format.");

                process.Kill();
            }
        }
    }
}
