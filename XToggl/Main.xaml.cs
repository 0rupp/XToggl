using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Parallel = System.Threading.Tasks;
using Xamarin.Forms;

using Diag = System.Diagnostics;
using Toggl;
using Toggl.Extensions;
using Toasts.Forms.Plugin.Abstractions;
using XToggl.Calendar;
using XLabs.Caching;
using XLabs.Ioc;
using XLabs.Web;
using System.Collections.ObjectModel;
using System.Net;
using System.Globalization;

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

			Parallel.Task.Factory
				.StartNew (() => FetchProjectsFromCache ())
				.ContinueWith ((task) => ShowProjectsAndUpdate ())
				.ContinueWith ((task) => InitCurrentEntry ())
				.ContinueWith ((task) => CheckForUpcomingEvents ());
		}

	//	protected override async void OnAppearing(){
	//	TODO establish link to gps 
	//  }

//		private async void fetch(){
//			List<Event> events = await RestAPI.GetEventsWithProjects ();
//
//			var str = events.ToString ();
//		}
//
//
//		private void post() {
//
//			Event e = new Event() { Name = "Testiii", ProjectId = _projects.First().Id.Value, StartDate = DateTime.Now.AddDays(-2), UserId = App.User.Id.Value };
//			RestAPI.SetEventsWithProject (e);
//		}

		private void FetchProjectsFromCache()
		{
			var keyValue = _cache.Get<List<Toggl.Project>>(ProjectsKey);
			_projects = keyValue != null ? new ObservableCollection<Toggl.Project> (keyValue) : 
				new ObservableCollection<Toggl.Project> ();

			list.ItemsSource = _projects;
		}

		private void InitializeEventProvider()
		{
			var eventProvider = DependencyService.Get<IEventProvider> ();
			eventProvider.Init ();
		}

		private void CheckForUpcomingEvents()
		{
			_upcomingEvent = App.UpcomingEvents.FirstOrDefault();
			if (_upcomingEvent != null) {
				Device.BeginInvokeOnMainThread (() => {
					var eventNotification = DependencyService.Get<IEventNotification> ();
					eventNotification.RegisterEvent (DateTime.Now.AddSeconds (3), PrintMsg); // _upcomingEvent.StartDate
				});
			}
		}

		private void InitCurrentEntry()
		{
			var currentEntry = App.Toggl.TimeEntry.Current();
			if (currentEntry.Id.HasValue) {
				_startedTimeEntry = currentEntry;
				_startedDateTime = currentEntry.Start.TogglDateTimeWorkAround ();
			}
			Device.BeginInvokeOnMainThread (() => {
				startBtn.IsEnabled = _startedTimeEntry == null;
				startBtn.IsVisible = _startedTimeEntry == null;
				stopBtn.IsVisible = _startedTimeEntry != null;
			});
		}

		private void ShowProjectsAndUpdate()
		{
			Parallel.Task.Factory
				.StartNew (() => Device.BeginInvokeOnMainThread (() => {
					var cnt = _projects.Count;
					var name = App.User.FullName;
					list.Header = new Label { Text = string.Format ("{0} Project{1} for {2}", cnt, (cnt == 1 ? "" : "s"), name.Replace("_", " ").Replace("r", "R")), XAlign = TextAlignment.Center };
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
			
			var project = App.Toggl.Project.Get (_upcomingEvent.ProjectId);
			var task = await DisplayAlert ("XToggl", "Do you want to start time tracking for " + _upcomingEvent.Name + " (project: " + project.Name + ") now?", "Yes", "No");
			if (task) {
				
				StartTimeMeasurementForProject (project);
			} else {
				await App.Notificator.Notify (ToastNotificationType.Info, "XToggl", "Time tracking not startet", TimeSpan.FromSeconds (1.0), null);
			}
		}

		private void StartTimeMeasurementForProject (Project project)
		{
			StopTimeMeasurementIfRunning ();

			_selectedProject = project;
			selectedProjectText.Text = _selectedProject.Name;

			Start (startBtn, EventArgs.Empty);
		}

		public async void Start(object sender, EventArgs e)
		{
			if (_startedDateTime.HasValue) {
				await App.Notificator.Notify(ToastNotificationType.Info, 
					"XToggl Message", "Please stop other tasks before starting a new one", TimeSpan.FromSeconds(2));
				return;
			}

			var btn = ((Button)sender);
			btn.IsVisible = false;

			await Parallel.Task.Factory
				.StartNew (() => AddTimeEntry (_selectedProject))
				.ContinueWith ((entry) => ChangeButtonVisibility (stopBtn));
		}

		private void StopTimeMeasurementIfRunning ()
		{
			if (_startedTimeEntry != null) {
				Stop (stopBtn, EventArgs.Empty);
			}
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
			var entry = new TimeEntry {
				IsBillable = false,
				CreatedWith = "TogglAPI.Net",
				Start = _startedDateTime.Value.ToIsoDateStr (),
				Duration = _startedDateTime.Value.ToTogglStartDuration (),
				WorkspaceId = project.WorkspaceId,
				ProjectId = project.Id
			};
			_startedTimeEntry = App.Toggl.TimeEntry.Add (entry);
		}

		private void EditTimeEntry() 
		{
			var now = DateTime.Now;
			_startedTimeEntry.Start = _startedTimeEntry.Start.TogglDateTimeWorkAroundStr();
			_startedTimeEntry.Stop = now.ToIsoDateStr ();
			_startedTimeEntry.Duration = _startedDateTime.Value.DifferenceInSeconds (now);

			App.Toggl.TimeEntry.Edit (_startedTimeEntry);
		}

		private void ChangeButtonVisibility(Button btn)
		{
			Device.BeginInvokeOnMainThread (() => {
				btn.IsVisible = !btn.IsVisible;
				btn.IsEnabled = btn.IsVisible;
			});
		}

	}
}