using System;
using System.Collections.Generic;

namespace XToggl.Calendar
{
	public interface IEventProvider {
		IList<Event> GetEventsFromNow();
	}
}