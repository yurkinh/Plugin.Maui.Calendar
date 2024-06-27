namespace SampleApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WeekViewPage : ContentPage
    {
        public WeekViewPage()
        {
            InitializeComponent();
        }

        void UnloadedHandler(object sender, EventArgs e)
        {
            calendar.Dispose();
        }
    }
}
