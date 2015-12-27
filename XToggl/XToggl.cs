using System;

using Xamarin.Forms;
using Toasts.Forms.Plugin.Abstractions;
using XLabs.Caching;
using XLabs.Ioc;

namespace XToggl
{
	public class App : Application
	{
		// Cache Items
		private static string UserKey = "user";
		private static Toggl.User _user;

		public static Toggl.User User
		{
			get 
			{ 
				if (_user == null) {
					ISimpleCache _cache = Resolver.Resolve<ISimpleCache>();	
					_user = _cache.Get<Toggl.User> (UserKey);
				}
				if (_user == null) {
					ISimpleCache _cache = Resolver.Resolve<ISimpleCache>();
					_user = App.Toggl.User.GetCurrent ();
					_cache.Remove(UserKey);
					if (_user == null)
						throw new InvalidOperationException("User");
					_cache.Add (UserKey, _user);
				}
				return _user;
			} 
		}
		// App Items
		const string apiKey = "211b283336f7055a287578fd1eed09bd";

		private static Toggl.Toggl _t = new Toggl.Toggl(apiKey);

		public static Toggl.Toggl Toggl { get { return _t; } }

		public static IToastNotificator Notificator { get { return DependencyService.Get<IToastNotificator> (); } }



		public App ()
		{	
			// new Main ()
			MainPage = new NavigationPage (new Main());
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}

