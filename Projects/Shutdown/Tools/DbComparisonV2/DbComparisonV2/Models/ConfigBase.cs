using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace DbComparisonV2.Models
{

        /// <summary>
        /// Basisklasse für die Konfigurationsdateien.
        /// </summary>
        [DataContract]
        public class ConfigBase
        {

            // Member-Variablen
            private uint mLocks;
            protected bool mChanged;

            /// <summary>
            /// Konstruktor.
            /// </summary>
            public ConfigBase()
            {
                mLocks = 0;
                mChanged = false;
            }

            /// <summary>
            /// Gibt an, wie of das Objekt gesperrt wurde.
            /// </summary>
            [XmlIgnore]
            public uint Locks
            {
                get { return mLocks; }
            }

            /// <summary>
            /// Gibt an, ob die Konfiguration für Änderungen gesperrt ist.
            /// </summary>
            [XmlIgnore]
            public bool Locked
            {
                get { return (mLocks > 0); }
            }

            /// <summary>
            /// Gibt an, ob die Konfiguration seit dem letzten Laden oder Speicher verändert wurde.
            /// </summary>
            [XmlIgnore]
            public virtual bool Changed
            {
                get { return mChanged; }
                set { mChanged = value; }
            }

            /// <summary>
            /// Sperrt die Konfiguration.
            /// </summary>
            public void Lock()
            {
                mLocks++;
            }

            /// <summary>
            /// Gibt die Konfiguration wieder frei.
            /// </summary>
            public void Unlock()
            {
                if (mLocks > 0)
                    mLocks--;
            }
        }
    

}
