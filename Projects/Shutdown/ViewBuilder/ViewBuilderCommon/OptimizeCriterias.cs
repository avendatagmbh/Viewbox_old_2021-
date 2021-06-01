using System;

namespace ViewBuilderCommon
{
    /// <summary>
    ///   Klasse zur Verwaltung der Optimierungskriterien.
    /// </summary>
    public class OptimizeCriterias
    {
        /// <summary>
        ///   Konstruktor.
        /// </summary>
        public OptimizeCriterias()
        {
            DoClientSplit = false;
            ClientField = string.Empty;
            DoCompCodeSplit = false;
            CompCodeField = string.Empty;

            DoFYearSplit = false;
            GJahrField = string.Empty;
            YearRequired = false;
        }

        /// <summary>
        ///   Gibt an, ob eine Aufteilung nach Mandanten durchgef�hrt werden soll.
        /// </summary>
        public Boolean DoClientSplit { get; set; }

        /// <summary>
        ///   Feld f�r Aufteilung nach Mandanten.
        /// </summary>
        public String ClientField { get; set; }

        /// <summary>
        ///   Gibt an, ob eine Aufteilungs nach Buchungskreisen durchgef�hrt werden soll.
        /// </summary>
        public Boolean DoCompCodeSplit { get; set; }

        /// <summary>
        ///   Feld f�r Aufteilung nach Buchungskreisen.
        /// </summary>
        public String CompCodeField { get; set; }

        /// <summary>
        ///   Gibt an, ob Aufteilung nach Gesch�ftsjahren durchgef�hrt werden soll.
        /// </summary>
        public Boolean DoFYearSplit { get; set; }

        /// <summary>
        ///   Feld f�r Aufteilung nach Gesch�ftsjahren.
        /// </summary>
        public String GJahrField { get; set; }

        public bool YearRequired { get; set; }
    }
}