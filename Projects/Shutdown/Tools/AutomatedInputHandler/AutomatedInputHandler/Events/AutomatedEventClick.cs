using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomatedInputHandler.Events
{
    public class AutomatedEventMoveAndClick : AutomatedEventBase, IPosition
    {
        public int PositionX { get; set; }
        public int PositionY { get; set; }

        public AutomatedEventMoveAndClick(int p_PosX, int p_PosY)
        {
            PositionX = p_PosX;
            PositionY = p_PosY;
        }

        public override string GetString()
        {
            return
                "Exec. Type: " + EventExecutionType.ToString() +
                "; Event type: " + EventType.ToString() +
                "; PositionX: " + PositionX.ToString() +
                "; PositionY: " + PositionY.ToString();
        }
    }
}
