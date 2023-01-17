using System;
using System.IO;
using System.Collections.Generic;
using CSVEntryClass;

namespace CSVDataClass {
    public class CSVData {
        // Liste mit allen Eintraegen aus der CSV
        private List<CSVEntry> csvList = new List<CSVEntry>();
        public CSVData(string csvFileName) {
            // Oeffne Datei
            using (StreamReader reader = new StreamReader(csvFileName)) {
                // Skip erste Zeile = Header
                var skipline = reader.ReadLine();
                // Lese bis zum Ende des Files
                while (!reader.EndOfStream) {
                    // einzelner String einer Zeile
                    var line = reader.ReadLine();
                    // wenn die Zeile Inhalt hat ?
                    if (line != null) {
                        // split am Delimiter
                        var v = line.Split(';');
                        // sind 8 Objekte vorhanden ?
                        if (v.Length == 8) {
                            // erstelle temporaeren CSV Eintrag
                            var tempCSVEntry = new CSVEntry(v[0], bool.Parse(v[1]), v[2], v[3], v[4], v[5], v[6], v[7]);
                            // fuege CSV Eintrag zur Liste hinzu wenn aktiv
                            if (tempCSVEntry.isActive()) {
                                csvList.Add(tempCSVEntry);
                            }
                        }
                    }
                }
            }
        }

        public List<CSVEntry> getList() {
            return csvList;
        }
    }
}