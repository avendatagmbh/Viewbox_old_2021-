using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomationWPF.Helpers
{
    public static class Enums
    {
        public enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        public enum EventTypeEnum
        {
            EnterText,
            Iteration,
            MoveAndClick,
            Wait
        }

        public enum EventExecutionTypeEnum
        {
            DataEvent,
            PostEvent,
            PreEvent,
        }

        public enum ConfigEnums
        {
            currentconfig,
            loadactionpath,
            loaddataconfig,
            saveactionpath,
            savedataconfig,
            windowclassname,
            windowname
        }
    }
}
