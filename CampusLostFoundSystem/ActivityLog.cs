using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampusLostFoundSystem
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public string Action { get; set; } = "";
        public string PerformedBy { get; set; } = "";
        public string Role { get; set; } = "";
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
