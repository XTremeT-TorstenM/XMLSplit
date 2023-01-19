using System;

namespace XMLSplit.Logging {
    public class Log {
        public Log() {
            Console.WriteLine("Log Cons");
        }

        ~Log() {
            Console.WriteLine("Log Dest");
        }
    }
}