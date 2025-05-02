using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Maui.Calendar.Controls;
public abstract class LazyView : ContentView, ILazyView, IDisposable
{
	public new bool IsLoaded { get; protected set; }

	public abstract void LoadView();

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (Content is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected override void OnBindingContextChanged()
	{
		if (Content != null && Content is not ActivityIndicator)
		{
			Content.BindingContext = BindingContext;
		}
	}
}
