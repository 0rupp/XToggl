using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Toggl;
using Toggl.QueryObjects;

namespace XToggl
{
	public partial class ProjectDetail : ContentPage
	{
		public ProjectDetail (Project p)
		{
			InitializeComponent ();

			var timeEntries = App.Toggl.TimeEntry.List (new TimeEntryParams { ProjectId = p.Id });
			list.ItemsSource = timeEntries;
			list.ItemSelected += (sender, e) => {
				((ListView)sender).SelectedItem = null;
			};
			var cnt = timeEntries.Count;
			list.Header = new Label { Text = cnt + " time entr" + (cnt == 1 ? "y" : "ies"), XAlign = TextAlignment.Center };
		}
	}
}

