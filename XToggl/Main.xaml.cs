using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace XToggl
{
	public partial class Main : ContentPage
	{
		public Main ()
		{
			InitializeComponent ();

			var u = App.Toggl.User.GetCurrent();

			Content = new StackLayout {
				VerticalOptions = LayoutOptions.Center,
				Children = {
					new Label {
						XAlign = TextAlignment.Center,
						Text = u.FullName
					}
				}
			};
		}
	}
}

