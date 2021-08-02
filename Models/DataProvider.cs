using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VatsCallsMonitoring.Models
{
    public static class DataProvider
    {
        public static List<Group> GetGroups()
        {
            List<Group> groups = ApiIntegration.GetGroups();
            Dictionary<string, string> groupNumbersDictionary = CSVReader.ReadGroupNumbers();
            foreach(Group group in groups)
            {
                if (groupNumbersDictionary.ContainsKey(group.Pin))
                {
                    group.Number = groupNumbersDictionary[group.Pin];
                }     
            }
            return groups;
        }

        public static List<Group> RefreshCallsData(List<Group> groups)
        {
            string CSVPath = ApiIntegration.DownloadCallsData(DateTime.Now.Date.AddDays(-7), DateTime.Now.Date);
            List<Call> calls = CSVReader.ReadCallsCSV(CSVPath);

            Dictionary<string, Group> groupsDictionary = new Dictionary<string, Group>();
            foreach(Group group in groups)
            {
                group.Calls = new List<Call>();
                if (group.Number != null)
                {
                    groupsDictionary.Add(group.Number, group);
                }                
            }            

            foreach(Call call in calls)
            {
                if (groupsDictionary.ContainsKey(call.DestNumber))
                {
                    groupsDictionary[call.DestNumber].Calls.Add(call);
                }
            }

            groups.Sort((a, b) =>
            {
                int result = b.FailedCalls - a.FailedCalls;
                if (result != 0)
                {
                    return result;
                }
                result = b.SuccessefullCalls - a.SuccessefullCalls;
                if (result != 0)
                {
                    return result;
                }
                return a.Name.CompareTo(b.Name);
            });

            return groups;
        }
    }
}
