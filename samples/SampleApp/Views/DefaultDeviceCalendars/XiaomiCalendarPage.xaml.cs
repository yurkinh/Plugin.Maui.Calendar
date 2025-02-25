using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp.Views;

public partial class XiaomiCalendarPage : ContentPage
{
    public XiaomiCalendarPage(XiaomiCalendarViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}