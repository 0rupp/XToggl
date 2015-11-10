﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Parallel = System.Threading.Tasks;
using Xamarin.Forms;

using Toggl;
using Toggl.Extensions;

namespace XToggl
{
	public partial class Main : ContentPage
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

		private void AddTimeEntry(Project project) 
		{
			_startedTimeEntry = App.Toggl.TimeEntry.Add (
				new TimeEntry {
					IsBillable = false,
					CreatedWith = "TogglAPI.Net",
					Start = _startedDateTime.Value.ToIsoDateStr (),
					Duration = -1,
					WorkspaceId = project.WorkspaceId,
					ProjectId = project.Id
				});
		}

		private void EditTimeEntry() 
		{
			var now = DateTime.Now;
			_startedTimeEntry.Stop = now.ToIsoDateStr ();
			_startedTimeEntry.Duration = now.Subtract (_startedDateTime.Value).Seconds;

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

