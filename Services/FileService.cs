using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using StudentDictionary.Models;

namespace StudentDictionary.Services
{
    public static class FileService
    {
        private static string FilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "students.txt");

        public static async Task<List<Classroom>> LoadDataAsync()
        {
            var classrooms = new List<Classroom>();

            if (!File.Exists(FilePath))
            {
                return classrooms;
            }

            var lines = await File.ReadAllLinesAsync(FilePath);
            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts.Length == 2)
                {
                    var className = parts[0];
                    var studentParts = parts[1].Split(',');
                    var students = new List<Student>();
                    foreach (var studentPart in studentParts)
                    {
                        var studentDetails = studentPart.Split('|');
                        if (studentDetails.Length == 4)
                        {
                            students.Add(new Student
                            {
                                Name = studentDetails[0],
                                IsLucky = bool.Parse(studentDetails[1]),
                                IsSelected = bool.Parse(studentDetails[2]),
                                WasQueried = int.Parse(studentDetails[3]) == 1
                            });
                        }
                    }
                    classrooms.Add(new Classroom { ClassName = className, Students = students });
                }
            }

            return classrooms;
        }

        public static async Task SaveDataAsync(List<Classroom> classrooms)
        {
            var lines = new List<string>();
            foreach (var classroom in classrooms)
            {
                var studentLines = classroom.Students.Select(s =>
                    $"{s.Name}|{s.IsLucky}|{s.IsSelected}|{(s.WasQueried ? 1 : 0)}");
                lines.Add($"{classroom.ClassName}:{string.Join(",", studentLines)}");
            }

            await File.WriteAllLinesAsync(FilePath, lines);
        }
    }
}
