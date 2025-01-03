using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp.Views;

public partial class EditEventPage : ContentPage
{
    public EditEventPage(EditEventPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}