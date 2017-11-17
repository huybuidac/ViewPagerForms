using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using PropertyChanged;
using ViewPagerForms.Forms;
using System.ComponentModel;

namespace ViewPagerDemo
{
    public partial class MyPage : ContentPage
    {
        private ViewModel _viewModel;
        public MyPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new ViewModel();
        }

        void Handle_Clicked2(object sender, System.EventArgs e)
        {
            viewpager.Position = 2;
        }

        void Handle_Clicked6(object sender, System.EventArgs e)
        {
            viewpager.Position = 6;
        }

        void Handle_Clicked_Remove_2(object sender, System.EventArgs e)
        {
            _viewModel.ListItems.RemoveAt(2);
        }

        void Handle_Clicked_Remove_6(object sender, System.EventArgs e)
        {
            _viewModel.ListItems.RemoveAt(6);
        }

        void Handle_Clicked_Insert_2(object sender, System.EventArgs e)
        {
            _viewModel.ListItems.Add(111111);
            _viewModel.ListItems.Insert(2, 999);
        }

        void Handle_Clicked_Change(object sender, System.EventArgs e)
        {
            _viewModel.ListItems = new ObservableCollection<int>() { 100, 200, 300, 400, 500, 600, 900 };
            _viewModel.dod();
        }
    }

    //[AddINotifyPropertyChangedInterface]
    public class ViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<int> ListItems { get; set; }
        public int Position { get; set; }

        public ViewModel()
        {
            ListItems = new ObservableCollection<int>() { 1, 2, 3, 4, 5, 6, 9 };
            Position = 5;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void dod()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListItems"));
        }
    }
}
