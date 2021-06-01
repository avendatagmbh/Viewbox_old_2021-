using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AutomatedInputHandler.Helper;
using System.Runtime.InteropServices;
using System.Diagnostics;
using AutomatedInputHandler.Events;
using System.IO;
using System.Threading;
using System.Configuration;
using System.Xml;
using System.Xml.Serialization;
using System.Threading.Tasks;

namespace AutomatedInputHandler
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeFormControls();
        }

        private void InitializeFormControls()
        {
            tbxClassName.Text = ConfigurationManager.AppSettings[Enums.ConfigEnums.windowclassname.ToString()];
            tbxWindowName.Text = ConfigurationManager.AppSettings[Enums.ConfigEnums.windowname.ToString()];
            tbxScriptToSave.Text = ConfigurationManager.AppSettings[Enums.ConfigEnums.saveactionpath.ToString()];
            tbxScriptToLoad.Text = ConfigurationManager.AppSettings[Enums.ConfigEnums.loadactionpath.ToString()];
            tbxLoadDataConfig.Text = ConfigurationManager.AppSettings[Enums.ConfigEnums.loaddataconfig.ToString()];
            tbxSaveDataConfig.Text = ConfigurationManager.AppSettings[Enums.ConfigEnums.savedataconfig.ToString()];
        }

        private void btnBuildWindoTree_Click(object sender, EventArgs e)
        {
            tvTree.Nodes.Clear();

            foreach (Process procesInfo in Process.GetProcesses().Where(
                x => x.ProcessName.ToLower().StartsWith(tbxWindowName2.Text.ToLower())))
            {
                tvTree.Nodes.Add("process {0} {1:x}", procesInfo.ProcessName, procesInfo.Id);

                foreach (ProcessThread threadInfo in procesInfo.Threads)
                {
                    // uncomment to dump thread handles
                    //Console.WriteLine("\tthread {0:x}", threadInfo.Id);
                    IntPtr[] windows = GetWindowHandlesForThread(threadInfo.Id);

                    if (windows != null && windows.Length > 0)
                    {
                        foreach (IntPtr hWnd in windows)
                        {
                            tvTree.Nodes[tvTree.Nodes.Count - 1].Nodes.Add(
                                string.Format("\twindow {0:x} text:{1} caption:{2}",
                                hWnd.ToInt32(), GetText(hWnd), GetEditText(hWnd)));

                            List<IntPtr> Children = GetAllChildrenWindowHandles(hWnd, 100);

                            TreeNode m_LastNode = tvTree.Nodes[tvTree.Nodes.Count - 1];

                            foreach (IntPtr hWnd2 in Children)
                            {
                                m_LastNode.Nodes[m_LastNode.Nodes.Count - 1].Nodes.Add(
                                    string.Format("\twindow {0:x} text:{1} caption:{2}",
                                    hWnd2.ToInt32(), GetText(hWnd2), GetEditText(hWnd2)));
                            }
                        }
                    }
                }
            }

            tvTree.Sort();
        }


        #region [ 1. ]
        private static IntPtr[] GetWindowHandlesForThread(int threadHandle)
        {
            _results.Clear();
            EnumWindows(WindowEnum, threadHandle);
            return _results.ToArray();
        }

        // enum windows

        private delegate int EnumWindowsProc(IntPtr hwnd, int lParam);

        [DllImport("user32.Dll")]
        private static extern int EnumWindows(EnumWindowsProc x, int y);
        [DllImport("user32")]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowsProc callback, int lParam);
        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        private static List<IntPtr> _results = new List<IntPtr>();

        private static int WindowEnum(IntPtr hWnd, int lParam)
        {
            int processID = 0;
            int threadID = GetWindowThreadProcessId(hWnd, out processID);
            if (threadID == lParam)
            {
                _results.Add(hWnd);
                EnumChildWindows(hWnd, WindowEnum, threadID);
            }
            return 1;
        }

        // get window text

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        private static string GetText(IntPtr hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        // get richedit text 

        public const int GWL_ID = -12;
        public const int WM_GETTEXT = 0x000D;

        [DllImport("User32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int index);
        [DllImport("User32.dll")]
        public static extern IntPtr SendDlgItemMessage(IntPtr hWnd, int IDDlgItem, int uMsg, int nMaxCount, StringBuilder lpString);
        [DllImport("User32.dll")]
        public static extern IntPtr GetParent(IntPtr hWnd);

        private static StringBuilder GetEditText(IntPtr hWnd)
        {
            Int32 dwID = GetWindowLong(hWnd, GWL_ID);
            IntPtr hWndParent = GetParent(hWnd);
            StringBuilder title = new StringBuilder(128);
            SendDlgItemMessage(hWndParent, dwID, WM_GETTEXT, 128, title);
            return title;
        }

        static List<IntPtr> GetAllChildrenWindowHandles(IntPtr hParent, int maxCount)
        {
            List<IntPtr> result = new List<IntPtr>();
            int ct = 0;
            IntPtr prevChild = IntPtr.Zero;
            IntPtr currChild = IntPtr.Zero;
            while (true && ct < maxCount)
            {
                currChild = DLLImport.FindWindowEx(hParent, prevChild, null, null);
                if (currChild == IntPtr.Zero) break;
                result.Add(currChild);
                prevChild = currChild;
                ++ct;
            }
            return result;
        }

        #endregion

        List<AutomatedEventBase> AutomatedEvents;

        //Clear all.
        private void button2_Click(object sender, EventArgs e)
        {
            ddlEventType.SelectedIndex = -1;
            tbxPositionX.Text = string.Empty;
            tbxPositionY.Text = string.Empty;
            tbxTextToEnter.Text = string.Empty;
            tbxWaitTime.Text = string.Empty;
            tbxIterationCounter.Text = string.Empty;
            tbxIterationID.Text = string.Empty;
        }

        //Clear text to enter textbox.
        private void button1_Click(object sender, EventArgs e)
        {
            tbxTextToEnter.Text = string.Empty;
        }

        //Clears all the actions.
        private void button4_Click(object sender, EventArgs e)
        {
            lbAutomatedEvents.Items.Clear();

            AutomatedEvents = new List<AutomatedEventBase>();
        }

        //Add event
        private void button3_Click(object sender, EventArgs e)
        {
            if (AutomatedEvents == null)
            {
                lbAutomatedEvents.Items.Clear();
                AutomatedEvents = new List<AutomatedEventBase>();
            }

            AutomatedEventBase m_Event = CreateEvent();

            if (m_Event != null)
            {
                AutomatedEvents.Add(m_Event);
                lbAutomatedEvents.Items.Add(m_Event.GetString());
            }
        }

        //Insert event
        private void button15_Click(object sender, EventArgs e)
        {
            if (AutomatedEvents == null)
            {
                lbAutomatedEvents.Items.Clear();
                AutomatedEvents = new List<AutomatedEventBase>();
            }

            AutomatedEventBase m_Event = CreateEvent();

            if (m_Event != null)
            {
                if (lbAutomatedEvents.SelectedIndex > -1)
                {
                    AutomatedEvents.Insert(lbAutomatedEvents.SelectedIndex, m_Event);
                    lbAutomatedEvents.Items.Insert(lbAutomatedEvents.SelectedIndex, m_Event.GetString());
                }
                else
                {
                    AutomatedEvents.Add(m_Event);
                    lbAutomatedEvents.Items.Add(m_Event.GetString());
                }
            }
        }

        private Enums.EventExecutionTypeEnum GetExecutionType()
        {
            if (rbtnPreEvent.Checked)
                return Enums.EventExecutionTypeEnum.PreEvent;

            if (rbtnPostEvent.Checked)
                return Enums.EventExecutionTypeEnum.PostEvent;

            if (rbtnDataEvent.Checked)
                return Enums.EventExecutionTypeEnum.DataEvent;

            throw new Exception("Cannot get execution event type!");
        }

        private Enums.EventExecutionTypeEnum GetExecutionType(string p_Text)
        {
            p_Text = p_Text.Substring(p_Text.IndexOf("Exec. Type: ") + 12);
            p_Text = p_Text.Substring(0, p_Text.IndexOf(";"));

            if (p_Text == "PreEvent")
                return Enums.EventExecutionTypeEnum.PreEvent;

            if (p_Text == "PostEvent")
                return Enums.EventExecutionTypeEnum.PostEvent;

            if (p_Text == "DataEvent")
                return Enums.EventExecutionTypeEnum.DataEvent;

            throw new Exception("Cannot get execution type!");
        }

        private AutomatedEventBase CreateEvent()
        {
            AutomatedEventBase m_Event = null;

            if (ddlEventType.SelectedIndex > -1)
            {
                switch (ddlEventType.Items[ddlEventType.SelectedIndex].ToString())
                {
                    case "EnterText":

                        if (!string.IsNullOrWhiteSpace(tbxTextToEnter.Text))
                        {
                            m_Event = new AutomatedEventEnterText(
                                tbxTextToEnter.Text);
                            m_Event.EventType = Enums.EventTypeEnum.EnterText;
                            m_Event.EventExecutionType = GetExecutionType();
                        }

                        break;
                    case "Iteration":

                        if (!string.IsNullOrWhiteSpace(tbxIterationCounter.Text) &&
                            !string.IsNullOrWhiteSpace(tbxIterationID.Text))
                        {
                            int m_IterationCounter, m_ID;

                            if (Int32.TryParse(tbxIterationCounter.Text, out m_IterationCounter) &&
                            Int32.TryParse(tbxIterationID.Text, out m_ID))
                            {
                                m_Event = new AutomatedEventIteration(m_IterationCounter, m_ID);
                                m_Event.EventType = Enums.EventTypeEnum.Iteration;
                                m_Event.EventExecutionType = GetExecutionType();
                            }
                        }

                        break;
                    case "MoveAndClick":

                        if (!string.IsNullOrWhiteSpace(tbxPositionX.Text) &&
                            !string.IsNullOrWhiteSpace(tbxPositionY.Text))
                        {
                            int m_PosX, m_PosY;

                            if (Int32.TryParse(tbxPositionX.Text, out m_PosX) &&
                            Int32.TryParse(tbxPositionY.Text, out m_PosY))
                            {
                                m_Event = new AutomatedEventMoveAndClick(
                                    m_PosX, m_PosY);
                                m_Event.EventType = Enums.EventTypeEnum.MoveAndClick;
                                m_Event.EventExecutionType = GetExecutionType();
                            }
                        }

                        break;
                    case "Wait":
                        if (!string.IsNullOrWhiteSpace(tbxWaitTime.Text))
                        {
                            int m_WaitTime;

                            if (Int32.TryParse(tbxWaitTime.Text, out m_WaitTime))
                            {
                                m_Event = new AutomatedEventWait(
                                    m_WaitTime);
                                m_Event.EventType = Enums.EventTypeEnum.Wait;
                                m_Event.EventExecutionType = GetExecutionType();
                            }
                        }
                        break;
                }
            }

            return m_Event;
        }

        private void RunEvents(List<AutomatedEventBase> p_Events)
        {
            #region [ Logic...What did you think? ]
            if (!string.IsNullOrWhiteSpace(tbxClassName.Text) ||
                    !string.IsNullOrWhiteSpace(tbxWindowName.Text))
            {
                DLLImport.SetForegroundWindow(
                    DLLImport.FindWindow(
                    tbxClassName.Text,
                    tbxWindowName.Text));
            }

            List<AutomatedEventBase> m_PreEvents =
                p_Events.Where(x => x.EventExecutionType == Enums.EventExecutionTypeEnum.PreEvent).ToList();
            List<AutomatedEventBase> m_DataEvents =
                p_Events.Where(x => x.EventExecutionType == Enums.EventExecutionTypeEnum.DataEvent).ToList();
            List<AutomatedEventBase> m_PostEvents =
                p_Events.Where(x => x.EventExecutionType == Enums.EventExecutionTypeEnum.PostEvent).ToList();

            AutomatedEventBase[] m_Copy;

            TokenSource = new CancellationTokenSource();
            Token = TokenSource.Token;

            try
            {
                RunTask = Task.Factory.StartNew(() =>
                {
                    if (!Token.IsCancellationRequested)
                    {
                        m_Copy = new AutomatedEventBase[m_PreEvents.Count];
                        m_PreEvents.CopyTo(m_Copy);

                        Run(m_Copy.ToList());
                    }

                    if (!Token.IsCancellationRequested)
                    {
                        BuildDataConfig();

                        m_Copy = new AutomatedEventBase[m_DataEvents.Count];
                        m_DataEvents.CopyTo(m_Copy);

                        foreach (Company company in CurrentDataConfig.Companies)
                        {
                            if (Token.IsCancellationRequested)
                                break;

                            DataConfig m_Config = new DataConfig();
                            m_Config.Companies = new List<Company>();
                            m_Config.Companies.Add(company);
                            m_Config.AllCompaniesCount = CurrentDataConfig.AllCompaniesCount;

                            if (company.Years != null && company.Years.Count > 0)
                            {
                                foreach (Year year in company.Years)
                                {
                                    if (Token.IsCancellationRequested)
                                        break;

                                    m_Config.Companies[0].Years = new List<Year>();
                                    m_Config.Companies[0].Years.Add(year);
                                    m_Config.Companies[0].AllYearsCount = company.AllYearsCount;

                                    if (year.Periods != null && year.Periods.Count > 0)
                                    {
                                        foreach (Period period in year.Periods)
                                        {
                                            if (Token.IsCancellationRequested)
                                                break;

                                            m_Config.Companies[0].Years[0].Periods = new List<Period>();
                                            m_Config.Companies[0].Years[0].Periods.Add(period);
                                            m_Config.Companies[0].Years[0].AllPeriodsCount = year.AllPeriodsCount;

                                            Run(m_Copy.ToList(), m_Config);
                                        }
                                    }
                                    else
                                        Run(m_Copy.ToList(), m_Config);
                                }
                            }
                            else
                                Run(m_Copy.ToList(), m_Config);
                        }
                    }

                    if (!Token.IsCancellationRequested)
                    {
                        m_Copy = new AutomatedEventBase[m_PostEvents.Count];
                        m_PostEvents.CopyTo(m_Copy);

                        Run(m_Copy.ToList());

                    }
                }, Token);
            }
            catch (OperationCanceledException ex1)
            {
                MessageBox.Show(ex1.Message);
            }
            catch (Exception ex2)
            {
                MessageBox.Show(ex2.Message);
            } 
            #endregion
        }

        //Runs all the events.
        private void button5_Click(object sender, EventArgs e)
        {
            RunEvents(AutomatedEvents);            
        }

        //Runs the selected event.
        private void button11_Click(object sender, EventArgs e)
        {
            List<AutomatedEventBase> m_Event = new List<AutomatedEventBase>();
            int[] m_SelectedIndices = GetSelectedIndices();

            for (int i = 0; i < m_SelectedIndices.Count(); i++)
            {
                m_Event.Add(AutomatedEvents[m_SelectedIndices[i]]);
            }

            RunEvents(m_Event);
        }

        private void Run(List<AutomatedEventBase> p_EventsToRun, DataConfig p_DataConfig = null)
        {
            try
            {
                int m_NextEventPosition = 0;

                while (m_NextEventPosition < p_EventsToRun.Count && !Token.IsCancellationRequested)
                {
                    //if (Token.IsCancellationRequested)
                    //    Token.ThrowIfCancellationRequested();

                    if (p_EventsToRun[m_NextEventPosition] is AutomatedEventIteration)
                    {
                        #region [ Iteration ]
                        int m_IterationCounter = (p_EventsToRun[m_NextEventPosition] as AutomatedEventIteration).IterationCounter;
                        int m_ID = (p_EventsToRun[m_NextEventPosition] as AutomatedEventIteration).ID;

                        List<AutomatedEventBase> m_Iteration = new List<AutomatedEventBase>();

                        //Remove the opening iteration event.
                        p_EventsToRun.RemoveAt(m_NextEventPosition);

                        do
                        {
                            m_Iteration.Add(p_EventsToRun[m_NextEventPosition]);
                            p_EventsToRun.RemoveAt(m_NextEventPosition);
                        }
                        while (
                            !(p_EventsToRun[m_NextEventPosition] is AutomatedEventIteration) ||
                            (p_EventsToRun[m_NextEventPosition] as AutomatedEventIteration).ID != m_ID);

                        //Remove the closing iteration event.
                        p_EventsToRun.RemoveAt(m_NextEventPosition);

                        //m_NextEventPosition++;

                        for (int i = 0; i < m_IterationCounter; i++)
                        {
                            AutomatedEventBase[] m_Copy = new AutomatedEventBase[m_Iteration.Count];
                            m_Iteration.CopyTo(m_Copy);

                            Run(m_Copy.ToList());
                        } 
                        #endregion
                    }
                    else if (p_EventsToRun[m_NextEventPosition] is AutomatedEventEnterText)
                    {
                        #region [ Enter text ]
                        IntPtr keyboardLayout = DLLImport.GetKeyboardLayout(0);

                        string m_text = (p_EventsToRun[m_NextEventPosition] as AutomatedEventEnterText).TextToEnter;

                        //m_text is the whole text i want to write. It may contain special characters, like 'enter', 'tab',
                        //lower/upper-case chars, and chars with shit/alt is pressed, like ';'.
                        while (!string.IsNullOrWhiteSpace(m_text))
                        {
                            int m_Index = 0;

                            //Enter, tab and similar keys are in {} brackets (for example {tab}). We get the 'tab'
                            //from the string and pass this to our method. Key combinations are separated by a '+'
                            //like {alt+tab+tab}, from this we will get 'press the alt, then press the tab, then
                            //press the tab again'.
                            if (m_text[m_Index] == '{' && m_text.IndexOf("}") > -1)
                            {
                                #region [ Special chars ]
                                string m_SubString = m_text.Substring(
                                                            m_Index + 1, m_text.IndexOf("}") - 1);

                                string[] m_Splitted = m_SubString.Split(new char[] { '+' });

                                if (m_Splitted.Length == 1 && m_Splitted[0][0] == '#')
                                {
                                    #region [ DataConfig value ]

                                    string m_DataText = string.Empty;

                                    if (m_Splitted[0].ToLower() == "#company" && p_DataConfig.Companies != null)
                                    {
                                        m_DataText = p_DataConfig.Companies[0].Identifier;
                                    }
                                    else if (m_Splitted[0].ToLower() == "#year" &&
                                        p_DataConfig.Companies != null && p_DataConfig.Companies[0].Years != null)
                                    {
                                        m_DataText = p_DataConfig.Companies[0].Years[0].year;
                                    }
                                    else if (m_Splitted[0].ToLower() == "#period" &&
                                        p_DataConfig.Companies != null && p_DataConfig.Companies[0].Years != null &&
                                        p_DataConfig.Companies[0].Years[0].Periods != null)
                                    {
                                        m_DataText = p_DataConfig.Companies[0].Years[0].Periods[0].period;
                                    }

                                    if (!string.IsNullOrWhiteSpace(m_DataText))
                                    {
                                        foreach(char c in m_DataText.ToCharArray())
                                        {
                                            PressOneKey(c, keyboardLayout);
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region [ Key-combination ]
                                    for (int i = 0; i < m_Splitted.Length; i++)
                                    {
                                        //If the string is longer than 1 char it means we are processing a tab-like key.
                                        if (m_Splitted[i].Length > 1)
                                            PressSpecial(m_Splitted[i]);
                                        else
                                        {
                                            //If the char is 1-char-long, it means we previously pressed a tab-like key,
                                            //and now we press a simple key, like in the case of {altgr+w}.

                                            //Get the virtual key of the char.
                                            short vKey = DLLImport.VkKeyScanEx(
                                                char.Parse(m_Splitted[i]), keyboardLayout);

                                            //Get the low byte from the virtual key.
                                            byte m_LOWBYTE = (Byte)(vKey & 0xFF);

                                            //Get the scan code of the key.
                                            byte sScan = (byte)DLLImport.MapVirtualKey(m_LOWBYTE, 0);

                                            //Press the key.
                                            //Key down event, as indicated by the 3rd parameter that is 0.
                                            DLLImport.keybd_event(m_LOWBYTE, sScan, 0, 0);
                                        }
                                    }

                                    Application.DoEvents();

                                    //We have pressed all the keys we wanted, now release them in backward-order
                                    //when pressing alt+tab we beed to release them in tab-alt order! The logic is the same.
                                    for (int i = m_Splitted.Length - 1; i > -1; i--)
                                    {
                                        if (m_Splitted[i].Length > 1)
                                            ReleaseSpecial(m_Splitted[i]);
                                        else
                                        {
                                            short vKey = DLLImport.VkKeyScanEx(
                                                char.Parse(m_Splitted[i]), keyboardLayout);

                                            byte m_LOWBYTE = (Byte)(vKey & 0xFF);

                                            byte sScan = (byte)DLLImport.MapVirtualKey(m_LOWBYTE, 0);

                                            //Key up event, as indicated by the 3rd parameter that is 0x0002.
                                            DLLImport.keybd_event(m_LOWBYTE, sScan, 0x0002, 0); //Key up
                                        }
                                    }

                                    Application.DoEvents();
                                    #endregion

                                    if (m_SubString == "shift+tab" && p_DataConfig.Companies[0].AllYearsCount > 1)
                                    {
                                        Thread.Sleep(500);

                                        if (p_DataConfig.Companies[0].AllYearsCount >
                                            p_DataConfig.Companies[0].Years[0].Index)
                                        {
                                            DLLImport.SendMouseMove(new Point(900, 910));
                                            DLLImport.SendMouseLeftClick(); 

                                            Thread.Sleep(500);

                                            if (p_DataConfig.Companies[0].Index == 1 &&
                                                p_DataConfig.Companies[0].Years[0].Index == 1)
                                            {
                                                DLLImport.SendMouseMove(new Point(900, 910));
                                                DLLImport.SendMouseLeftClick();

                                                Thread.Sleep(500);                                                
                                            }

                                            DLLImport.SendMouseMove(new Point(900, 890));
                                            DLLImport.SendMouseLeftClick(); 

                                            Thread.Sleep(500);
                                        }
                                        else
                                        {
                                            DLLImport.SendMouseMove(new Point(900, 890));
                                            DLLImport.SendMouseLeftClick(); 

                                            Thread.Sleep(500);

                                            DLLImport.SendMouseMove(new Point(900, 910));
                                            DLLImport.SendMouseLeftClick();

                                            Thread.Sleep(500);
                                        }
                                    }
                                }
                                #endregion

                                //We do not use the '{' and '}' brackets, thats why the '+2'. :)
                                m_Index = m_SubString.Length + 2;
                            }
                            else
                            {
                                PressOneKey(m_text[m_Index], keyboardLayout);                                

                                //Get the next char from the string.
                                m_Index++;
                            }

                            //Remove the already processed chars from the string.
                            if (m_Index < m_text.Length)
                                m_text = m_text.Substring(m_Index);
                            else
                                m_text = string.Empty;
                        } 
                        #endregion

                        m_NextEventPosition++;
                    }
                    else if (p_EventsToRun[m_NextEventPosition] is AutomatedEventMoveAndClick)
                    {
                        #region [ Move and click ]
                        DLLImport.SendMouseMove(new Point(
                                            (p_EventsToRun[m_NextEventPosition] as AutomatedEventMoveAndClick).PositionX,
                                            (p_EventsToRun[m_NextEventPosition] as AutomatedEventMoveAndClick).PositionY));

                        DLLImport.SendMouseLeftClick(); 
                        #endregion

                        m_NextEventPosition++;
                    }
                    else if (p_EventsToRun[m_NextEventPosition] is AutomatedEventWait)
                    {
                        Thread.Sleep((p_EventsToRun[m_NextEventPosition] as AutomatedEventWait).WaitTimeInMilliSeconds);

                        m_NextEventPosition++;
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
            }
        }

        private void PressOneKey(char p_Key, IntPtr p_keyboardLayout)
        {
            #region [ One char ]
            short vKey = DLLImport.VkKeyScanEx(p_Key, p_keyboardLayout);

            //Hi-byte indicates if we need to press shift, alt or other similar keys.
            byte m_HIBYTE = (Byte)(vKey >> 8);
            byte m_LOWBYTE = (Byte)(vKey & 0xFF);

            byte sScan = (byte)DLLImport.MapVirtualKey(m_LOWBYTE, 0);

            //Press the special key if needed.
            if ((m_HIBYTE == 1))
                PressShift();
            else if ((m_HIBYTE == 2))
                PressControl();
            else if ((m_HIBYTE == 4))
                PressAlt();
            else if ((m_HIBYTE == 6))
                PressAltGr();

            //Press, then release the key.
            DLLImport.keybd_event(m_LOWBYTE, sScan, 0, 0); //Key down
            DLLImport.keybd_event(m_LOWBYTE, sScan, 0x0002, 0); //Key up

            //Release the special key if needed.
            if ((m_HIBYTE == 1))
                ReleaseShift();
            else if ((m_HIBYTE == 2))
                ReleaseControl();
            else if ((m_HIBYTE == 4))
                ReleaseAlt();
            else if ((m_HIBYTE == 6))
                ReleaseAltGr();
            #endregion
        }

        #region [ Press shift ]
        private void PressShift()
        {
            DLLImport.keybd_event(0xA0, 0x2A, 0, 0);
        } 
        #endregion

        #region [ Release shift ]
        private void ReleaseShift()
        {
            DLLImport.keybd_event(0xA0, 0x2A, 0x0002, 0);
        } 
        #endregion

        #region [ Press control ]
        private void PressControl()
        {
            DLLImport.keybd_event(0xA2, 0x1D, 0, 0);
        } 
        #endregion

        #region [ Release control ]
        private void ReleaseControl()
        {
            DLLImport.keybd_event(0xA2, 0x1D, 0x0002, 0);
        }
        #endregion

        #region [ Press altgr ]
        private void PressAltGr()
        {
            DLLImport.keybd_event(0xA5, 0x38, 0, 0);
        }
        #endregion

        #region [ Release altgr ]
        private void ReleaseAltGr()
        {
            DLLImport.keybd_event(0xA5, 0x38, 0x0002, 0);
        }
        #endregion

        #region [ Press alt ]
        private void PressAlt()
        {
            DLLImport.keybd_event(0x12, 0x38, 0, 0);
        }
        #endregion

        #region [ Release alt ]
        private void ReleaseAlt()
        {
            DLLImport.keybd_event(0x12, 0x38, 0x0002, 0);
        }
        #endregion

        #region [ Press special ]
        private void PressSpecial(string p_Special)
        {
            switch (p_Special.ToLower())
            {
                case "home":
                    DLLImport.keybd_event(0x24, 0x47, 0, 0);
                    break;
                case "end":
                    DLLImport.keybd_event(0x23, 0x4F, 0, 0);
                    break;
                case "pagedown":
                    DLLImport.keybd_event(0x22, 0x51, 0, 0);
                    break;
                case "pageup":
                    DLLImport.keybd_event(0x21, 0x49, 0, 0);
                    break;
                case "backspace":
                    DLLImport.keybd_event(0x08, 0x0e, 0, 0);
                    break;
                case "esc":
                    DLLImport.keybd_event(0x1B, 0x01, 0, 0);
                    break;
                case "tab":
                    DLLImport.keybd_event(0x09, 0x0f, 0, 0);
                    break;
                case "enter":
                    DLLImport.keybd_event(0x0D, 0x1C, 0, 0);
                    break;
                case "shift":
                    PressShift();
                    break;
                case "control":
                    PressControl();
                    break;
                case "alt":
                    PressAlt();
                    break;
                case "altgr":
                    PressAltGr();
                    break;
                case "f1":
                    DLLImport.keybd_event(0x70, 0x59, 0, 0);
                    break;
                case "f2":
                    DLLImport.keybd_event(0x71, 0x60, 0, 0);
                    break;
                case "f3":
                    DLLImport.keybd_event(0x72, 0x61, 0, 0);
                    break;
                case "f4":
                    DLLImport.keybd_event(0x73, 0x62, 0, 0);
                    break;
                case "f5":
                    DLLImport.keybd_event(0x74, 0x63, 0, 0);
                    break;
                case "f6":
                    DLLImport.keybd_event(0x75, 0x64, 0, 0);
                    break;
                case "f7":
                    DLLImport.keybd_event(0x76, 0x65, 0, 0);
                    break;
                case "f8":
                    DLLImport.keybd_event(0x77, 0x66, 0, 0);
                    break;
                case "f9":
                    DLLImport.keybd_event(0x78, 0x67, 0, 0);
                    break;
                case "f10":
                    DLLImport.keybd_event(0x79, 0x68, 0, 0);
                    break;
                case "f11":
                    DLLImport.keybd_event(0x7A, 0x87, 0, 0);
                    break;
                case "f12":
                    DLLImport.keybd_event(0x7B, 0x88, 0, 0);
                    break;
            }
        }
        #endregion

        #region [ Release special ]
        private void ReleaseSpecial(string p_Special)
        {
            switch (p_Special.ToLower())
            {
                case "home":
                    DLLImport.keybd_event(0x24, 0x47, 0x0002, 0);
                    break;
                case "end":
                    DLLImport.keybd_event(0x23, 0x4F, 0x0002, 0);
                    break;
                case "pagedown":
                    DLLImport.keybd_event(0x22, 0x51, 0x0002, 0);
                    break;
                case "pageup":
                    DLLImport.keybd_event(0x21, 0x49, 0x0002, 0);
                    break;
                case "backspace":
                    DLLImport.keybd_event(0x08, 0x0e, 0x0002, 0);
                    break;
                case "esc":
                    DLLImport.keybd_event(0x1B, 0x01, 0x0002, 0);
                    break;
                case "tab":
                    DLLImport.keybd_event(0x09, 0x0f, 0x0002, 0);
                    break;
                case "enter":
                    DLLImport.keybd_event(0x0D, 0x1C, 0x0002, 0);
                    break;
                case "shift":
                    ReleaseShift();
                    break;
                case "control":
                    ReleaseControl();
                    break;
                case "alt":
                    ReleaseAlt();
                    break;
                case "altgr":
                    ReleaseAltGr();
                    break;
                case "f1":
                    DLLImport.keybd_event(0x70, 0x59, 0x0002, 0);
                    break;
                case "f2":
                    DLLImport.keybd_event(0x71, 0x60, 0x0002, 0);
                    break;
                case "f3":
                    DLLImport.keybd_event(0x72, 0x61, 0x0002, 0);
                    break;
                case "f4":
                    DLLImport.keybd_event(0x73, 0x62, 0x0002, 0);
                    break;
                case "f5":
                    DLLImport.keybd_event(0x74, 0x63, 0x0002, 0);
                    break;
                case "f6":
                    DLLImport.keybd_event(0x75, 0x64, 0x0002, 0);
                    break;
                case "f7":
                    DLLImport.keybd_event(0x76, 0x65, 0x0002, 0);
                    break;
                case "f8":
                    DLLImport.keybd_event(0x77, 0x66, 0x0002, 0);
                    break;
                case "f9":
                    DLLImport.keybd_event(0x78, 0x67, 0x0002, 0);
                    break;
                case "f10":
                    DLLImport.keybd_event(0x79, 0x68, 0x0002, 0);
                    break;
                case "f11":
                    DLLImport.keybd_event(0x7A, 0x87, 0x0002, 0);
                    break;
                case "f12":
                    DLLImport.keybd_event(0x7B, 0x88, 0x0002, 0);
                    break;
            }
        }
        #endregion        

        private int[] GetSelectedIndices()
        {
            int[] m_SelectedIndices = new int[lbAutomatedEvents.SelectedIndices.Count];

            for (int i = 0; i < lbAutomatedEvents.SelectedIndices.Count; i++)
                m_SelectedIndices[i] = lbAutomatedEvents.SelectedIndices[i];

            return m_SelectedIndices;
        }

        //Deletes the selected actions.
        private void button6_Click(object sender, EventArgs e)
        {
            int m_Modifier = 0;
            int[] m_SelectedIndices = GetSelectedIndices();

            for (int i = 0; i < m_SelectedIndices.Count(); i++)
            {
                lbAutomatedEvents.Items.RemoveAt(m_SelectedIndices[i] - m_Modifier);
                AutomatedEvents.RemoveAt(m_SelectedIndices[i] - m_Modifier);
                m_Modifier++;
            }
        }

        //Saves the events into a file.
        private void button7_Click(object sender, EventArgs e)
        {
            StreamWriter m_Writer = new StreamWriter(
                tbxScriptToSave.Text + ".txt");

            foreach (AutomatedEventBase myEvent in AutomatedEvents)
                m_Writer.WriteLine(myEvent.GetString());

            m_Writer.Flush();
            m_Writer.Close();
        }

        //Loads the events from the file.
        private void button8_Click(object sender, EventArgs e)
        {
            AutomatedEvents = new List<AutomatedEventBase>();
            lbAutomatedEvents.Items.Clear();

            StreamReader m_Reader = new StreamReader(tbxScriptToLoad.Text + ".txt");

            AutomatedEventBase m_NewEvent = null;

            while (m_Reader.Peek() != -1)
            {
                string m_Line = m_Reader.ReadLine();

                if (m_Line.Contains("EnterText"))
                {
                    m_NewEvent = new AutomatedEventEnterText(
                        m_Line.Substring(m_Line.IndexOf("TextToEnter: ") + 13));

                    m_NewEvent.EventType = Enums.EventTypeEnum.EnterText;
                    m_NewEvent.EventExecutionType = GetExecutionType(m_Line);
                }
                else if (m_Line.Contains("Iteration"))
                {
                    string m_SubString = m_Line.Substring(m_Line.IndexOf("ID: ") + 4);

                    int m_IterationCounter = 0, m_ID = 0;

                    Int32.TryParse(m_SubString.Substring(0, m_SubString.IndexOf(";")), out m_ID);
                    m_SubString = m_SubString.Substring(m_SubString.IndexOf("IterationCounter: ") + 18);

                    Int32.TryParse(m_SubString, out m_IterationCounter);

                    m_NewEvent = new AutomatedEventIteration(m_IterationCounter, m_ID);

                    m_NewEvent.EventType = Enums.EventTypeEnum.Iteration;
                    m_NewEvent.EventExecutionType = GetExecutionType(m_Line);
                }
                else if (m_Line.Contains("MoveAndClick"))
                {
                    string m_SubString = m_Line.Substring(m_Line.IndexOf("PositionX: ") + 11);

                    int m_PosX = 0, m_PosY = 0;

                    Int32.TryParse(m_SubString.Substring(0, m_SubString.IndexOf(";")), out m_PosX);
                    m_SubString = m_SubString.Substring(m_SubString.IndexOf("PositionY: ") + 11);

                    Int32.TryParse(m_SubString, out m_PosY);

                    m_NewEvent = new AutomatedEventMoveAndClick(m_PosX, m_PosY);

                    m_NewEvent.EventType = Enums.EventTypeEnum.MoveAndClick;
                    m_NewEvent.EventExecutionType = GetExecutionType(m_Line);
                }
                else if (m_Line.Contains("Wait"))
                {
                    string m_SubString = m_Line.Substring(m_Line.IndexOf("WaitTimeInMilliSeconds: ") + 24);

                    int m_Wait = 0;
                    Int32.TryParse(m_SubString, out m_Wait);

                    m_NewEvent = new AutomatedEventWait(m_Wait);

                    m_NewEvent.EventType = Enums.EventTypeEnum.Wait;
                    m_NewEvent.EventExecutionType = GetExecutionType(m_Line);
                }

                if (m_NewEvent != null)
                {
                    lbAutomatedEvents.Items.Add(m_NewEvent.GetString());
                    AutomatedEvents.Add(m_NewEvent);
                }
            }

            m_Reader.Close();
        }

        //Edit the selected action.
        private void button9_Click(object sender, EventArgs e)
        {
            if (lbAutomatedEvents.SelectedIndex > -1)
            {
                string m_Event = lbAutomatedEvents.Items[lbAutomatedEvents.SelectedIndex].ToString();

                Enums.EventExecutionTypeEnum m_ExecutionType = GetExecutionType(m_Event);

                if (m_ExecutionType == Enums.EventExecutionTypeEnum.PreEvent)
                {
                    rbtnPreEvent.Checked = false;
                    rbtnPreEvent.Checked = true;
                }
                else if (m_ExecutionType == Enums.EventExecutionTypeEnum.PostEvent)
                {
                    rbtnPostEvent.Checked = false;
                    rbtnPostEvent.Checked = true;
                }
                else if (m_ExecutionType == Enums.EventExecutionTypeEnum.DataEvent)
                {
                    rbtnDataEvent.Checked = false;
                    rbtnDataEvent.Checked = true;
                }

                if (m_Event.Contains("EnterText"))
                {
                    tbxTextToEnter.Text = m_Event.Substring(m_Event.IndexOf("TextToEnter: ") + 13);

                    ddlEventType.SelectedIndex = ddlEventType.FindString("EnterText");
                }
                else if (m_Event.Contains("Iteration"))
                {
                    string m_SubString = m_Event.Substring(m_Event.IndexOf("ID: ") + 4);

                    tbxIterationID.Text = m_SubString.Substring(0, m_SubString.IndexOf(";"));
                    m_SubString = m_SubString.Substring(m_SubString.IndexOf("IterationCounter: ") + 18);

                    tbxIterationCounter.Text = m_SubString;

                    ddlEventType.SelectedIndex = ddlEventType.FindString("Iteration");
                }
                else if (m_Event.Contains("MoveAndClick"))
                {
                    string m_SubString = m_Event.Substring(m_Event.IndexOf("PositionX: ") + 11);

                    tbxPositionX.Text = m_SubString.Substring(0, m_SubString.IndexOf(";"));
                    m_SubString = m_SubString.Substring(m_SubString.IndexOf("PositionY: ") + 11);

                    tbxPositionY.Text = m_SubString;

                    ddlEventType.SelectedIndex = ddlEventType.FindString("MoveAndClick");
                }
                else if (m_Event.Contains("Wait"))
                {
                    string m_SubString = m_Event.Substring(m_Event.IndexOf("WaitTimeInMilliSeconds: ") + 24);

                    tbxWaitTime.Text = m_SubString;

                    ddlEventType.SelectedIndex = ddlEventType.FindString("Wait");
                }
            }
        }

        //Saves an edited event.
        private void button10_Click(object sender, EventArgs e)
        {
            if (lbAutomatedEvents.SelectedIndex > -1)
            {
                AutomatedEventBase m_NewEvent = AutomatedEvents[lbAutomatedEvents.SelectedIndex];

                switch (ddlEventType.Items[ddlEventType.SelectedIndex].ToString())
                {
                    case "EnterText":

                        m_NewEvent = new AutomatedEventEnterText(tbxTextToEnter.Text);

                        m_NewEvent.EventType = Enums.EventTypeEnum.EnterText;
                        m_NewEvent.EventExecutionType = GetExecutionType();

                        break;
                    case "Iteration":

                        int m_IterationCounter = 0, m_IterationID = 0;

                        Int32.TryParse(tbxIterationCounter.Text, out m_IterationCounter);
                        Int32.TryParse(tbxIterationID.Text, out m_IterationID);

                        m_NewEvent = new AutomatedEventIteration(m_IterationCounter, m_IterationID);

                        m_NewEvent.EventType = Enums.EventTypeEnum.Iteration;
                        m_NewEvent.EventExecutionType = GetExecutionType();

                        break;
                    case "MoveAndClick":

                        int m_PosX = 0, m_PosY = 0;

                        Int32.TryParse(tbxPositionX.Text, out m_PosX);
                        Int32.TryParse(tbxPositionY.Text, out m_PosY);

                        m_NewEvent = new AutomatedEventMoveAndClick(m_PosX, m_PosY);

                        m_NewEvent.EventType = Enums.EventTypeEnum.MoveAndClick;
                        m_NewEvent.EventExecutionType = GetExecutionType();

                        break;
                    case "Wait":

                        int m_Wait = 0;
                        Int32.TryParse(tbxWaitTime.Text, out m_Wait);

                        m_NewEvent = new AutomatedEventWait(m_Wait);

                        m_NewEvent.EventType = Enums.EventTypeEnum.Wait;
                        m_NewEvent.EventExecutionType = GetExecutionType();

                        break;
                }

                if (m_NewEvent != null)
                {
                    lbAutomatedEvents.Items[lbAutomatedEvents.SelectedIndex] = m_NewEvent.GetString();
                    AutomatedEvents[lbAutomatedEvents.SelectedIndex] = m_NewEvent;
                }
            }
        }        

        //Moves the selected event up.
        private void button12_Click(object sender, EventArgs e)
        {
            int m_SelectedIndex = lbAutomatedEvents.SelectedIndex;

            string m_Current = lbAutomatedEvents.Items[lbAutomatedEvents.SelectedIndex].ToString();

            lbAutomatedEvents.Items[lbAutomatedEvents.SelectedIndex] =
                lbAutomatedEvents.Items[lbAutomatedEvents.SelectedIndex - 1];

            lbAutomatedEvents.Items[lbAutomatedEvents.SelectedIndex - 1] = m_Current;

            lbAutomatedEvents.SelectedIndex--;

            AutomatedEventBase m_Selected = AutomatedEvents[m_SelectedIndex];
            AutomatedEvents[m_SelectedIndex] = AutomatedEvents[m_SelectedIndex - 1];
            AutomatedEvents[m_SelectedIndex - 1] = m_Selected;
        }

        //Moves the selected event down.
        private void button13_Click(object sender, EventArgs e)
        {
            int m_SelectedIndex = lbAutomatedEvents.SelectedIndex;

            string m_Current = lbAutomatedEvents.Items[lbAutomatedEvents.SelectedIndex].ToString();

            lbAutomatedEvents.Items[lbAutomatedEvents.SelectedIndex] =
                lbAutomatedEvents.Items[lbAutomatedEvents.SelectedIndex + 1];

            lbAutomatedEvents.Items[lbAutomatedEvents.SelectedIndex + 1] = m_Current;

            lbAutomatedEvents.SelectedIndex++;

            AutomatedEventBase m_Selected = AutomatedEvents[m_SelectedIndex];
            AutomatedEvents[m_SelectedIndex] = AutomatedEvents[m_SelectedIndex + 1];
            AutomatedEvents[m_SelectedIndex + 1] = m_Selected;
        }

        //Copy the selected actions.
        private void button14_Click(object sender, EventArgs e)
        {
            int[] m_SelectedIndices = GetSelectedIndices();

            for (int i = 0; i < m_SelectedIndices.Count(); i++)
            {
                lbAutomatedEvents.Items.Add(
                    lbAutomatedEvents.Items[m_SelectedIndices[i]]);
                AutomatedEvents.Add(AutomatedEvents[m_SelectedIndices[i]]);
            }
        }





        /// <summary>
        /// http://www.codeproject.com/Articles/7305/Keyboard-Events-Simulation-using-keybd_event-funct
        /// </summary>

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBOARD_INPUT
        {
            public const uint Type = 1;
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        };

        [StructLayout(LayoutKind.Explicit)]
        struct KEYBDINPUT
        {
            [FieldOffset(0)]
            public ushort wVk;
            [FieldOffset(2)]
            public ushort wScan;
            [FieldOffset(4)]
            public uint dwFlags;
            [FieldOffset(8)]
            public uint time;
            [FieldOffset(12)]
            public IntPtr dwExtraInfo;
        };

        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        };

        [StructLayout(LayoutKind.Explicit)]
        struct INPUT
        {
            [FieldOffset(0)]
            public int type;
            [FieldOffset(4)]
            public MOUSEINPUT mi;
            [FieldOffset(4)]
            public KEYBDINPUT ki;
            [FieldOffset(4)]
            public HARDWAREINPUT hi;
        };
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, IntPtr pInput, int cbSize);

        //Clear class name textbox.
        private void button16_Click(object sender, EventArgs e)
        {
            tbxClassName.Text = string.Empty;
        }

        //Clear window name textbox.
        private void button17_Click(object sender, EventArgs e)
        {
            tbxWindowName.Text = string.Empty;
        }

        private void btnClearCompanyName_Click(object sender, EventArgs e)
        {
            tbxCompanyName.Text = string.Empty;
        }

        private void btnClearYear_Click(object sender, EventArgs e)
        {
            tbxYear.Text = string.Empty;
        }

        private void btnClearPeriod_Click(object sender, EventArgs e)
        {
            tbxPeriod.Text = string.Empty;
        }

        private void btnDataUp_Click(object sender, EventArgs e)
        {
            if (tvData.SelectedNode.Index > 0)
            {
                TreeNode m_Node = tvData.SelectedNode;

                tvData.SelectedNode.Parent.Nodes.RemoveAt(
                    tvData.SelectedNode.Index);
                tvData.SelectedNode.Parent.Nodes.Insert(
                    m_Node.Index - 1, m_Node);

                tvData.SelectedNode = m_Node;
            }
        }

        private void btnDataDown_Click(object sender, EventArgs e)
        {
            if (tvData.SelectedNode.Parent != null)
            {
                if (tvData.SelectedNode.Index < tvData.SelectedNode.Parent.Nodes.Count &&
                    tvData.SelectedNode.Parent.Nodes.Count > 1)
                {
                    TreeNode m_Node = tvData.SelectedNode;                    

                    tvData.SelectedNode.Parent.Nodes.RemoveAt(
                        tvData.SelectedNode.Index);

                    tvData.SelectedNode.Parent.Nodes.Insert(
                        m_Node.Index + 1, m_Node);
                    

                    tvData.SelectedNode = m_Node;
                }
            }
        }        

        private void btnSaveData_Click(object sender, EventArgs e)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DataConfig));
            TextWriter writer = new StreamWriter(tbxSaveDataConfig.Text + ".xml");

            BuildDataConfig();

            serializer.Serialize(writer, CurrentDataConfig);

            writer.Flush();
            writer.Close();
        }

        private void btnLoadData_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbxLoadDataConfig.Text) &&
                File.Exists(tbxLoadDataConfig.Text + ".xml"))
            {
                try
                {
                    rbtnCollapsed.Checked = true;
                    tvData.Nodes[0].Nodes.Clear();

                    XmlSerializer serializer = new XmlSerializer(typeof(DataConfig));

                    FileStream fs = new FileStream(tbxLoadDataConfig.Text + ".xml", FileMode.Open);

                    DataConfig m_Config = (DataConfig)serializer.Deserialize(fs);

                    fs.Close();

                    if (m_Config != null)
                    {
                        foreach (Company m_Company in m_Config.Companies)
                        {
                            TreeNode m_CompanyNode = new TreeNode();
                            m_CompanyNode.Text = "Company: " + m_Company.Identifier + "; Index: " + m_Company.Index;
                            m_CompanyNode.Tag = "Company";

                            tvData.Nodes[0].Nodes.Add(m_CompanyNode);

                            foreach (Year m_Year in m_Company.Years)
                            {
                                TreeNode m_YearNode = new TreeNode();
                                m_YearNode.Text = "Year: " + m_Year.year + "; Index: " + m_Year.Index;
                                m_YearNode.Tag = "Year";

                                m_CompanyNode.Nodes.Add(m_YearNode);

                                foreach (Period m_Period in m_Year.Periods)
                                {
                                    TreeNode m_PeriodNode = new TreeNode();
                                    m_PeriodNode.Text = "Period: " + m_Period.period + "; Index: " + m_Period.Index;
                                    m_PeriodNode.Tag = "Period";

                                    m_YearNode.Nodes.Add(m_PeriodNode);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void rbtnCollapsed_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
            {
                rbtnExpand.Checked = false;
                tvData.CollapseAll();
            }
        }

        private void rbtnExpand_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
            {
                rbtnCollapsed.Checked = false;
                tvData.ExpandAll();
            }
        }

        private void btnDeleteSelected_Click(object sender, EventArgs e)
        {
            if (tvData.SelectedNode != null)
            {
                TreeNode m_Selected = tvData.SelectedNode;
                int m_Index = m_Selected.Index;

                if (m_Selected.Parent != null)
                {
                    if (m_Index > 0)
                        tvData.SelectedNode = m_Selected.Parent.Nodes[m_Index];

                    m_Selected.Parent.Nodes.Remove(m_Selected);
                }
                else
                {
                    if (m_Index > 0)
                        tvData.SelectedNode = tvData.Nodes[m_Index];

                    tvData.Nodes.Remove(m_Selected);
                }
            }
        }

        private void btnClearIndex_Click(object sender, EventArgs e)
        {
            tbxIndex.Text = string.Empty;
        }

        private void btnEditCompany_Click(object sender, EventArgs e)
        {
            tbxCompanyName.Text = tvData.SelectedNode.Text;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (tvData.SelectedNode == null || tvData.SelectedNode.Parent == null) //Company
            {
                if (!string.IsNullOrWhiteSpace(tbxCompanyName.Text))
                {
                    if (!tvData.Nodes[0].Nodes.ContainsKey(tbxCompanyName.Text))
                    {
                        TreeNode m_Node = new TreeNode();
                        m_Node.Name = tbxCompanyName.Text;
                        m_Node.Text = "Company: " + tbxCompanyName.Text + "; Index: " + tbxIndex.Text;
                        m_Node.Tag = "Company";

                        tvData.Nodes[0].Nodes.Add(m_Node);
                    }
                    else
                        MessageBox.Show("Company already exists!");
                }
            }
            else if (tvData.SelectedNode.Parent.Parent == null) //Year
            {
                if (!string.IsNullOrWhiteSpace(tbxYear.Text))
                {
                    if (!tvData.SelectedNode.Nodes.ContainsKey(tbxYear.Text))
                    {
                        TreeNode m_Node = new TreeNode();
                        m_Node.Name = tbxYear.Text;
                        m_Node.Text = "Year: " + tbxYear.Text + "; Index: " + tbxIndex.Text;
                        m_Node.Tag = "Year";

                        tvData.SelectedNode.Nodes.Add(m_Node);
                    }
                    else
                        MessageBox.Show("Year already exists!");
                }
            }
            else if (tvData.SelectedNode.Parent.Parent.Parent == null)
            {
                if (!string.IsNullOrWhiteSpace(tbxPeriod.Text))
                {
                    if (!tvData.SelectedNode.Nodes.ContainsKey(tbxPeriod.Text))
                    {
                        TreeNode m_Node = new TreeNode();
                        m_Node.Name = tbxPeriod.Text;
                        m_Node.Text = "Period: " + tbxPeriod.Text + "; Index: " + tbxIndex.Text;
                        m_Node.Tag = "Period";

                        tvData.SelectedNode.Nodes.Add(m_Node);
                    }
                    else
                        MessageBox.Show("Period already exists!");
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (tvData.SelectedNode != null)
            {
                string m_Name;

                switch (tvData.SelectedNode.Tag.ToString())
                {
                    case "Company":
                        m_Name = tvData.SelectedNode.Text.Substring(
                            tvData.SelectedNode.Text.IndexOf("Company: ") + 9);

                        tbxCompanyName.Text = m_Name.Substring(0, m_Name.IndexOf(";"));

                        m_Name = m_Name.Substring(m_Name.IndexOf("; Index: ") + 9);

                        tbxIndex.Text = m_Name;

                        break;
                    case "Year":

                        m_Name = tvData.SelectedNode.Text.Substring(
                            tvData.SelectedNode.Text.IndexOf("Year: ") + 6);

                        tbxYear.Text = m_Name.Substring(0, m_Name.IndexOf(";"));

                        m_Name = m_Name.Substring(m_Name.IndexOf("; Index: ") + 9);

                        tbxIndex.Text = m_Name;

                        break;
                    case "Period":

                        m_Name = tvData.SelectedNode.Text.Substring(
                            tvData.SelectedNode.Text.IndexOf("Period: ") + 8);

                        tbxPeriod.Text = m_Name.Substring(0, m_Name.IndexOf(";"));

                        m_Name = m_Name.Substring(m_Name.IndexOf("; Index: ") + 9);

                        tbxIndex.Text = m_Name;

                        break;
                }
            }
        }

        private void btnSaveCompany_Click(object sender, EventArgs e)
        {
            switch (tvData.SelectedNode.Tag.ToString())
            {
                case "Company":

                    tvData.SelectedNode.Text = "Company: " + tbxCompanyName.Text + "; Index: " + tbxIndex.Text;

                    break;
                case "Year":

                    tvData.SelectedNode.Text = "Year: " + tbxYear.Text + "; Index: " + tbxIndex.Text;

                    break;
                case "Period":

                    tvData.SelectedNode.Text = "Period: " + tbxPeriod.Text + "; Index: " + tbxIndex.Text;

                    break;
            }
        }

        private void btnInsertCompany_Click(object sender, EventArgs e)
        {
            if (tvData.SelectedNode == null || tvData.SelectedNode.Parent == null || tvData.SelectedNode.Parent.Text == "DataConfig") //Company
            {
                if (!string.IsNullOrWhiteSpace(tbxCompanyName.Text))
                {
                    if (!tvData.Nodes[0].Nodes.ContainsKey(tbxCompanyName.Text))
                    {
                        TreeNode m_Node = new TreeNode();
                        m_Node.Name = tbxCompanyName.Text;
                        m_Node.Text = "Company: " + tbxCompanyName.Text + "; Index: " + tbxIndex.Text;
                        m_Node.Tag = "Company";

                        tvData.Nodes[0].Nodes.Insert(
                            ((tvData.SelectedNode != null) ? tvData.SelectedNode.Index : 0), m_Node);
                    }
                    else
                        MessageBox.Show("Company already exists!");
                }
            }
            else if (tvData.SelectedNode.Parent.Parent.Text == "DataConfig") //Year
            {
                if (!string.IsNullOrWhiteSpace(tbxYear.Text))
                {
                    if (!tvData.SelectedNode.Parent.Nodes.ContainsKey(tbxYear.Text))
                    {
                        TreeNode m_Node = new TreeNode();
                        m_Node.Name = tbxYear.Text;
                        m_Node.Text = "Year: " + tbxYear.Text + "; Index: " + tbxIndex.Text;
                        m_Node.Tag = "Year";

                        tvData.SelectedNode.Parent.Nodes.Insert(tvData.SelectedNode.Index, m_Node);
                    }
                    else
                        MessageBox.Show("Year already exists!");
                }
            }
            else if (tvData.SelectedNode.Parent.Parent.Parent.Text == "DataConfig")
            {
                if (!string.IsNullOrWhiteSpace(tbxPeriod.Text))
                {
                    if (!tvData.SelectedNode.Parent.Nodes.ContainsKey(tbxPeriod.Text))
                    {
                        TreeNode m_Node = new TreeNode();
                        m_Node.Name = tbxPeriod.Text;
                        m_Node.Text = "Period: " + tbxPeriod.Text + "; Index: " + tbxIndex.Text;
                        m_Node.Tag = "Period";

                        tvData.SelectedNode.Parent.Nodes.Insert(tvData.SelectedNode.Index, m_Node);
                    }
                    else
                        MessageBox.Show("Period already exists!");
                }
            }
        }

        private void rbtnPreEvent_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
            {
                rbtnDataEvent.Checked = false;
                rbtnPostEvent.Checked = false;
            }
        }

        private void rbtnDataEvent_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
            {
                rbtnPreEvent.Checked = false;
                rbtnPostEvent.Checked = false;
            }
        }

        private void rbtnPostEvent_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
            {
                rbtnDataEvent.Checked = false;
                rbtnPreEvent.Checked = false;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            TokenSource.Cancel();
        }

        private void btnOpenForGenerate_Click(object sender, EventArgs e)
        {
            DialogResult m_Result = dlgOpenFileDialog.ShowDialog();

            if (m_Result == System.Windows.Forms.DialogResult.OK)
            {
                tbxGenerateData.Text = dlgOpenFileDialog.FileName;
            }
        }

        private void btnGenerateData_Click(object sender, EventArgs e)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DataConfig));
            TextWriter writer = new StreamWriter(tbxSaveDataConfig.Text + ".xml");

            StreamReader m_Reader = new StreamReader(tbxGenerateData.Text);

            CurrentDataConfig = new DataConfig();

            string m_Line;
            while (m_Reader.Peek() != -1)
            {
                m_Line = m_Reader.ReadLine();

                string[] m_Splitted = m_Line.Split(new char[] { '\t' });

                if (m_Splitted != null)
                {
                    Company m_CurrentCompany;
                    Year m_Year;

                    bool m_AddCompany = false;
                    bool m_AddYear = false;

                    if (m_Splitted.Length > 1 && !string.IsNullOrWhiteSpace(m_Splitted[0]) &&
                        !string.IsNullOrWhiteSpace(m_Splitted[1]))
                    {
                        if (CurrentDataConfig.Companies == null)
                        {
                            CurrentDataConfig.Companies = new List<Company>();
                            CurrentDataConfig.AllCompaniesCount = 0;
                        }

                        m_CurrentCompany = CurrentDataConfig.Companies.FirstOrDefault(
                            company => company.Identifier == m_Splitted[0]);

                        if (m_CurrentCompany == null)
                        {
                            m_CurrentCompany = new Company();
                            m_CurrentCompany.Identifier = m_Splitted[0];
                            m_CurrentCompany.Index = CurrentDataConfig.AllCompaniesCount + 1;

                            m_AddCompany = true;

                            CurrentDataConfig.AllCompaniesCount++;
                        }

                        if (m_CurrentCompany.Years == null)
                        {
                            m_CurrentCompany.Years = new List<Year>();
                            m_CurrentCompany.AllYearsCount = 0;
                        }

                        m_Year = m_CurrentCompany.Years.FirstOrDefault(
                            year => year.year == m_Splitted[1]);

                        if (m_Year == null)
                        {
                            m_Year = new Year();
                            m_Year.year = m_Splitted[1];
                            m_Year.Index = m_CurrentCompany.AllYearsCount + 1;
                            m_AddYear = true;

                            m_CurrentCompany.AllYearsCount++;
                        }

                        if(m_AddYear)
                            m_CurrentCompany.Years.Add(m_Year);

                        if (m_AddCompany)
                            CurrentDataConfig.Companies.Add(m_CurrentCompany);
                    }
                }
            }

            m_Reader.Close();

            serializer.Serialize(writer, CurrentDataConfig);

            writer.Flush();
            writer.Close();

            MessageBox.Show("Done!");
        }
    }
}
