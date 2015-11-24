using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Parallel = System.Threading.Tasks;
using Xamarin.Forms;

using Toggl;
using Toggl.Extensions;
using Toasts.Forms.Plugin.Abstractions;
using XToggl.Calendar;

namespace XToggl
{
	public partial class Main : MasterDetailPage
	{
		private TimeEntry _startedTimeEntry;
		private DateTime? _startedDateTime;
		private Event _upComingEvent;

		public Main ()
		{
			InitializeComponent ();

			var eventProvider = DependencyService.Get<IEventProvider> ();
			_upComingEvent = eventProvider.GetNextEvent ();

			var eventNotification = DependencyService.Get<IEventNotification> ();
			eventNotification.RegisterEvent (DateTime.Now.AddSeconds (3), PrintMsg);

			var currentEntry = App.Toggl.TimeEntry.Current();
			if (currentEntry.Id.HasValue) {
				_startedTimeEntry = currentEntry;
				_startedDateTime = DateTime.Parse (currentEntry.Start);
			}

			var u = App.Toggl.User.GetCurrent();
			var projects = App.Toggl.Project.List ();
			var cnt = projects.Count;

			header.Text = string.Format ("{0} Project{1} for {2}", cnt, (cnt == 1 ? "" : "s"), u.FullName);
			list.ItemsSource = projects;
			list.ItemSelected += (sender, e) => {
				((ListView)sender).SelectedItem = null;
			};

			list.ItemTapped += async (sender, e) => {
				var project = e.Item as Project;
				await Navigation.PushAsync(new ProjectDetail(project));
			};
		}


		private void PrintMsg() {
			AskForUpcomingEvent ();
		}

		private async void AskForUpcomingEvent() {
			var task = await DisplayAlert ("XToggl", "Do you want to start time tracking for Project 0 now?", "Yes", "No");
			if (task) {
				// später mal einen Bezug vom Event zum Projekt / TimeEntry herstellen !
				var p = list.ItemsSource.Cast<Project> ().First ();
				AddTimeEntry (p);
			}
			else {
				App.Notificator.Notify (ToastNotificationType.Info, "XToggl", "Time tracking not startet", TimeSpan.FromSeconds (1.0), null);
			}			
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
			if(btn.Text.ToLower() == "gps")
				Navigation.PushAsync(new GpsPage());
			else if(btn.Text.ToLower() == "nfc")
				Navigation.PushAsync(new GpsPage());
			else if(btn.Text.ToLower() == "calendar")
				Navigation.PushAsync(new CalendarPage());
		}

		private void AddTimeEntry(Project project) 
		{
			_startedDateTime = DateTime.Now;

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

