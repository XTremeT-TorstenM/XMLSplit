// using System;
using System.IO;

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using XMLSplit.Logging;
using XMLSplit.Configuration;
using XMLSplit.XML;
using XMLSplit.CSV;

namespace XMLSplit {
internal class XMLSplit
    {
        // XML-Split liest CSV beim Start ein 
        // Im naechsten Schritt wird das Production Verzeichniss inkl Unterverzeichnissen
        // nach XML durchsucht
        // Alle gefundenen Dateien werden in einer Liste gespeichert
        // Zu jeder Datei wird auch ihr CSV Eintrag hinzugefuegt
        // Zustand: Liste(XML Datei / CSV Eintrag)
        // Danach erfolgt die Trennung mit den jeweiligen XSLT
        // Speicherung im Target / Backup des Originals / Weiterleitung Drucker

        private static void Main(string[] args)
        {
            // erzeuge log class fuer Logging
            Log log = new Log();

            // erzeuge config class fuer laden der Konfiguration
            Config config = new Config(log);

            // erzeuge csvData mit CSV File aus Config
            CSVData csvData = new CSVData(config, log);

            // erzeuge xmlFileList mit allen XMLFiles die im ProdDir aus der Config zu finden sind
            XMLFilelist xmlfilelist = new XMLFilelist(csvData, config, log);
            xmlfilelist.getFileList();

            // Alle XML Dateien in der Liste splitten
            xmlfilelist.processAll();
            log.saveLog();
        }
    }
}