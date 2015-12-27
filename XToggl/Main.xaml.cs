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
using XLabs.Caching;
using XLabs.Ioc;
using System.Collections.ObjectModel;

namespace XToggl
{
	public partial class Main : MasterDetailPage
	{
		private TimeEntry _startedTimeEntry;
		private Project _selectedProject;
		private DateTime? _startedDateTime;
		private Event _upcomingEvent;
		private ObservableCollection<Toggl.Project> _projects;

		private ISimpleCache _cache;
		private string ProjectsKey = "projects";

		public Main ()
		{
			InitializeComponent ();

			_cache = Resolver.Resolve<ISimpleCache>();

			list.ItemSelected += (sender, e) => {
				((ListView)sender).SelectedItem = null;
			};
			Parallel.Task.Factory
				.StartNew (() => InitializeEventProvider ());

			CheckKeyAndDownloadNewContent ();
		}

		/// </summary>
		private void CheckKeyAndDownloadNewContent()
		{
			if (_cache == null)
			{
				throw new ArgumentNullException(
					"_cacheService",
					new Exception("Native SimpleCache implementation wasn't found."));
			}

			var keyValue = _cache.Get<List<Toggl.Project>>(ProjectsKey);
			_projects = keyValue != null ? new ObservableCollection<Toggl.Project> (keyValue) : 
				new ObservableCollection<Toggl.Project> ();

			list.ItemsSource = _projects;
			Parallel.Task.Factory
				.StartNew (() => InitUI ())
				.ContinueWith ((task) => InitCurrentEntry ());
		}

		//_cache.FlushAll()

		private void InitializeEventProvider()
		{
			var eventProvider = DependencyService.Get<IEventProvider> ();
			eventProvider.Init ();
			_upcomingEvent = eventProvider.GetNextEvent ();

			var eventNotification = DependencyService.Get<IEventNotification> ();
			eventNotification.RegisterEvent (DateTime.Now.AddSeconds (3), PrintMsg);
		}

		private void InitCurrentEntry()
		{
			var currentEntry = App.Toggl.TimeEntry.Current();
			if (currentEntry.Id.HasValue) {
				_startedTimeEntry = currentEntry;
				_startedDateTime = DateTime.Parse (currentEntry.Start);
			}
			Device.BeginInvokeOnMainThread (() => {
				startBtn.IsEnabled = _startedTimeEntry == null;
				startBtn.IsVisible = _startedTimeEntry == null;
				stopBtn.IsVisible = !startBtn.IsVisible;
			});
		}

		private void InitUI()
		{
			Parallel.Task.Factory
				.StartNew (() => Device.BeginInvokeOnMainThread (() => {
					var cnt = _projects.Count;
					var name = App.User.FullName;
					header.Text = string.Format ("{0} Project{1} for {2}", cnt, (cnt == 1 ? "" : "s"), name.Replace("_", " ").Replace("r", "R"));
					list.ItemsSource = _projects;
					list.ItemTapped += (sender, e) => {
						_selectedProject = e.Item as Project;
						selectedProjectText.Text = _selectedProject.Name;
					};
				}))
				.ContinueWith ( (task) => {
					_projects = new ObservableCollection<Toggl.Project>(App.Toggl.Project.List());
					_cache.Remove(ProjectsKey);
					_cache.Add(ProjectsKey, _projects.ToList());
				});
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

			Parallel.Task.Factory
				.StartNew (() => AddTimeEntry (_selectedProject))
				.ContinueWith ((entry) => ChangeButtonVisibility (stopBtn));
		}

		public void Stop(object sender, EventArgs e)
		{
			var btn = ((Button)sender);
			btn.IsVisible = false;

			Parallel.Task.Factory
				.StartNew (() => EditTimeEntry ())
				.ContinueWith ((entry) => 
					{
						ChangeButtonVisibility (startBtn);
						_startedTimeEntry = null;
						_startedDateTime = null;
					});

		}

		public void ProjectDetail(object sender, EventArgs e)
		{
			var btn = ((Button)sender);
			var project = btn.CommandParameter as Project;
			Navigation.PushAsync (new ProjectDetail (project));
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

		private void ChangeButtonVisibility(Button btn)
		{
			Device.BeginInvokeOnMainThread (() => {
				btn.IsVisible = !btn.IsVisible;
			});
		}

	}
}

