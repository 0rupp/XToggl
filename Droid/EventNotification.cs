using System;
using Android.OS;

namespace XToggl.Droid
{
	public class EventNotification : IEventNotification
	{
		public EventNotification ()
		{
		}

		#region IEventNotification implementation

		void IEventNotification.RegisterEvent (DateTime eventDate, Action action)
		{
//          oder Implementierung  über alarmManager (dann auch wenn App offline)
//			http://stackoverflow.com/questions/24196890/android-schedule-task-to-execute-at-specific-time-daily
			Handler h = new Handler ();
			var ms = SystemClock.UptimeMillis ();
			var msSinceUpTime = eventDate.MsSinceUpTime (ms);
			h.PostAtTime (action, msSinceUpTime);
		}

		#endregion
	}
}

