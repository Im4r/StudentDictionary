using StudentDictionary.Models;
using StudentDictionary.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentDictionary.Views
{
    public partial class ClassesAndStudentsView : ContentPage
    {
        private List<Classroom> _classrooms;
        private int? _luckyNumberIndex;
        private string _selectedClassName;
        private HashSet<string> _recentlyQueriedStudentIds = new HashSet<string>();

        public ClassesAndStudentsView()
        {
            InitializeComponent();
            LoadClassData();

            MessagingCenter.Subscribe<ManageStudentsView, int>(this, "LuckyNumberGenerated", (sender, index) => 
            {
                _luckyNumberIndex = index;
                DisplayLuckyStudent(index);
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadClassData();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<ManageStudentsView, int>(this, "LuckyNumberGenerated");
        }

        private async Task LoadClassData()
        {
            _classrooms = await FileService.LoadDataAsync();
            ClassesPicker.ItemsSource = _classrooms.Select(c => c.ClassName).ToList();

            if (!string.IsNullOrEmpty(_selectedClassName))
            {
                ClassesPicker.SelectedItem = _selectedClassName;
                UpdateCollectionView();
            }
        }

        public async void RefreshStudentsView()
        {
            await LoadClassData();
            OnClassSelected(this, null); 
        }

        private void OnClassSelected(object sender, EventArgs e)
        {
            if (ClassesPicker.SelectedIndex != -1)
            {
                _selectedClassName = ClassesPicker.SelectedItem.ToString();
                UpdateCollectionView();
            }
        }

        private void DisplayLuckyStudent(int index)
        {
            var luckyStudent = _classrooms.SelectMany(c => c.Students).ElementAtOrDefault(index);
            if (luckyStudent != null)
            {
                luckyStudent.IsLucky = true;
                UpdateCollectionView(); 
            }
        }

        private async void OnPickStudentClicked(object sender, EventArgs e)
        {
            if (ClassesPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Error", "Please select a class first.", "OK");
                return;
            }

            var selectedClassName = ClassesPicker.SelectedItem.ToString();
            var selectedClass = _classrooms.FirstOrDefault(c => c.ClassName == selectedClassName);
            if (selectedClass == null) return;

            if (selectedClass.Students.Count(s => s.WasQueried) >= 4)
            {
                foreach (var student in selectedClass.Students)
                {
                    student.WasQueried = false;
                }
            }

            var availableStudents = selectedClass.Students
            .Where(s => !s.IsSelected && !s.IsLucky && !_recentlyQueriedStudentIds.Contains(s.Id)) 
            .ToList();

            if (availableStudents.Count == 0)
            {
                await DisplayAlert("Error", "No students available for picking or all students have been picked.", "OK");
                return;
            }

            var random = new Random();
            var pickedStudentIndex = random.Next(availableStudents.Count);
            var pickedStudent = availableStudents[pickedStudentIndex];
            pickedStudent.WasQueried = true;
            _recentlyQueriedStudentIds.Add(pickedStudent.Id);

            if (_recentlyQueriedStudentIds.Count >= 4)
            {
                _recentlyQueriedStudentIds.Clear();
            }

            SelectedStudentLabel.Text = $"{pickedStudent.Name} has been picked.";
            await FileService.SaveDataAsync(_classrooms);
            RefreshStudentsView();
        }

        private async void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            if (checkBox.BindingContext is StudentViewModel studentViewModel)
            {
                var student = _classrooms.SelectMany(c => c.Students)
                                         .FirstOrDefault(s => s.Id == studentViewModel.Id);
                if (student != null)
                {
                    student.IsSelected = checkBox.IsChecked;
                    await FileService.SaveDataAsync(_classrooms); 
                }
            }
        }

        private void UpdateCollectionView()
        {
            var selectedClassName = ClassesPicker.SelectedItem?.ToString();
            var selectedClass = _classrooms.FirstOrDefault(c => c.ClassName == selectedClassName);
            if (selectedClass != null)
            {
                var studentViewModels = selectedClass.Students
                    .OrderBy(s => s.Name)
                    .Select((student, index) => new StudentViewModel
                    {
                        Id = student.Id,
                        Name = student.Name,
                        DisplayName = $"{index + 1}. {student.Name}",
                        IsSelected = student.IsSelected,
                        IsLucky = student.IsLucky
                    })
                    .ToList();

                StudentsCollectionView.ItemsSource = studentViewModels;

                if (_luckyNumberIndex.HasValue && _luckyNumberIndex.Value < studentViewModels.Count)
                {
                    DisplayLuckyStudent(_luckyNumberIndex.Value);
                }
            }
        }

        private async void OnStudentNameUnfocused(object sender, FocusEventArgs e)
        {
            var entry = (Entry)sender;
            if (entry.BindingContext is StudentViewModel studentViewModel)
            {
                var updatedDisplayName = entry.Text;
                var nameParts = updatedDisplayName.Split(new[] { ". " }, StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length > 1)
                {
                    var updatedName = nameParts[1];
                    var student = _classrooms.SelectMany(c => c.Students)
                                             .FirstOrDefault(s => s.Id == studentViewModel.Id);

                    if (student != null)
                    {
                        student.Name = updatedName;
                        studentViewModel.DisplayName = updatedDisplayName;
                        studentViewModel.Name = updatedName;
                        await FileService.SaveDataAsync(_classrooms);
                    }
                }
            }
        }

        private async void GoToManageStudents(object sender, EventArgs e)
        {
            var manageStudentsPage = new ManageStudentsView();
            await Navigation.PushAsync(manageStudentsPage);
        }
    }
}
