using System.Collections.ObjectModel;
using ViewPagerForms.Forms;
using Xamarin.Forms;

namespace ViewPagerDemo
{
    public partial class ViewPagerDemoPage : ContentPage
    {
        public ViewPagerDemoPage()
        {
            InitializeComponent();
            BindingContext = new ViewModel();
            var v = new ViewPagerControl();
        }
        //void Handle_Clicked_Increase(object sender, System.EventArgs e)
        //{
        //    MenuStack.HeightRequest += 10;
        //}

        //void Handle_Clicked_Insert(object sender, System.EventArgs e)
        //{
        //    (BindingContext as ViewModel).ListItems.Add((BindingContext as ViewModel).ListItems.Count);
        //}

        //void Handle_Clicked_Remove(object sender, System.EventArgs e)
        //{
        //    (BindingContext as ViewModel).ListItems.RemoveAt((BindingContext as ViewModel).ListItems.Count - 1);
        //}
    }

    public class ViewModel
    {
        public ObservableCollection<int> ListItems { get; set; }

        public ViewModel()
        {
            ListItems = new ObservableCollection<int>() { 1, 2, 3, 4, 5, 6, 9 };
        }
    }
}
