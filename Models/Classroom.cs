using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentDictionary.Models
{
    public class Classroom
    {
        public string ClassName { get; set; }
        public List<Student> Students { get; set; } = new List<Student>();
    }
}
