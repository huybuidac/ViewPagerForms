using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using UIKit;
using ViewPagerForms;
using ViewPagerForms.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ViewPagerControl), typeof(ViewPagerRenderer))]
namespace ViewPagerForms
{
    public interface IViewPagerRenderer
    {
        UIViewController CurrentViewController { get; set; }
    }

    public class ViewPagerRenderer : ViewRenderer<ViewPagerControl, UIView>, IUIPageViewControllerDataSource, IViewPagerRenderer
    {
        UIPageViewController _pageController;
        UIViewController _currentViewController;

        readonly IDictionary<object, UIViewController> _controllers = new Dictionary<object, UIViewController>();
        INotifyCollectionChanged _itemSourceEvent;
        private bool _forceUpdateVC;

        public override UIViewController ViewController => _pageController;

        UIViewController IViewPagerRenderer.CurrentViewController
        {
            get => _currentViewController;
            set
            {
                _currentViewController = value;
                if (value != null && Element != null)
                {
                    foreach (var kv in _controllers)
                    {
                        if (kv.Value == value)
                        {
                            if (Element.ItemsSource != null)
                            {
                                Element.Position = Element.ItemsSource.IndexOf(kv.Key);
                            }
                            break;
                        }
                    }
                }
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<ViewPagerControl> e)
        {
            base.OnElementChanged(e);
            if (Control == null)
            {
                _pageController = new UIPageViewController(UIPageViewControllerTransitionStyle.Scroll,
                                                           UIPageViewControllerNavigationOrientation.Horizontal,
                                                           UIPageViewControllerSpineLocation.None);
                _pageController.DataSource = this;
                SetNativeControl(_pageController.View);
            }
            if (e.OldElement != null)
            {
                if (e.OldElement.ItemsSource is INotifyCollectionChanged)
                {
                    (e.OldElement.ItemsSource as INotifyCollectionChanged).CollectionChanged -= ViewPager_CollectionChanged;
                }
            }
            if (e.NewElement != null)
            {
                var element = e.NewElement;
                _itemSourceEvent = element.ItemsSource as INotifyCollectionChanged;
                if (_itemSourceEvent != null)
                {
                    _itemSourceEvent.CollectionChanged += ViewPager_CollectionChanged;
                }
                if (element.ItemsSource != null)
                {
                    ShowViewByIndex(Element.Position);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_itemSourceEvent != null)
                {
                    _itemSourceEvent.CollectionChanged -= ViewPager_CollectionChanged;
                    _itemSourceEvent = null;
                }
                foreach (var vc in _controllers.Values)
                {
                    vc.Dispose();
                }
                _controllers.Clear();
            }
            base.Dispose(disposing);
        }

        void ViewPager_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _forceUpdateVC = true;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ShowViewByIndex(Element.Position);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        if (_controllers.ContainsKey(item))
                        {
                            _controllers[item].Dispose();
                            _controllers.Remove(item);
                            if (Element.ItemsSource.Count() > 0)
                            {
                                var newIndex = Element.Position;
                                if (newIndex == Element.ItemsSource.Count())
                                {
                                    newIndex--;
                                }
                                ShowViewByIndex(newIndex);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (var item in e.OldItems)
                    {
                        if (_controllers.ContainsKey(item))
                        {
                            _controllers[item].Dispose();
                            _controllers.Remove(item);
                        }
                    }
                    ShowViewByIndex(Element.Position);
                    break;
                case NotifyCollectionChangedAction.Move:
                    ShowViewByIndex(Element.Position);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var vc in _controllers.Values)
                    {
                        vc.Dispose();
                    }
                    _controllers.Clear();
                    ShowViewByIndex(Element.Position);
                    break;
            }
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == ViewPagerControl.ItemsSourceProperty.PropertyName)
            {
                if (_itemSourceEvent != null)
                {
                    _itemSourceEvent.CollectionChanged -= ViewPager_CollectionChanged;
                }
                foreach (var vc in _controllers.Values)
                {
                    vc.Dispose();
                }
                _controllers.Clear();
                if (Element.ItemsSource != null)
                {
                    _itemSourceEvent = Element.ItemsSource as INotifyCollectionChanged;
                    if (_itemSourceEvent != null)
                    {
                        _itemSourceEvent.CollectionChanged += ViewPager_CollectionChanged;
                    }
                    ShowViewByIndex(0);
                }
            }
            else if (e.PropertyName == ViewPagerControl.PositionProperty.PropertyName)
            {
                ShowViewByIndex(Element.Position, false);
            }
        }

