using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VatsCallsMonitoring.Models
{
    public class User
    {
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public string Pin { get; set; }
        public bool IsSupervisor { get; set; }
        public bool IsOperator { get; set; }
        public string Email { get; set; }
        public int Recording { get; set; }
    }
}
