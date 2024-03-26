using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp.Services;
public interface INavigationService
{
	Task GoToAsync(ShellNavigationState state);
	Task GoToAsync(ShellNavigationState state, bool animate);
	Task GoToAsync(ShellNavigationState state, IDictionary<string, object> parameters);
	Task GoToAsync(ShellNavigationState state, bool animate, IDictionary<string, object> parameters);
	Task GoToAsync(ShellNavigationState state, ShellNavigationQueryParameters shellNavigationQueryParameters);
	Task GoToAsync(ShellNavigationState state, bool animate, ShellNavigationQueryParameters shellNavigationQueryParameters);
	Task GoBackAsync() => Shell.Current.GoToAsync("..", true);
	Task GoBackAsync(IDictionary<string, object> parameters) => Shell.Current.GoToAsync("..", parameters);

	Task PopBack();
}
