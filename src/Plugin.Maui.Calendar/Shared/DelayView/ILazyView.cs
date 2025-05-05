using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Maui.Calendar.Controls;
public interface ILazyView
{
	View Content { get; set; }

	bool IsLoaded { get; }

	void LoadView();
}