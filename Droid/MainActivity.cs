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
using XLabs.Ioc;
using XLabs.Caching;
using XLabs.Caching.SQLite;
using XLabs.Serialization;
using System.IO;

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
			DependencyService.Register<EventNotification> ();
			DependencyService.Register<Geolocator> ();
			if (!Resolver.IsSet) {
				var documents = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
				var pathToDatabase = Path.Combine(documents, "xforms.db");



				var resolverContainer = new SimpleContainer();
				resolverContainer
					.Register<IJsonSerializer> (t => new SystemJsonSerializer())
					.Register<ISimpleCache> (
					t => new SQLiteSimpleCache (new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid (),
						new SQLite.Net.SQLiteConnectionString (pathToDatabase, true), t.Resolve<IJsonSerializer> ()));
				var json = resolverContainer.GetResolver ().Resolve<IJsonSerializer> ();

				Resolver.SetResolver(resolverContainer.GetResolver());
			}

			LoadApplication (new App ());
		}
	}
}

