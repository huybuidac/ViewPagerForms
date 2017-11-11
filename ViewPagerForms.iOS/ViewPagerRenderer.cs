using System;
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
    public class ViewPagerRenderer : ViewRenderer<ViewPagerControl, UIView>, IUIPageViewControllerDataSource
    {
        UIPageViewController _pageController;
        readonly IDictionary<object, UIViewController> _controllers = new Dictionary<object, UIViewController>();
        INotifyCollectionChanged _itemSourceEvent;

        public override UIViewController ViewController => _pageController;

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            if (_controllers != null)
            {
                foreach (var vc in _controllers.Values)
                {
                    vc.View.Frame = NativeView.Bounds;
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
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        if (_controllers.ContainsKey(item))
                        {
                            _controllers[item].Dispose();
                            _controllers.Remove(item);
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
                    break;
                case NotifyCollectionChangedAction.Move:
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
        }

        private void ShowViewByIndex(int index, bool animate = false)
        {
            var context = Element.ItemsSource.ElementAt(index);
            UIViewController controller;
            if (!_controllers.TryGetValue(context, out controller))
            {
                _controllers[context] = controller = new ContentViewController(Element, Element.ItemTemplate, context);
            }
            ShowViewByController(controller, animate);
        }

        private void ShowViewByController(UIViewController controller, bool animate = false)
        {
            _pageController.SetViewControllers(new[] { controller }, UIPageViewControllerNavigationDirection.Forward, false, null);
        }

        /// <summary>
        /// Callback of _pageController.SetViewControllers
        /// </summary>
        void _pageController_ViewControllersChanged(bool finished)
        {
        }

        /// <summary>
        /// Notify when change page
        /// </summary>
        void _pageController_DidFinishAnimating(object sender, UIPageViewFinishedAnimationEventArgs e)
        {
            if (e.Finished)
            {
                //_pageController.SetViewControllers(_controllers.Values.ToArray(), UIPageViewControllerNavigationDirection.Forward, false, null);
            }
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
                        _controllers[prevContext] = output = new ContentViewController(Element, Element.ItemTemplate, prevContext);
                    }
                }
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
                        _controllers[nextContext] = output = new ContentViewController(Element, Element.ItemTemplate, nextContext);
                    }
                }
            }
            return output;
        }

        class ContentViewController : UIViewController
        {
            readonly WeakReference<DataTemplate> _dataTemplate;
            readonly WeakReference<object> _context;
            readonly WeakReference<Element> _parent;
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

            public ContentViewController(Element parent, DataTemplate dataTemplate, object bindingContext)
            {
                _dataTemplate = new WeakReference<DataTemplate>(dataTemplate);
                _context = new WeakReference<object>(bindingContext);
                _parent = new WeakReference<Element>(parent);
            }

            public override void ViewDidLoad()
            {
                base.ViewDidLoad();
                DataTemplate dataTemplate;
                object context;
                Element parent;
                if (_renderer == null
                    && _dataTemplate.TryGetTarget(out dataTemplate)
                    && _context.TryGetTarget(out context)
                    && _parent.TryGetTarget(out parent))
                {
                    VisualElement view;
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
                if (_renderer.Element != null)
                {
                    _renderer.Element.Layout(View.Bounds.ToRectangle());
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (_renderer != null)
                    {
                        if (_renderer.Element != null)
                        {
                            _renderer.Element.Parent = null;
                        }
                        _renderer.ViewController?.RemoveFromParentViewController();
                        _renderer.NativeView?.Dispose();
                        _renderer.Dispose();
                        _renderer = null;
                    }
                }
                base.Dispose(disposing);
            }
        }
    }
}
