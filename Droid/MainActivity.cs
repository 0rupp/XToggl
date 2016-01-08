using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using Toasts.Forms.Plugin.Droid;
using Xamarin.Forms;
using XLabs.Caching;
using XLabs.Caching.SQLite;
using XLabs.Forms;
using XLabs.Ioc;
using XLabs.Platform.Services.Geolocation;
using XLabs.Serialization;
using XLabs.Web;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Text;

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
				var serializer = new SystemJsonSerializer();

				resolverContainer
					.Register<IJsonSerializer> (t => serializer)
					.Register<IRestClient>(new JsonRestClient(serializer))
					.Register<ISimpleCache> (
					t => new SQLiteSimpleCache (new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid (),
						new SQLite.Net.SQLiteConnectionString (pathToDatabase, true), t.Resolve<IJsonSerializer> ()));
				
				Resolver.SetResolver(resolverContainer.GetResolver());
			}

			LoadApplication (new App ());
		}

	}
}

