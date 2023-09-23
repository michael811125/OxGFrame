using OxGFrame.CenterFrame;
using OxGFrame.CenterFrame.EventCenter;

public class EventCenterExample : CenterBase<EventCenterExample, EventBase>
{
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
