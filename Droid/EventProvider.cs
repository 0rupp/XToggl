using System;
using Android.Provider;
using Android.Content;
using System.Collections.Generic;

using XToggl.Calendar;

namespace XToggl.Droid
{
	public class EventProvider : IEventProvider
	{
		private long calendarId;
		public static ContentResolver ContentResolver;

		public EventProvider ()
		{
			var calendarsUri = CalendarContract.Calendars.ContentUri;
			string[] calendarsProjection = {
				CalendarContract.Calendars.InterfaceConsts.Id,
				CalendarContract.Calendars.InterfaceConsts.CalendarDisplayName,
				CalendarContract.Calendars.InterfaceConsts.AccountName
			};
			var cursor = ContentResolver.Query(calendarsUri, calendarsProjection, null, null, null);

			string[] sourceColumns = {
				CalendarContract.Calendars.InterfaceConsts.CalendarDisplayName,
				CalendarContract.Calendars.InterfaceConsts.AccountName };

			if (cursor.MoveToFirst ())
				do {
					calendarId = cursor.GetLong(0);
				} while (cursor.MoveToNext ());
			cursor.Close ();
		}

		#region IEventProvider implementation

		IList<Event> IEventProvider.GetEventsFromNow ()
		{
			Android.Database.ICursor cursor = GetCursor ();

			var sourceColumns = new string[] {
				CalendarContract.Events.InterfaceConsts.Id,
				CalendarContract.Events.InterfaceConsts.Title,
				CalendarContract.Events.InterfaceConsts.Dtstart };
			
			IList<Event> data = new List<Event> (cursor.Count);
			if (cursor.MoveToFirst ())
				do {
					var ms = cursor.GetLong (2);
					var startDate = ms.ToTogglDateTime();
					var ev = new Event(cursor.GetLong(0), cursor.GetString (1), startDate);
					
					data.Add (ev);
				} while (cursor.MoveToNext ());
			cursor.Close ();

			return data;
		}

		Event IEventProvider.GetNextEvent ()
		{
			Android.Database.ICursor cursor = GetCursor ();
			var cnt = cursor.Count;

			Event ev = null;
			IList<Event> data = new List<Event> (cnt);
			if (cursor.MoveToFirst ()) {
				var ms = cursor.GetLong (2);
				var startDate = ms.ToTogglDateTime ();
				ev = new Event (cursor.GetLong (0), cursor.GetString (1), startDate);
			}
			cursor.Close ();

			return ev;
		}

		private Android.Database.ICursor GetCursor ()
		{
			var eventsUri = CalendarContract.Events.ContentUri;
			string[] eventsProjection =  {
				CalendarContract.Events.InterfaceConsts.Id,
				CalendarContract.Events.InterfaceConsts.Title,
				CalendarContract.Events.InterfaceConsts.Dtstart
			};
			var ms = (long)new TimeSpan (DateTime.Now.Subtract (new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks).TotalMilliseconds;
			var cursor = ContentResolver.Query (eventsUri, eventsProjection, "calendar_id = ? AND dtstart >= ?", new string[] {
				calendarId.ToString (),
				ms.ToString ()
			}, "dtstart ASC");

			return cursor;
		}
		#endregion
	}
}

