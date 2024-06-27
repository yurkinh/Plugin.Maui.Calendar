namespace SampleApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AdvancedPage : ContentPage
    {
        public AdvancedPage()
        {
            InitializeComponent();
        }

        void UnloadedHandler(object sender, EventArgs e)
        {
            calendar.Dispose();
        }
    }
}
