using System;
using XLabs.Platform.Services.Geolocation;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XToggl
{
	public class Gps
	{	
		IGeolocator geolocator;
		CancellationTokenSource cancelSource;

		public Gps ()
		{
			Setup();
		}

		public IGeolocator Geolocator {
			get {
				return geolocator;
			}
		}

		void Setup()
		{
			if (this.geolocator != null)
				return;
			this.geolocator = DependencyService.Get<IGeolocator> ();
			if(!this.geolocator.IsListening)
				this.geolocator.StartListening (1000, 5);
			
		}

		public async Task<Position> GetPosition ()
		{
			this.cancelSource = new CancellationTokenSource();

			var PositionStatus = String.Empty;

			var task = await this.geolocator.GetPositionAsync (timeout: 10000, cancelToken: this.cancelSource.Token, includeHeading: true);
			return task;
		}

	}
}

