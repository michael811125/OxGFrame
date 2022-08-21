using OxGFrame.EventCenter;

public class EventCenterExample : EventCenterBase<EventCenterExample>
{
    #region declaration and definition EVENT_xBASE
    public const int EEventTest = EVENT_xBASE + 1;
    #endregion

    public EventCenterExample()
    {
        #region Register Event
        this.Register(new EEventTest(EEventTest));
        #endregion
    }
}
