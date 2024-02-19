using StudentDictionary.Views;

namespace StudentDictionary
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }
    }
}
