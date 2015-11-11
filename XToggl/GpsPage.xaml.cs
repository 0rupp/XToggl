using System;
using System.Collections.Generic;

using Xamarin.Forms;
using XLabs.Platform.Services.Geolocation;
using System.Threading.Tasks;
using System.Threading;

namespace XToggl
{
	public partial class GpsPage : ContentPage
	{
		IGeolocator geolocator;
		CancellationTokenSource cancelSource;
		private readonly TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();

		public GpsPage ()
		{
			InitializeComponent ();
			Setup ();
		}

		public void GetPosition (object sender, EventArgs e) 
		{
			GetPosition ();
		}

		void Setup()
		{
			if (this.geolocator != null)
				return;
			this.geolocator = DependencyService.Get<IGeolocator> ();
			if(!this.geolocator.IsListening)
				this.geolocator.StartListening (1000, 5);
			this.geolocator.PositionError += OnListeningError;
			this.geolocator.PositionChanged += OnPositionChanged;
		}

		async Task GetPosition ()
		{
			Setup();

			this.cancelSource = new CancellationTokenSource();

			var PositionStatus = String.Empty;
			var PositionLatitude = String.Empty;
			var PositionLongitude = String.Empty;
			IsBusy = true;
			await this.geolocator.GetPositionAsync (timeout: 10000, cancelToken: this.cancelSource.Token, includeHeading: true)
				.ContinueWith (t =>
					{
						IsBusy = false;
						if (t.IsFaulted)
							PositionStatus = ((GeolocationException)t.Exception.InnerException).Error.ToString();
						else if (t.IsCanceled)
							PositionStatus = "Canceled";
						else
						{
							ShowPosition(t.Result);
						}

					}, _scheduler);
		}

		private void ShowPosition (Position pos)
		{
			Device.BeginInvokeOnMainThread (() =>  {
				var str = pos.Timestamp.ToString ("G");
				str += " La: " + pos.Latitude.ToString ("N4");
				str += " Lo: " + pos.Longitude.ToString ("N4");
				position.Text = str;
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
			ShowPosition (e.Position);
		}

	}
}

