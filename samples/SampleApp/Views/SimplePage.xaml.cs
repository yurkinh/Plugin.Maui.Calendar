namespace SampleApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SimplePage : ContentPage
    {
        public SimplePage()
        {
            InitializeComponent();
        }


        void UnloadedHandler(object sender, EventArgs e)
        {
            //calendar.Dispose();
        }
    }
}
