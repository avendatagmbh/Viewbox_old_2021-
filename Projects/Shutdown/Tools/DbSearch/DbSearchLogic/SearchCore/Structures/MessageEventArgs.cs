using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbSearchLogic.SearchCore.Structures {
    public class MessageEventArgs : EventArgs {
        private string mMsg;

        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="sMsg"></param>
        public MessageEventArgs(string sMsg) {
            mMsg = sMsg;
        }

        /// <summary>
        /// Nachrichtentext.
        /// </summary>
        public string Msg {
            get { return mMsg; }
        }
    }
}
