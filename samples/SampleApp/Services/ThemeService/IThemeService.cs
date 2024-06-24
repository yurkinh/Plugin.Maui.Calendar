namespace SampleApp.Services;
public interface IThemeService
{
    void SetTheme(AppTheme appTheme);
    AppTheme UserAppTheme { get; }
    AppTheme RequestedTheme { get; }
}
