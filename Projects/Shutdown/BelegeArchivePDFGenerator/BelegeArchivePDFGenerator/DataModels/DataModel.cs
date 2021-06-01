using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BelegeArchivePDFGenerator.DataModels
{
    public class DataModel
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Kunden_nr { get; set; }
        public string Verstags_konto { get; set; }
        public string Referenz_nummber { get; set; }
        public string Strom_rechnung { get; set; }
        public string abrechnun_date { get; set; }
        public string zahlpunkt { get; set; }
        public string verbrauchersstelle { get; set; }
        public string rechnung_date { get; set; }
        public string falligkeits_date { get; set; }
        public string leistung_ver { get; set; }
        public string leistung_preis { get; set; }
        public string leistung_sum { get; set; }
        public string ablesung { get; set; }
        public string wirkarbeit_ht_ver { get; set; }
        public string wirkarbeit_ht_preis { get; set; }
        public string wirkarbeit_ht_sum { get; set; }
        public string wirkarbeit_nt_ver { get; set; }
        public string wirkarbeit_nt_preis { get; set; }
        public string wirkarbeit_nt_sum { get; set; }
        public string gutschrift_messpreis { get; set; }
        public string messung { get; set; }
        public string abrechnung { get; set; }
        public string messstellenbetrieb { get; set; }
        public string eeg_ver { get; set; }
        public string eeg_preis { get; set; }
        public string eeg_sum { get; set; }
        public string kwkg_ver { get; set; }
        public string kwkg_preis { get; set; }
        public string kwkg_sum { get; set; }
        public string strumsteuer { get; set; }
        public string wirkaibeit_ver { get; set; }
        public string wirkaibeit_preis { get; set; }
        public string wirkaibeit_sum { get; set; }
        public string rechnungsbetrag { get; set; }
        public string zahlnummer { get; set; }
    }
}
