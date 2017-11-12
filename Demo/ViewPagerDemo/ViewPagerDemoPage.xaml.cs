using System.Collections.ObjectModel;
using ViewPagerForms.Forms;
using Xamarin.Forms;

namespace ViewPagerDemos
{
    public partial class ViewPagerDemoPage : ContentPage
    {
        public ViewPagerDemoPage()
        {
            InitializeComponent();
            BindingContext = new ViewModel();
            //var con = new ViewPagerControl();
        }
    }


}
