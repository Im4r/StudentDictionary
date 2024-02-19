using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentDictionary.Models
{
    public class StudentViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; } 
        public bool IsSelected { get; set; }
        public bool IsLucky { get; set; } 

    }
}
