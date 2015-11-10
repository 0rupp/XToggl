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
		}
	}
}

