using System;

namespace XToggl.Calendar
{
	public class Event {
		public Event () {
		}

		public Event (long id, string name, DateTime startDate)
		{
			Id = id;
			Name = name;
			StartDate = startDate;
		}
		public long Id { get; set; }

		public string Name { get; set; }
		public DateTime StartDate { get; set; }
		public int ProjectId { get; set; }
		public int UserId { get; set; }
	}
}