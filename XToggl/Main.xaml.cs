using System;
using System.Collections.Generic;
using System.ComponentModel;
using Parallel = System.Threading.Tasks;
using Xamarin.Forms;

using Toggl;
using Toggl.Extensions;
using Toasts.Forms.Plugin.Abstractions;

namespace XToggl
{
	public partial class Main : MasterDetailPage
	{
		private TimeEntry _startedTimeEntry = null;
		private DateTime? _startedDateTime;

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
			list.ItemTapped += async (sender, e) => {
				var project = e.Item as Project;
				await Navigation.PushAsync(new ProjectDetail(project));
			};
		}

		public void Start(object sender, EventArgs e)
		{
			if (_startedDateTime.HasValue) {
				App.Notificator.Notify(ToastNotificationType.Info, 
					"XToggl Message", "Please stop other tasks before starting a new one", TimeSpan.FromSeconds(2));
				return;
			}

			var btn = ((Button)sender);
			btn.IsVisible = false;

			var project = btn.CommandParameter as Toggl.Project;

			_startedDateTime = DateTime.Now;

			Parallel.Task.Factory
				.StartNew (() => AddTimeEntry (project))
				.ContinueWith ((entry) => ChangeButtonVisibility (btn.ParentView as StackLayout, 2, true));

		}

		public void Stop(object sender, EventArgs e)
		{
			var btn = ((Button)sender);
			btn.IsVisible = false;

			Parallel.Task.Factory
				.StartNew (() => EditTimeEntry ())
				.ContinueWith ((entry) => 
					{
						ChangeButtonVisibility (btn.ParentView as StackLayout, 1, true);
						_startedTimeEntry = null;
						_startedDateTime = null;
					});
			

		}

		public void ChangePage(object sender, EventArgs e)
		{
			var btn = sender as Button;
//			App.Notificator.Notify (ToastNotificationType.Info, "", btn.Text, TimeSpan.FromSeconds (5.0), null);

			if(btn.Text.ToLower() == "gps")
				Navigation.PushAsync(new GpsPage());
			else if(btn.Text.ToLower() == "nfg")
				Navigation.PushAsync(new GpsPage());
		}

		private void AddTimeEntry(Project project) 
		{
			_startedTimeEntry = App.Toggl.TimeEntry.Add (
				new TimeEntry {
					IsBillable = false,
					CreatedWith = "TogglAPI.Net",
					Start = _startedDateTime.Value.ToIsoDateStr (),
					Duration = _startedDateTime.Value.ToTogglStartDuration (),
					WorkspaceId = project.WorkspaceId,
					ProjectId = project.Id
				});
		}

		private void EditTimeEntry() 
		{
			var now = DateTime.Now;
			_startedTimeEntry.Stop = now.ToIsoDateStr ();
			_startedTimeEntry.Duration = _startedDateTime.Value.DifferenceInSeconds (now);

			App.Toggl.TimeEntry.Edit (_startedTimeEntry);
		}

		private void ChangeButtonVisibility(StackLayout layout, int btnIndex, bool visible)
		{
			Device.BeginInvokeOnMainThread (() => {
				var stopBtn = layout.Children [btnIndex] as Button;
				stopBtn.IsVisible = visible;
			});
		}

	}
}

