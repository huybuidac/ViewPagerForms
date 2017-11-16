using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Android.Support.V4.View;
using Android.Views;
using ViewPagerForms.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using AView = Android.Views;

namespace ViewPagerForms
{
    public class FormAdapter : PagerAdapter
    {
        public const int Max = 50000;

        private ViewPagerControl _element;
        private INotifyCollectionChanged _collectionChanged;
        private readonly IDictionary<object, IVisualElementRenderer> _dicViews = new Dictionary<object, IVisualElementRenderer>();
        private readonly IList<WeakReference<IVisualElementRenderer>> _trashStore = new List<WeakReference<IVisualElementRenderer>>();

        private bool _ignoreRemove;

        public FormAdapter(ViewPagerControl viewPager, ViewPager container)
        {
            _element = viewPager;
            _collectionChanged = _element.ItemsSource as INotifyCollectionChanged;
            if (_collectionChanged != null)
            {
                _collectionChanged.CollectionChanged += _collectionChanged_CollectionChanged;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_element != null)
                {
                    _element = null;
                }
                if (_collectionChanged != null)
                {
                    _collectionChanged.CollectionChanged -= _collectionChanged_CollectionChanged;
                    _collectionChanged = null;
                }
                foreach (var render in _dicViews.Values)
                {
                    DisposeRender(render, true);
                }
                _dicViews.Clear();

                IVisualElementRenderer renderTrash = null;
                foreach (var weakref in _trashStore)
                {
                    if (weakref.TryGetTarget(out renderTrash))
                    {
                        DisposeRender(renderTrash, true);
                    }
                }
                _trashStore.Clear();
            }
            base.Dispose(disposing);
        }

        private void DisposeRender(IVisualElementRenderer render, bool dispose = false)
        {
            var formView = render.Element;
            render.ViewGroup?.RemoveFromParent();
            if (dispose)
            {
                render.Dispose();
            }
            else
            {
                var addToTrash = true;
                IVisualElementRenderer model = null;
                foreach (var weakref in _trashStore)
                {
                    if (weakref.TryGetTarget(out model))
                    {
                        if (model == render)
                        {
                            addToTrash = false;
                            break;
                        }
                    }
                }
                if (addToTrash)
                {
                    _trashStore.Add(new WeakReference<IVisualElementRenderer>(render));
                }
                else
                {
                    this.Log($"Context={formView?.BindingContext} Added to trash");
                }
            }

            if (formView != null)
            {
                Platform.SetRenderer(formView, null);
                formView.Parent = null;
            }
        }

        void _collectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Log($"Action={e.Action}");
            var oldItems = e.OldItems;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in oldItems)
                    {
                        if (_dicViews.ContainsKey(item))
                        {
                            DisposeRender(_dicViews[item]);
                            _dicViews.Remove(item);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (var item in oldItems)
                    {
                        if (_dicViews.ContainsKey(item))
                        {
                            DisposeRender(_dicViews[item], true);
                            _dicViews.Remove(item);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
            }
            NotifyDataSetChanged();
        }

        public override int Count
        {
            get
            {
                return _element.Infinite ? Max : _element.ItemsSource.Count();
            }
        }

        public override bool IsViewFromObject(AView.View view, Java.Lang.Object obj)
        {
            return view == obj;
        }

        public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
        {
            IVisualElementRenderer render;

            int realPosition = GetRealPosition(position);

            this.Log($"Position={realPosition}");
            var context = _element.ItemsSource.ElementAt(realPosition);

            if (!_dicViews.TryGetValue(context, out render))
            {
                VisualElement view;

                var dataTemplate = _element.ItemTemplate;
                if (dataTemplate is DataTemplateSelector)
                {
                    view = ((DataTemplateSelector)dataTemplate).SelectTemplate(context, _element).CreateContent() as VisualElement;
                }
                else
                {
                    view = dataTemplate.CreateContent() as VisualElement;
                }

                view.BindingContext = context;
                view.Parent = _element;
                view.Layout(_element.Bounds);

                _dicViews[context] = render = Platform.CreateRenderer(view);

                Platform.SetRenderer(view, render);
            }
            if (render.ViewGroup.Parent == container)
            {
                _ignoreRemove = true;
            }
            else
            {
                render.ViewGroup.RemoveFromParent();
                container.AddView(render.ViewGroup);
            }
            return render.ViewGroup;
        }

        private int GetRealPosition(int position)
        {
            var realPosition = position;
            if (_element.Infinite)
            {
                realPosition = (position - Max / 2) % _element.ItemsSource.Count();
                if (realPosition < 0)
                {
                    realPosition = realPosition % _element.ItemsSource.Count();
                    realPosition += _element.ItemsSource.Count();
                }
            }

            return realPosition;
        }

        public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object obj)
        {
            this.Log($"Position={GetRealPosition(position)}");

            if (!_ignoreRemove)
            {
                container.RemoveView(obj as AView.View);
            }
            _ignoreRemove = false;
        }

        public override int GetItemPosition(Java.Lang.Object @object)
        {
            var result = PositionNone;

            var render = @object as IVisualElementRenderer;
            var context = render?.Element.BindingContext;
            if (context != null && _element.ItemsSource.IndexOf(context) > -1)
            {
                result = PositionUnchanged;
            }
            else if (render != null)
            {
                DisposeRender(render);
            }
            this.Log($"Context={context} Result={result}");
            return result;
        }

        public void UpdateLayout()
        {
            foreach (var item in _dicViews.Values)
            {
                item.Element.Layout(_element.Bounds);
            }
        }
    }
}
