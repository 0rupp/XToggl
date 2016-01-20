using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Toasts.Forms.Plugin.Abstractions;
using Xamarin.Forms;
using XLabs.Caching;
using XLabs.Ioc;
using XToggl.Calendar;

namespace XToggl
{
	public partial class Projects : ContentPage
	{
		private ISimpleCache _cache;
		private string ProjectsKey = "projects";

		public Projects (Event calEvent)
		{
			InitializeComponent ();

			ObservableCollection<Toggl.Project> allProjects = CommonInit ();
			var projects = allProjects.Where (proj => !App.UpcomingEvents.Any(ev => proj.Id.Equals(ev.ProjectId)));
			list.ItemsSource = projects;
			var cnt = projects.Count();
			var name = App.User.FullName;
			list.Header = new Label { Text = string.Format ("{0} Project{1} for {2}", cnt, (cnt == 1 ? "" : "s"), name.Replace("_", " ").Replace("r", "R")), XAlign = TextAlignment.Center };

			list.ItemTapped += (sender, e) => {
				var project = e.Item as Toggl.Project;
				calEvent.ProjectId = project.Id.Value;
				var added = App.AddUpcomingEvent(calEvent);
				App.Notificator.Notify (ToastNotificationType.Info, "XToggl", "Event notification " + (added ? "activated" : "already active") , TimeSpan.FromSeconds (1.0), null);
				Navigation.PopAsync();
			};
		}

		public Projects (GpsPosition gpsPos, IList<GpsPosition> gpsPositions)
		{
			InitializeComponent ();

			ObservableCollection<Toggl.Project> allProjects = CommonInit ();
			var projects = allProjects.Where (proj => !gpsPositions.Any(pos => proj.Id.Equals(pos.ProjectId)));
			list.ItemsSource = projects;
			var cnt = projects.Count();
			var name = App.User.FullName;
			list.Header = new Label { Text = string.Format ("{0} Project{1} for {2}", cnt, (cnt == 1 ? "" : "s"), name.Replace("_", " ").Replace("r", "R")), XAlign = TextAlignment.Center };

			list.ItemTapped += (sender, e) => {
				var project = e.Item as Toggl.Project;
				gpsPos.ProjectId = project.Id.Value;
				var added = App.AddGpsPosition(gpsPos);
				App.Notificator.Notify (ToastNotificationType.Info, "XToggl", "Position notification " + (added ? "activated" : "already active") , TimeSpan.FromSeconds (1.0), null);
				Navigation.PopAsync();
			};
		}

		private ObservableCollection<Toggl.Project> CommonInit(){
			list.ItemSelected += (sender, e) => {
				((ListView)sender).SelectedItem = null;
			};

			_cache = Resolver.Resolve<ISimpleCache>();	
			var keyValue = _cache.Get<List<Toggl.Project>>(ProjectsKey);
			return keyValue != null ? new ObservableCollection<Toggl.Project> (keyValue) : 
				new ObservableCollection<Toggl.Project> ();

		}
	}
}

