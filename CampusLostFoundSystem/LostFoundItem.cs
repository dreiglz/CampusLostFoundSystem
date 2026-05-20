using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampusLostFoundSystem
{
    public class LostFoundItem
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = "";
        public string Category { get; set; } = "";
        public string Status { get; set; } = "";
        public string Location { get; set; } = "";
        public string Description { get; set; } = "";
        public string Reporter { get; set; } = "";
        public string ClaimStatus { get; set; } = "Not Claimed";
        public string ImagePath { get; set; } = "";

        public DateTime DateReported { get; set; } = DateTime.Now;
    }

}




