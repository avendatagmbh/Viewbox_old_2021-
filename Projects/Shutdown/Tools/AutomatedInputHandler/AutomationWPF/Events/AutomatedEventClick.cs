using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomationWPF.Events
{
    /// <summary>
    /// Mouse-click event.
    /// </summary>
    public class AutomatedEventMoveAndClick : AutomatedEventBase, IPosition
    {
        /// <summary>
        /// Mouse X position.
        /// </summary>
        public int PositionX { get; set; }
        /// <summary>
        /// Mouse Y position.
        /// </summary>
        public int PositionY { get; set; }

        /// <summary>
        /// Constructor of the class.
        /// </summary>
        /// <param name="p_PosX">Mouse X position.</param>
        /// <param name="p_PosY">Mouse Y position.</param>
        public AutomatedEventMoveAndClick(int p_PosX, int p_PosY)
        {
            PositionX = p_PosX;
            PositionY = p_PosY;
        }

        /// <summary>
        /// Gets the formatted version of the event, used in saving/loading events.
        /// </summary>
        /// <returns>Formatted version of the event.</returns>
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
