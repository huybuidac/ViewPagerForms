using Android.Support.V4.View;
using ViewPagerForms;
using ViewPagerForms.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ViewPagerControl), typeof(ViewPagerRenderer))]
namespace ViewPagerForms
{
    public class ViewPagerRenderer : ViewRenderer<ViewPagerControl, ViewPager>
    {
        ViewPager _viewPager;
        private bool _ingorePositionChanged;

        protected override void OnElementChanged(ElementChangedEventArgs<ViewPagerControl> e)
        {
            base.OnElementChanged(e);
            if (Control == null)
            {
                _viewPager = new ViewPager(Context);
                _viewPager.AdapterChange += _viewPager_AdapterChange;
                _viewPager.PageSelected += _viewPager_PageSelected;
                SetNativeControl(_viewPager);
            }
            if (e.OldElement != null)
            {

            }
            if (e.NewElement != null)
            {
                var element = e.NewElement;
                if (element.ItemsSource != null)
                {
                    _viewPager.Adapter = new FormAdapter(element, _viewPager);
                    if (element.Infinite)
                    {
                        _viewPager.SetCurrentItem(FormAdapter.Max / 2 + element.Position, false);
                    }
                    else
                    {
                        _viewPager.SetCurrentItem(element.Position, false);
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_viewPager != null)
                {
                    _viewPager.Adapter = null;
                    _viewPager.AdapterChange -= _viewPager_AdapterChange;
                    _viewPager.PageSelected -= _viewPager_PageSelected;
                    _viewPager = null;
                }
            }
            base.Dispose(disposing);
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            (_viewPager?.Adapter as FormAdapter)?.UpdateLayout();
        }

        void _viewPager_AdapterChange(object sender, ViewPager.AdapterChangeEventArgs e)
        {
            if (e.OldAdapter != null)
            {
                e.OldAdapter.Dispose();
            }
        }

        void _viewPager_PageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            _ingorePositionChanged = true;
            if (Element.Infinite)
            {
                var xPos = (e.Position - FormAdapter.Max / 2) % Element.ItemsSource.Count();
                if (xPos < 0)
                {
                    xPos = xPos % Element.ItemsSource.Count();
                    xPos += Element.ItemsSource.Count();
                }
                Element.Position = xPos;
            }
            else
            {
                Element.Position = e.Position;
            }
            _ingorePositionChanged = false;
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == ViewPagerControl.ItemsSourceProperty.PropertyName)
            {
                _viewPager.Adapter = new FormAdapter(Element, _viewPager);
            }
            else if (e.PropertyName == ViewPagerControl.PositionProperty.PropertyName)
            {
                if (!_ingorePositionChanged)
                {
                    if (Element.Infinite)
                    {
                        _viewPager.SetCurrentItem(FormAdapter.Max / 2 + Element.Position, false);
                    }
                    else
                    {
                        _viewPager.SetCurrentItem(Element.Position, false);
                    }
                }
            }
        }
    }
}