        public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            return Control.GetSizeRequest(widthConstraint, heightConstraint, 44, 44);
        }

        private void ShowViewByIndex(int index, bool animate = false)
        {
            int? count = Element?.ItemsSource?.Count();
            if (count.HasValue && index > -1 && index < count)
            {
                var context = Element.ItemsSource.ElementAt(index);
                UIViewController controller;
                if (!_controllers.TryGetValue(context, out controller))
                {
                    _controllers[context] = controller = new ContentViewController(Element, (IViewPagerRenderer)this, context);
                }
                ShowViewByController(controller, animate);
            }
        }

        private void ShowViewByController(UIViewController controller, bool animate = false)
        {
            if (!_pageController.ViewControllers.Any() || controller != _pageController.ViewControllers[0] || _forceUpdateVC)
                _pageController.SetViewControllers(new[] { controller }, UIPageViewControllerNavigationDirection.Forward, false, null);
            _forceUpdateVC = false;
        }

        UIViewController IUIPageViewControllerDataSource.GetPreviousViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
        {
            UIViewController output = null;
            var curViewController = referenceViewController as ContentViewController;
            var curContext = curViewController?.Context;
            if (curContext != null)
            {
                var length = Element.ItemsSource.Count();
                var prevIndex = Element.ItemsSource.IndexOf(curContext) - 1;
                if (prevIndex < 0 && Element.Infinite)
                {
                    prevIndex = length - 1;
                }
                if (prevIndex >= 0 && prevIndex < length)
                {
                    var prevContext = Element.ItemsSource.ElementAt(prevIndex);
                    if (!_controllers.TryGetValue(prevContext, out output))
                    {
                        _controllers[prevContext] = output = new ContentViewController(Element, (IViewPagerRenderer)this, prevContext);
                    }
                }
                this.Log($"CurContext={curContext} nextIndex={prevIndex}");
            }
            return output;
        }

        UIViewController IUIPageViewControllerDataSource.GetNextViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
        {
            UIViewController output = null;
            var curViewController = referenceViewController as ContentViewController;
            var curContext = curViewController?.Context;
            if (curContext != null)
            {
                var length = Element.ItemsSource.Count();
                var nextIndex = Element.ItemsSource.IndexOf(curContext) + 1;
                if (nextIndex >= length && Element.Infinite)
                {
                    nextIndex = 0;
                }
                if (nextIndex >= 0 && nextIndex < length)
                {
                    var nextContext = Element.ItemsSource.ElementAt(nextIndex);
                    if (!_controllers.TryGetValue(nextContext, out output))
                    {
                        _controllers[nextContext] = output = new ContentViewController(Element, (IViewPagerRenderer)this, nextContext);
                    }
                }
                this.Log($"CurContext={curContext} nextIndex={nextIndex}");
            }
            return output;
        }

        class ContentViewController : UIViewController
        {
            readonly WeakReference<IViewPagerRenderer> _viewPagerRenderer;
            readonly WeakReference<object> _context;
            readonly WeakReference<ViewPagerControl> _parent;
            IVisualElementRenderer _renderer;

            public object Context
            {
                get
                {
                    object context;
                    _context.TryGetTarget(out context);
                    return context;
                }
            }

            public override void ViewDidAppear(bool animated)
            {
                base.ViewDidAppear(animated);
                IViewPagerRenderer viewPagerRenderer;
                _viewPagerRenderer.TryGetTarget(out viewPagerRenderer);
                if (viewPagerRenderer != null)
                {
                    viewPagerRenderer.CurrentViewController = this;
                }
            }

            public ContentViewController(ViewPagerControl parent, IViewPagerRenderer viewPagerRender, object bindingContext)
            {
                _viewPagerRenderer = new WeakReference<IViewPagerRenderer>(viewPagerRender);
                _context = new WeakReference<object>(bindingContext);
                _parent = new WeakReference<ViewPagerControl>(parent);
            }

            public override void ViewDidLoad()
            {
                base.ViewDidLoad();
                object context;
                ViewPagerControl parent;
                if (_renderer == null
                    && _context.TryGetTarget(out context)
                    && _parent.TryGetTarget(out parent))
                {
                    VisualElement view;
                    var dataTemplate = parent.ItemTemplate;
                    if (dataTemplate is DataTemplateSelector)
                    {
                        view = ((DataTemplateSelector)dataTemplate).SelectTemplate(context, parent).CreateContent() as VisualElement;
                    }
                    else
                    {
                        view = dataTemplate.CreateContent() as VisualElement;
                    }
                    view.BindingContext = context;
                    view.Parent = parent;
                    _renderer = Platform.CreateRenderer(view);
                    View = _renderer.NativeView;
                }
            }

            public override void ViewDidLayoutSubviews()
            {
                base.ViewDidLayoutSubviews();
                ViewPagerControl parent = null;
                if (_renderer != null && _renderer.Element != null && _parent.TryGetTarget(out parent))
                {
                    var size = parent.Bounds.Size;
                    var x = 0d;
                    var y = 0d;
                    var width = size.Width;
                    var height = size.Height;
                    if (_renderer.Element is Xamarin.Forms.View)
                    {
                        var margin = (_renderer.Element as Xamarin.Forms.View).Margin;
                        x = margin.Left;
                        y = margin.Top;
                        width -= margin.Left + margin.Right;
                        height -= margin.Top + margin.Bottom;
                    }
                    _renderer.Element.Layout(new Rectangle(x, y, width, height));
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (_renderer != null)
                    {
                        var element = _renderer.Element;
                        if (_renderer.Element != null)
                        {
                            _renderer.Element.Parent = null;
                        }
                        _renderer.ViewController?.RemoveFromParentViewController();
                        _renderer.NativeView?.RemoveFromSuperview();
                        _renderer.Dispose();
                        _renderer = null;
                        if (element != null)
                        {
                            Platform.SetRenderer(element, null);
                        }
                    }
                }
                base.Dispose(disposing);
            }

        }

        public static void Init(bool log = false)
        {
            ObjectExtensions.AllowLog = log;
        }
    }
}
