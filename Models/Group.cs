using System.Collections.Generic;
using System.Linq;

namespace VatsCallsMonitoring.Models
{
    public class Group
    {
        public string Name { get; set; }
        public string Number { get; set; }
        public string Pin { get; set; }
        public List<Call> Calls { get; set; }
        public int SuccessefullCalls 
        { 
            get 
            {
                return Calls.Count(a=>a.State==1);
            } 
        }

        public int FailedCalls {
            get
            {
                return Calls.Count(a => a.State == 2);
            }
        }
        
        public List<User> Users { get; set; }
    }
}
