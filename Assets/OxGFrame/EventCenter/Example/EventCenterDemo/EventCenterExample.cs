using OxGFrame.EventCenter;

public class EventCenterExample : EventCenterBase<EventCenterExample>
{
    #region declaration and definition xBASE
    public const int EEventTest = xBASE + 1;
    #endregion

    public EventCenterExample()
    {
        #region Register Event
        this.Register(new EEventTest(EEventTest));
        #endregion
    }
}
