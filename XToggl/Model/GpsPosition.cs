using System;
using System.Collections.Generic;

namespace XToggl
{
	public class GpsPositionsResult
	{
		public List<GpsPosition> GpsPositions { get; set; }
	}

	public class GpsPosition
	{
		public GpsPosition ()
		{			
		}

		public GpsPosition (double latitude, double longitude, int projectId, int userId)
		{
			UserId = userId;
			ProjectId = projectId;
			Longitude = longitude;
			Latitude = latitude;
		}

		public int Id { get; set; }

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public int ProjectId { get; set; }

		public int UserId { get; set; }
	}
}