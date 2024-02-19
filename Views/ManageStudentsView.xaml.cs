using StudentDictionary.Models;
using StudentDictionary.Services;
using Microsoft.Maui.Controls;

namespace StudentDictionary.Views
{
    public partial class ManageStudentsView : ContentPage
    {
        private List<Classroom> _classrooms;

        public ManageStudentsView()
        {
            InitializeComponent();
            LoadClassData();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadClassData();
        }

        private async void OnGenerateLuckyNumberClicked(object sender, EventArgs e)
        {
            foreach (var classroom in _classrooms)
            {
                foreach (var student in classroom.Students)
                {
                    student.IsLucky = false;
                }
            }
            var allStudents = _classrooms.SelectMany(c => c.Students).ToList();
            var luckyNumberIndex = new Random().Next(allStudents.Count);
            var luckyStudent = allStudents[luckyNumberIndex];
            luckyStudent.IsLucky = true;
            LuckyNumberLabel.Text = $"{luckyNumberIndex + 1}";
            MessagingCenter.Send(this, "LuckyNumberGenerated", luckyNumberIndex);

            await FileService.SaveDataAsync(_classrooms);
        }





        private async Task LoadClassData()
        {
            _classrooms = await FileService.LoadDataAsync();
            ClassPicker.ItemsSource = _classrooms.Select(c => c.ClassName).ToList();
        }

        private async void OnAddNewClassClicked(object sender, EventArgs e)
        {
            var newClassName = NewClassNameEntry.Text?.Trim();
            if (!string.IsNullOrEmpty(newClassName))
            {
                var newClass = new Classroom { ClassName = newClassName, Students = new List<Student>() };
                _classrooms.Add(newClass);
                await FileService.SaveDataAsync(_classrooms);
                LoadClassData(); 
                NewClassNameEntry.Text = string.Empty; 
            }
        }

        private async void OnAddNewStudentClicked(object sender, EventArgs e)
        {
            var newStudentName = NewStudentNameEntry.Text?.Trim();
            var selectedClass = ClassPicker.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(newStudentName) && !string.IsNullOrEmpty(selectedClass))
            {
                var classroom = _classrooms.FirstOrDefault(c => c.ClassName == selectedClass);
                if (classroom != null)
                {
                    var newStudent = new Student { Name = newStudentName };
                    classroom.Students.Add(newStudent);
                    await FileService.SaveDataAsync(_classrooms);
                    LoadClassData();
                    NewStudentNameEntry.Text = string.Empty;
                }
            }
        }
    }

}