namespace SampleApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SimplePage : ContentPage
    {
        public SimplePage()
        {
            InitializeComponent();
        }


        void simpleCalendarPage_Unloaded(System.Object sender, System.EventArgs e)
        {
            calendar.Dispose();
        }
    }
}
