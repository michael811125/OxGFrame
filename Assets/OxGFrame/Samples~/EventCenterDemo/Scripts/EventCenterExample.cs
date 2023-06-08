using OxGFrame.EventCenter;

public class EventCenterExample : EventCenter<EventCenterExample>
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
