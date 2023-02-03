using System;
using System.IO;

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using XMLSplit.Logging;
using XMLSplit.Configuration;
using XMLSplit.XML;
using XMLSplit.CSV;

namespace XMLSplit {
public class XMLSplit
    {
        // XML-Split wird durch WatchDirectory getriggert und liest das CSV beim Start ein 
        // Im naechsten Schritt wird das Production Verzeichniss inkl Unterverzeichnissen
        // nach XML durchsucht --> Alle gefundenen Dateien werden in einer Liste gespeichert
        // Zu jeder gefundenen XML Datei wird auch ihr CSV Eintrag vermerkt
        // Zustand: Liste(XML Datei / CSV Eintrag)
        // Danach erfolgt die Trennung mit den jeweiligen XSLT
        // Speicherung im Target / Backup des Originals / Weiterleitung Drucker / Loeschung
        private Log log;
        private Config config;
        private CSVData csvdata;
        private XMLFilelist xmlfilelist;

        public XMLSplit() {
            // erzeuge log class fuer Logging
            this.log = new Log();
            // erzeuge config class fuer Laden der Konfiguration
            this.config = new Config(this.log);
            // erzeuge csvData mit CSV File aus Config
            this.csvdata = new CSVData(this.config, this.log);
            // erzeuge xmlFileList mit allen XMLFiles die im ProdDir aus der Config zu finden sind
            this.xmlfilelist = new XMLFilelist(this.csvdata, this.config, this.log);
        }
        private static void Main(string[] args)
        {
            XMLSplit xmlsplit = new XMLSplit();

            // generiere FileList aus Inhalten des Produktionsordners und der CSV
            xmlsplit.xmlfilelist.getFileList();

            // Alle XML Dateien in der Liste verarbeiten
            xmlsplit.xmlfilelist.processAll();

            // log Datei speichern
            xmlsplit.log.saveLog();
        }
    }
}