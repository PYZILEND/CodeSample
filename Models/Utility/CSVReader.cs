using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace VatsCallsMonitoring.Models
{
    static class CSVReader
    {
        const int StateColumn = 3;
        const int DestNumberColumn = 6;
        //const int groupColumn = 8;
        //const int startDateColumn = 9;
        //const int durationColumn = 10;

        const string NumbersToGroupsCSVPath = "GroupsToNumbers.csv";

        /// <summary>
        /// Reads group numbers from CSV file and compiles them into dictionary of (group pin -> group number)
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> ReadGroupNumbers()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            using (StreamReader reader = new StreamReader(NumbersToGroupsCSVPath, Encoding.Default))
            {
                while (!reader.EndOfStream)
                {
                    string row = reader.ReadLine();
                    string[] components = row.Split(';');
                    dictionary.Add(components[1], components[0]);                    
                }
            }
            return dictionary;
        }
        
        
        public static List<Call> ReadCallsCSV(string path)
        {
            List<Call> calls = new List<Call>();
            using (StreamReader reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    string row = reader.ReadLine();

                    string[] columns = row.Split(';');
                    string number = Regex.Match(columns[DestNumberColumn], ":\\d*").Value.Substring(1);
                    
                    calls.Add(new Call() { State = int.Parse(columns[StateColumn]), DestNumber = number});
                }
            }
            return calls;
        }
    }
}