using System;
using System.Collections.Generic;

using Xamarin.Forms;
using XToggl.Calendar;

namespace XToggl
{
	public partial class CalendarPage : ContentPage
	{
		public CalendarPage ()
		{
			InitializeComponent ();

			var eventProvider = DependencyService.Get<IEventProvider> ();
			var data = eventProvider.GetEventsFromNow ();
			list.ItemsSource = data;
			list.ItemSelected += (sender, e) => {
				((ListView)sender).SelectedItem = null;
			};
		}
	}
}

