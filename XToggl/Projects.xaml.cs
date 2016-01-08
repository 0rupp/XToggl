using System;
using System.Collections.Generic;

using Xamarin.Forms;
using System.Collections.ObjectModel;
using XLabs.Caching;
using Toasts.Forms.Plugin.Abstractions;
using XToggl.Calendar;
using System.Linq;
using XLabs.Ioc;

namespace XToggl
{
	public partial class Projects : ContentPage
	{
		private ObservableCollection<Toggl.Project> _projects;

		private ISimpleCache _cache;
		private string ProjectsKey = "projects";

		public Projects (Event calEvent)
		{
			InitializeComponent ();

			_cache = Resolver.Resolve<ISimpleCache>();	
			var keyValue = _cache.Get<List<Toggl.Project>>(ProjectsKey);
			_projects = keyValue != null ? new ObservableCollection<Toggl.Project> (keyValue) : 
				new ObservableCollection<Toggl.Project> ();

			var cnt = _projects.Count;
			var name = App.User.FullName;
			header.Text = string.Format ("{0} Project{1} for {2}", cnt, (cnt == 1 ? "" : "s"), name.Replace("_", " ").Replace("r", "R"));
			list.ItemsSource = _projects.Where (proj => !App.UpcomingEvents.Any(ev => proj.Id.Equals(ev.ProjectId)));
			list.ItemSelected += (sender, e) => {
				((ListView)sender).SelectedItem = null;
			};
			list.ItemTapped += (sender, e) => {
				var project = e.Item as Toggl.Project;
				calEvent.ProjectId = project.Id.Value;
				var added = App.AddUpcomingEvent(calEvent);
				App.Notificator.Notify (ToastNotificationType.Info, "XToggl", "Event notification " + (added ? "activated" : "already active") , TimeSpan.FromSeconds (1.0), null);
				Navigation.PopAsync();
			};
		}
	}
}

