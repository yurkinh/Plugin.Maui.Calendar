using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp.Services;
public class NavigationService : INavigationService
{
	public Task GoToAsync(ShellNavigationState state) => Shell.Current.GoToAsync(state, true);
	public Task GoToAsync(ShellNavigationState state, bool animate) => Shell.Current.GoToAsync(state, animate);
	public Task GoToAsync(ShellNavigationState state, IDictionary<string, object> parameters) => Shell.Current.GoToAsync(state, true, parameters);
	public Task GoToAsync(ShellNavigationState state, bool animate, IDictionary<string, object> parameters) => Shell.Current.GoToAsync(state, animate, parameters);
	public Task GoToAsync(ShellNavigationState state, ShellNavigationQueryParameters shellNavigationQueryParameters) => Shell.Current.GoToAsync(state, true, shellNavigationQueryParameters);
	public Task GoToAsync(ShellNavigationState state, bool animate, ShellNavigationQueryParameters shellNavigationQueryParameters) => Shell.Current.GoToAsync(state, animate, shellNavigationQueryParameters);
	public Task GoBackAsync() => Shell.Current.GoToAsync("..", true);
	public Task GoBackAsync(IDictionary<string, object> parameters) => Shell.Current.GoToAsync("..", parameters);

	public Task PopBack() => Shell.Current.Navigation.PopAsync();
}