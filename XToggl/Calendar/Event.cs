using System;

namespace XToggl.Calendar
{
	public class Event {
		public Event (long id, string name, DateTime startDate)
		{
			Id = id;
			Name = name;
			StartDate = startDate;
		}

		public long Id { get; private set; }
		public string Name { get; private set; }
		public DateTime StartDate { get; private set; }
	}
}