using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using Android.Provider;
using Toasts.Forms.Plugin.Droid;
using XLabs.Forms;
using Xamarin.Forms;
using XLabs.Platform.Services.Geolocation;
using System.Collections.Generic;

namespace XToggl.Droid
{
	[Activity (Label = "XToggl", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : XFormsApplicationDroid
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			global::Xamarin.Forms.Forms.Init (this, bundle);

			ToastNotificatorImplementation.Init();

			EventProvider.ContentResolver = ContentResolver;
			DependencyService.Register<EventProvider> ();
			DependencyService.Register<Geolocator> ();

			LoadApplication (new App ());

		}
	}
}

