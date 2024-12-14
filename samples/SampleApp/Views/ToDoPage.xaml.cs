using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp.Views;

public partial class ToDoPage : ContentPage
{
    public ToDoPage(ToDoPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}