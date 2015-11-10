using System;

using Xamarin.Forms;

namespace XToggl
{
	public class App : Application
	{
		const string apiKey = "211b283336f7055a287578fd1eed09bd";
		private static Toggl.Toggl _t = new Toggl.Toggl(apiKey);

		public static Toggl.Toggl Toggl { get { return _t; } }

		public App ()
		{	
			MainPage = new Main ();
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}

