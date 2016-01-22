using System;
using System.Collections.Generic;

using Xamarin.Forms;
using XLabs.Platform.Services.Geolocation;

using System.Threading;
using XLabs.Caching;
using XLabs.Ioc;

namespace XToggl
{
	public partial class GpsPage : ContentPage
	{
		private string GpsPositionsKey = "gpsPositions";
		private Gps gps;
		Position lastKnownPosition;

		public GpsPage ()
		{
			InitializeComponent ();
			gps = new Gps ();
			gps.Geolocator.PositionError += OnListeningError;
			gps.Geolocator.PositionChanged += OnPositionChanged;
		}

		public async void GetPosition (object sender, EventArgs e) 
		{
			var position = await gps.GetPosition ();
			ShowPosition (position);
		}

		public async void ChooseProject (object sender, EventArgs e) 
		{
			var positions = await App.GetGpsPositions ();
			var gpsPos = new GpsPosition (lastKnownPosition.Latitude, lastKnownPosition.Longitude, 0, App.User.Id.Value);
			await Navigation.PushAsync (new Projects (gpsPos, positions));
		}

		public void DeleteCache (object sender, EventArgs e) 
		{
			ISimpleCache _cache = Resolver.Resolve<ISimpleCache>();	
			_cache.Remove(GpsPositionsKey);
		}

		private void ShowPosition (Position pos)
		{
			Device.BeginInvokeOnMainThread (() =>  {
				var str = pos.Timestamp.ToString ("G");
				str += " La: " + pos.Latitude.ToString ("N4");
				str += " Lo: " + pos.Longitude.ToString ("N4");
				position.Text = str;
				btnChooseProject.IsEnabled = pos != null;
			});
		}

		/// <summary>
		/// Handles the <see cref="E:ListeningError" /> event.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="PositionErrorEventArgs"/> instance containing the event data.</param>
		private void OnListeningError(object sender, PositionErrorEventArgs e)
		{
			Device.BeginInvokeOnMainThread (() => {
				position.Text = e.Error.ToString ();
			});
		}

		/// <summary>
		/// Handles the <see cref="E:PositionChanged" /> event.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="PositionEventArgs"/> instance containing the event data.</param>
		private void OnPositionChanged(object sender, PositionEventArgs e)
		{
			lastKnownPosition = e.Position;
			ShowPosition (e.Position);
		}

	}
}

