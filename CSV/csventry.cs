using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;

namespace XMLSplit.CSV {
    // CSVEntry Class die einen CSV Eintrag repraesentiert
    public class CSVEntry {
        // CSV Eintraege
        private bool ACTIVE;
        private string Mandant, SOURCE, mitSKZ, ohneSKZ, Target, Datenstrom, Printer;

        public CSVEntry(string Mandant, bool ACTIVE, string SOURCE, string mitSKZ, string ohneSKZ, string Target, string Datenstrom, string Printer) {
            this.Mandant = Mandant;
            this.ACTIVE = ACTIVE;
            this.SOURCE = SOURCE;
            this.mitSKZ = mitSKZ;
            this.ohneSKZ = ohneSKZ;
            this.Target = Target;
            this.Datenstrom = Datenstrom;
            this.Printer = Printer;
        }

        // Gibt nur den Pfadnamen des SOURCE zurueck
        public string getSOURCEPath() {
            int idx = this.SOURCE.LastIndexOf('\\');
            return this.SOURCE.Substring(0, idx);
        }

        // Gibt nur den Dateinamen des SOURCE zurueck
        public string getSOURCEFile() {
            int idx = this.SOURCE.LastIndexOf('\\');
            return this.SOURCE.Substring(idx + 1);
        }

        // Gibt das xslt fuer die Trennung mit SKZ zurueck
        public string getMitSKZ() {
            return this.mitSKZ;
        }

        // Gibt das xslt fuer die Trennung ohne SKZ zurueck
        public string getOhneSKZ() {
            return this.ohneSKZ;
        }

        // Gibt den Zielordner zurueck wohin die gesplitteten XML gespeichert werden sollen
        public string getTarget() {
            return this.Target;
        }

        // Gibt den String zurueck, der den Inhalt darstellt, nach dem gesucht wird
        public string getDatenstrom() {
            return this.Datenstrom;
        }

        // Gibt zurueck ob Eintrag aktiv ist 
        public bool isActive() {
            return this.ACTIVE;
        }

        // Ueberschriebene ToString fuer Debug
        public override string ToString()
        {
            return "CSV Entry: " + this.Mandant + " - " + this.SOURCE;
        }
    }
}