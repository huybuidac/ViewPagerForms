using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using PropertyChanged;
using ViewPagerForms.Forms;

namespace ViewPagerDemo
{
    public partial class MyPage : ContentPage
    {
        public MyPage()
        {
            InitializeComponent();
            BindingContext = new ViewModel();
        }

        void Handle_Clicked2(object sender, System.EventArgs e)
        {
            viewpager.Position = 2;
        }

        void Handle_Clicked6(object sender, System.EventArgs e)
        {
            viewpager.Position = 6;
        }
    }

    [AddINotifyPropertyChangedInterface]
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
