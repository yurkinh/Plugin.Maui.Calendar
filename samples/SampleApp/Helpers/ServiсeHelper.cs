using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp.Helpers;
public static class ServiceHelper
{
	public static IServiceProvider Services { get; private set; }

	public static void Initialize(IServiceProvider serviceProvider) =>
		Services = serviceProvider;

	public static T GetService<T>() => Services.GetService<T>();
}
