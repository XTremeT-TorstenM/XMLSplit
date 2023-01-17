using System;
using System.IO;
using CSVDataClass;
using CSVEntryClass;
using XMLFilelistClass;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

internal class XMLSplit {
    // XML-Split soll mit der CSV als Argument gestartet werden
    // Diese wird komplett eingelesen
    // Im naechsten Schritt wird das Production Verzeichniss inkl Unterverzeichnissen
    // nach XMK durchsucht
    // Alle gefundenen Dateien werden in einer Liste gespeichert
    // Zu jeder Datei wird auch ihr CSV Eintrag hinzugefuegt
    // Zustand: Liste(XML Datei / CSV Eintrag)
    // Danach erfolgt die Trennung mit den jeweiligen XSLT
    // Speicherung im Target / Backup des Originals / Weiterleitung Drucker

    private static void Main(string[] args) {

        List<Dictionary<string, CSVEntry>> xmlList = new List<Dictionary<string, CSVEntry>>();
        // check Argument Liste
        var arglen = args.Length;
        if (arglen == 0) {
            Console.WriteLine("Usage: xmlsplit <csvFile> | <pathToXMLFiles>");
            Environment.Exit(0);
        }
        // existiert das CSV File ?
        if (!File.Exists(args[0])) {
            Console.WriteLine("CSV file {0} does not exist!", args[0]);
            Environment.Exit(0);
        }
        var csvFilename = args[0];
        // erzeuge csvData mit dem uebergebenen CSV File
        CSVData csvData = new CSVData(csvFilename);
        // erzeuge xmlFilelist aus dem uebergebenen Verzeichnis (Standart)
        XMLFilelist xmlFilelist = (arglen == 2) ? new XMLFilelist(args[1]) : new XMLFilelist();
        xmlFilelist.getXMLFilelist(csvData);
        xmlFilelist.showFilelist();

        // string[] p1 = Directory.GetFiles(@"C:\Temp\Testumgebung\Test_Production\", "P07_RE_2*.xml");
        // foreach(string file in p1) {
        //     Console.WriteLine(file);
        // }
    }
}