using System;
using System.Collections.Generic;

using Xamarin.Forms;
using XLabs.Platform.Services.Geolocation;
using System.Threading.Tasks;
using System.Threading;
using XLabs.Caching;
using XLabs.Ioc;

namespace XToggl
{
	public partial class GpsPage : ContentPage
	{
		IGeolocator geolocator;
		Position _lastKnownPosition;
		CancellationTokenSource cancelSource;
		private readonly TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();
		private string GpsPositionsKey = "gpsPositions";

		public GpsPage ()
		{
			InitializeComponent ();
			Setup ();
		}

		public async void GetPosition (object sender, EventArgs e) 
		{
			await GetPosition ();
		}

		public async void ChooseProject (object sender, EventArgs e) 
		{
			var positions = await App.GetGpsPositions ();
			var gpsPos = new GpsPosition (_lastKnownPosition.Latitude, _lastKnownPosition.Longitude, 0, App.User.Id.Value);
			await Navigation.PushAsync (new Projects (gpsPos, positions));
		}

		public async void DeleteCache (object sender, EventArgs e) 
		{
			ISimpleCache _cache = Resolver.Resolve<ISimpleCache>();	
			_cache.Remove(GpsPositionsKey);
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
			_lastKnownPosition = pos;
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
			ShowPosition (e.Position);
		}

	}
}

