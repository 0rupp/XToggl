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

		public async static Task<List<Event>> GetEventsWithProjects()
		{
			if (!App.User.Id.HasValue)
				throw new InvalidOperationException ("unauthorized user");

			string url = "http://ns377884.ip-5-196-89.eu/Dominik/x/events/" + App.User.Id;
			var response = await _client.GetAsync<EventsResult>(url);
			return response.Events;
		}

		public async static void SetEventsWithProject(Event e)
		{
			if (!App.User.Id.HasValue)
				throw new InvalidOperationException ("unauthorized user");

			string url = "http://ns377884.ip-5-196-89.eu/Dominik/x/event";
			_client.PostAsync<Event> (url, e);
		}
	}
}

