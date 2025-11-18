using System.Collections;
using System.Globalization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.Calendar.Controls.Interfaces;
using Plugin.Maui.Calendar.Controls.SelectionEngines;
using Plugin.Maui.Calendar.Controls.ViewLayoutEngines;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Interfaces;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Styles;
using Plugin.Maui.Calendar.Shared.Extensions;
using System.Collections.Specialized;
using System.Collections.ObjectModel;


namespace Plugin.Maui.Calendar.Controls;
public partial class Calendar : ContentView, IDisposable
{
	/// <summary>
	/// Event that is triggered when the month changes.
	/// </summary>
	public event EventHandler<MonthChangedEventArgs> MonthChanged;
	public event EventHandler SwipedLeft;
	public event EventHandler SwipedRight;
	public event EventHandler SwipedUp;
	public event EventHandler SwipedDown;
}
