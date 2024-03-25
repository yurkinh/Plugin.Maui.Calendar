using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp.Services;
public interface IThemeService
{
	void SetTheme(AppTheme appTheme);
	AppTheme UserAppTheme { get; }
	AppTheme RequestedTheme { get; }
}
