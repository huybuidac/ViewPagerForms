using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using ViewPagerForms;

namespace ViewPagetDemoNuget.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            ViewPagerRenderer.Init(true);

            LoadApplication(new App());
            return base.FinishedLaunching(app, options);
        }
    }
}
