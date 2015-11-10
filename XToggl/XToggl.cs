using System;

using Xamarin.Forms;

namespace XToggl
{
	public class App : Application
	{
		public App ()
		{
			var apiKey="211b283336f7055a287578fd1eed09bd";
			var t = new Toggl.Toggl(apiKey);
			var c = t.User.GetCurrent();

			MainPage = new ContentPage {
				Content = new StackLayout {
					VerticalOptions = LayoutOptions.Center,
					Children = {
						new Label {
							XAlign = TextAlignment.Center,
							Text = c.FullName
						}
					}
				}
			};
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

