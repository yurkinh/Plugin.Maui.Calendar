namespace SampleApp.Services;
public class ThemeService : IThemeService
{
    public void SetTheme(AppTheme appTheme) => Application.Current!.UserAppTheme = appTheme;
    public AppTheme UserAppTheme => Application.Current!.UserAppTheme;
    public AppTheme RequestedTheme => Application.Current!.RequestedTheme;
}
