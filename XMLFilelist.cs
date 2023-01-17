using System;
using System.IO;
using System.Collections.Generic;
using CSVDataClass;
using CSVEntryClass;

namespace XMLFilelistClass {
    public class XMLFilelist {
        private List<Dictionary<string, CSVEntry>> xmlList;

        public XMLFilelist(string xmlfilePath = @"C:\Temp\Testumgebung\Test_Production") {
            xmlList = new List<Dictionary<string, CSVEntry>>();

            Console.WriteLine("File List {0}", xmlfilePath);
        }
        
        public void getXMLFilelist(CSVData csvData) {
            foreach(CSVEntry entry in csvData.getList()) {
                if (Directory.Exists(entry.getSOURCEPath())) {
                    string[] files = Directory.GetFiles(entry.getSOURCEPath(), entry.getSOURCEFile());
                    if (files.Length > 0) {
                        foreach(string file in files) {
                            var dicent = new Dictionary<string, CSVEntry>();
                            dicent.Add(file, entry);
                            xmlList.Add(dicent);
                        }
                    }
                }
            }
        }
        
        public void showFilelist() {
            foreach (Dictionary<string, CSVEntry> xmlFile in xmlList)
            {
                foreach (var kvp in xmlFile)
                {
                    Console.WriteLine("Key: {0} Value: {1}", kvp.Key, kvp.Value);
                }
            }

        }
    }
}