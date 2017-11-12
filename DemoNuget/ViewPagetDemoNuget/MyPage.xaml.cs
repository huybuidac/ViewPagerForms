using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace ViewPagetDemoNuget
{
    public partial class MyPage : ContentPage
    {
        public MyPage()
        {
            InitializeComponent();
            BindingContext = new ViewModel();
        }
    }

    public class ViewModel
    {
        public ObservableCollection<int> ListItems { get; set; }
        public int Position { get; set; }

        public ViewModel()
        {
            ListItems = new ObservableCollection<int>() { 1, 2, 3, 4, 5, 6, 9 };
            Position = 5;
        }
    }
}
