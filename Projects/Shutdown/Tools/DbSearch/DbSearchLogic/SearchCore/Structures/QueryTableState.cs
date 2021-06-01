using System.Diagnostics;
using System.Windows.Threading;

namespace DbSearchLogic.SearchCore.Structures {

    /// <summary>
    /// Statusinformationen einer Tabelle.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <company>AvenDATA GmbH</company>
    /// <since>27.01.2010</since>
    public class QueryTableState {
        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="sTableName"></param>
        /// <param name="sHostname"></param>
        /// <param name="nColumns"></param>
        /// <param name="tableCount"></param>
        public QueryTableState(string sTableName, string sHostname, int nColumns, long tableCount) {
            Name = sTableName;
            Hostname = sHostname;
            Columns = nColumns;
            CurrentColumn = 0;
            CurrentColumnName = string.Empty;
            TableCount = tableCount;
            Stopwatch = new Stopwatch();
        }

        public long TableCount { get; set; }

        /// <summary>
        /// Startzeitpunkt der Suche.
        /// </summary>
        public Stopwatch Stopwatch { get; private set; }

        /// <summary>
        /// Name der zu durchsuchenden Tabelle.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Name des verwendeten Datenbankservers.
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Fortschritt der Suche.
        /// </summary>
        public double Progress {
            get { return System.Math.Min(1, mProgress); }
        }
        private double mProgress;

        /// <summary>
        /// Inkrementieren der aktuell durchsuchten Spalte.
        /// </summary>
        public void NextColumn(string sColumn) {
            Stopwatch.Restart();
            CurrentColumnName = sColumn;
            CurrentColumn++;
            mProgress += 0.5 / (double)Columns;
        }

        /// <summary>
        /// Durchsuchen der aktuellen Spalte abgeschlossen.
        /// </summary>
        public void ColumnFinished() {
            mProgress += 0.5 / (double)Columns;
        }

        /// <summary>
        /// Anzahl aller Spalten.
        /// </summary>
        public int Columns { get; private set; }

        /// <summary>
        /// Index der aktuellen Spalte.
        /// </summary>
        public int CurrentColumn { get; private set; }

        /// <summary>
        /// Name der aktuellen Spalte.
        /// </summary>
        public string CurrentColumnName { get; private set; }
    }
}
