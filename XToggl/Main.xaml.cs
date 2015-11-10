using System;
using System.Collections.Generic;

using Xamarin.Forms;

using Toggl;
using Toggl.Extensions;

namespace XToggl
{
	public partial class Main : ContentPage
	{
		private DateTime? _startedDateTime = null;

		public Main ()
		{
			InitializeComponent ();

			var u = App.Toggl.User.GetCurrent();
			header.Text = u.FullName;

			var projects = App.Toggl.Project.List ();
			var cnt = projects.Count;

			header.Text = cnt + " project" + (cnt == 1 ? "" : "s");
			list.ItemsSource = projects;
			list.ItemSelected += (sender, e) => {
				((ListView)sender).SelectedItem = null;
			};
		}

		public void Start(object sender, EventArgs e)
		{
			if (_startedDateTime.HasValue) {
				return;
			}

			var btn = ((Button)sender);
			btn.IsVisible = false;

			var parent = btn.ParentView as StackLayout;
			var stopBtn = parent.Children [2] as Button;
			stopBtn.IsVisible = true;

			_startedDateTime = DateTime.Now;
		}

		public void Stop(object sender, EventArgs e)
		{
			var btn = ((Button)sender);
			btn.IsVisible = false;

			var project = btn.CommandParameter as Toggl.Project;

			var now = DateTime.Now;
			var timeEntry = new TimeEntry () {
				IsBillable = false,
				CreatedWith = "TogglAPI.Net",
				Stop = now.ToIsoDateStr (),
				Start = _startedDateTime.Value.ToIsoDateStr (),
				Duration = now.Subtract (_startedDateTime.Value).Seconds,
				WorkspaceId = project.WorkspaceId
			};
			App.Toggl.TimeEntry.Add (timeEntry);

			_startedDateTime = null;

			var parent = btn.ParentView as StackLayout;
			var startBtn = parent.Children [1] as Button;
			startBtn.IsVisible = true;
		}

	}
}

