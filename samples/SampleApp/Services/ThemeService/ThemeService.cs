using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp.Services;
public class ThemeService : IThemeService
{
	public AppTheme UserAppTheme => Application.Current!.UserAppTheme;

	public AppTheme RequestedTheme => Application.Current!.RequestedTheme;

	public void SetTheme(AppTheme appTheme) => Application.Current!.UserAppTheme = appTheme;
}
