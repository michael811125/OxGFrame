using CoreFrame.EventCenter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventCenterExample : EventCenterBase
{
    private static EventCenterExample _instance = null;
    public static EventCenterExample GetInstance()
    {
        if (_instance == null) _instance = new EventCenterExample();
        return _instance;
    }

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
