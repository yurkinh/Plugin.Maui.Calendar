using SampleApp.Views;

namespace SampleApp;

public partial class App : Application
{
    public static new App Current => (App)Application.Current;
    public App()
	{
		InitializeComponent();

        MainPage = new AppShell();
    }
   

    protected override void OnStart()
    {
        AppTheme currentTheme = Application.Current.RequestedTheme;        
    }   

    protected override void OnResume()
    {
        AppTheme currentTheme = Application.Current.RequestedTheme;        
    }
  
}

