using System;
using XLabs.Web;
using XLabs.Ioc;
using System.Collections.Generic;
using XToggl.Calendar;
using System.Threading.Tasks;

namespace XToggl
{
	public class RestAPI
	{
		private static IRestClient _client = Resolver.Resolve<IRestClient> ();
		private static string _baseUrl = "http://ns377884.ip-5-196-89.eu/Dominik/x/";

		#region Events

		public async static Task<List<Event>> GetEventsWithProjects()
		{
			if (!App.User.Id.HasValue)
				throw new InvalidOperationException ("unauthorized user");

			string url = _baseUrl + "events/" + App.User.Id;
			var response = await _client.GetAsync<EventsResult>(url);
			return response.Events;
		}

		public async static void SetEventsWithProject(Event e)
		{
			if (!App.User.Id.HasValue)
				throw new InvalidOperationException ("unauthorized user");

			string url = _baseUrl + "event";
			await _client.PostAsync<Event> (url, e);
		}

		#endregion

		#region GpsPositions

		public async static Task<List<GpsPosition>> GetGpsPositionsWithProjects()
		{
			if (!App.User.Id.HasValue)
				throw new InvalidOperationException ("unauthorized user");

			string url = _baseUrl + "gps/" + App.User.Id;
			var response = await _client.GetAsync<GpsPositionsResult>(url);
			return response.GpsPositions;
		}

		public async static void SetGpsPositionsWithProject(GpsPosition p)
		{
			if (!App.User.Id.HasValue)
				throw new InvalidOperationException ("unauthorized user");

			string url = _baseUrl + "gps";
			await _client.PostAsync<GpsPosition> (url, p);
		}

		#endregion

	}
}

