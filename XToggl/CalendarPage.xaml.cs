using System;
using System.Collections.Generic;

using Xamarin.Forms;
using XToggl.Calendar;
using XLabs.Caching;
using XLabs.Ioc;
using Toasts.Forms.Plugin.Abstractions;

namespace XToggl
{
	public partial class CalendarPage : ContentPage
	{
		public CalendarPage ()
		{
			InitializeComponent ();

			var eventProvider = DependencyService.Get<IEventProvider> ();
			var data = eventProvider.GetEventsFromNow ();
			list.Header = new Label { Text = data.Count + " events", XAlign = TextAlignment.Center };
			list.ItemsSource = data;
			list.ItemSelected += (sender, e) => {
				((ListView)sender).SelectedItem = null;
			};
			list.ItemTapped += (sender, e) => {
				var calEvent = e.Item as Event;
				Navigation.PushAsync (new Projects (calEvent));
			};
		}

		public void DeleteCache(object sender, EventArgs e) {
			ISimpleCache _cache = Resolver.Resolve<ISimpleCache> ();
			_cache.Remove ("events");
		}
	}
}

