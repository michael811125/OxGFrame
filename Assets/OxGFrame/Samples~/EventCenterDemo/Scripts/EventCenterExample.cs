using OxGFrame.EventCenter;

public class EventCenterExample : EventCenterBase<EventCenterExample>
{
    #region Default API
    public static void Add<T>() where T : EventBase, new()
    {
        GetInstance().Register<T>();
    }

    public static void Add<T>(int eventId) where T : EventBase, new()
    {
        GetInstance().Register<T>(eventId);
    }

    public static void Add(int eventId, EventBase eventBase)
    {
        GetInstance().Register(eventId, eventBase);
    }

    public static T Find<T>() where T : EventBase
    {
        return GetInstance().GetEvent<T>();
    }

    public static T Find<T>(int eventId) where T : EventBase
    {
        return GetInstance().GetEvent<T>(eventId);
    }
    #endregion

    public EventCenterExample()
    {
        // easy
        this.Register<EventMsgTest>();

        // or

        //this.Register<EventMsgTest>(0x01);

        // or

        //this.Register(0x01, new EventMsgTest());
    }
}
