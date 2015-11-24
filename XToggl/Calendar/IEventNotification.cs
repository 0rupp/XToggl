using System;

namespace XToggl
{
	public interface IEventNotification
	{
		void RegisterEvent(DateTime dt, Action action);
	}
}

