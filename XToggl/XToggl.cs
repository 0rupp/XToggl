using System;

using Xamarin.Forms;
using Toasts.Forms.Plugin.Abstractions;
using XLabs.Caching;
using XLabs.Ioc;
using XToggl.Calendar;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Threading.Tasks;

namespace XToggl
{
	public class App : Application
	{
		// Cache Items
		private static string UserKey = "user";
		private static Toggl.User _user;

		private static string EventsKey = "events";
		private static IList<Event> _upcomingEvents;

		private static string GpsPositionsKey = "gpsPositions";
		private static IList<GpsPosition> _gpsPositions;

		public static IList<Event> UpcomingEvents 
		{
			get 
			{
				if (_upcomingEvents == null) {
					ISimpleCache _cache = Resolver.Resolve<ISimpleCache>();
					_upcomingEvents  = _cache.Get<List<Event>> (EventsKey);
				}
				if (_upcomingEvents == null) {
					_upcomingEvents = new List<Event> ();
				}
				return _upcomingEvents;
			}
		}

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

		public async static Task<IList<GpsPosition>> GetGpsPositions()
		{
			if (_gpsPositions == null) {
				ISimpleCache _cache = Resolver.Resolve<ISimpleCache>();	
				_gpsPositions = _cache.Get<List<GpsPosition>> (GpsPositionsKey);
			}
			if (_gpsPositions == null) {
				ISimpleCache _cache = Resolver.Resolve<ISimpleCache>();
				_cache.Remove(GpsPositionsKey);
				_gpsPositions = await GetPositionsFromApi();
				if (_gpsPositions == null)
					throw new InvalidOperationException("GpsPositions");
				_cache.Add (GpsPositionsKey, _gpsPositions);
			}
			return _gpsPositions;
		}

		// App Items
		const string apiKey = "211b283336f7055a287578fd1eed09bd";

		private static Toggl.Toggl _t = new Toggl.Toggl(apiKey);

		public static Toggl.Toggl Toggl { get { return _t; } }

		public static IToastNotificator Notificator { get { return DependencyService.Get<IToastNotificator> (); } }

		public App ()
		{
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

		public static bool AddUpcomingEvent(Event calEvent) 
		{
			bool toAdd = !UpcomingEvents.Contains (calEvent);
			if (toAdd) 
			{
				_upcomingEvents.Add (calEvent);
				ISimpleCache _cache = Resolver.Resolve<ISimpleCache>();	
				_cache.Remove (EventsKey);
				_cache.Set(EventsKey, _upcomingEvents);
			}
			return toAdd;
		}

		public static bool AddGpsPosition(GpsPosition gpsPosition) 
		{
			bool toAdd = !_gpsPositions.Contains (gpsPosition);
			if (toAdd)
			{
				_gpsPositions.Add (gpsPosition);
				ISimpleCache _cache = Resolver.Resolve<ISimpleCache>();	
				_cache.Remove (GpsPositionsKey);
				_cache.Set(GpsPositionsKey, _gpsPositions);

				RestAPI.SetGpsPositionsWithProject (gpsPosition);
			}
			return toAdd;
		}

		private static async Task<List<GpsPosition>> GetPositionsFromApi() 
		{
			return await RestAPI.GetGpsPositionsWithProjects ().ConfigureAwait(false);
		}
	}
}