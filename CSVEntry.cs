using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;

namespace CSVEntryClass {
    public class CSVEntry {
        private string Mandant;
        private bool ACTIVE;
        private string SOURCE;
        private string mitSKZ;
        private string ohneSKZ;
        private string Target;
        private string Datenstrom;
        private string Printer;

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

        public string getSOURCEPath() {
            int idx = this.SOURCE.LastIndexOf('\\');
            return this.SOURCE.Substring(0, idx);
        }

        public string getSOURCEFile() {
            int idx = this.SOURCE.LastIndexOf('\\');
            return this.SOURCE.Substring(idx + 1);
        }

        public string getMitSKZ() {
            return this.mitSKZ;
        }

        public string getOhneSKZ() {
            return this.ohneSKZ;
        }

        public string getTarget() {
            return this.Target;
        }

        public string getDatenstrom() {
            return this.Datenstrom;
        }

        public bool isActive() {
            return this.ACTIVE;
        }

        public override string ToString()
        {
            return "CSV Entry: " + this.Mandant + " - " + this.ACTIVE;
        }

    }
}