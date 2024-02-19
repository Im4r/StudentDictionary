using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudentDictionary.Models
{
    public class Student
    {
        public bool IsSelected { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString(); 
        public string Name { get; set; }
        public string ClassName { get; set; }
        public bool WasQueried { get; set; } 

        public bool IsLucky { get; set; }
    }
}


