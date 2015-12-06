using System;
using System.Collections.Generic;

namespace XToggl.Calendar
{
	public interface IEventProvider {
		void Init();
		IList<Event> GetEventsFromNow();
		Event GetNextEvent();
	}
}