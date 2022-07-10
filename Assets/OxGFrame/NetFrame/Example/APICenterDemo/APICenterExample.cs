using NetFrame.APICenter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APICenterExample : APICenterBase
{
    private static APICenterExample _instance = null;
    public static APICenterExample GetInstance()
    {
        if (_instance == null) _instance = new APICenterExample();
        return _instance;
    }

    #region declaration and definition API_xBASE
    public const int APIQueryTest = API_xBASE + 1;
    #endregion

    public APICenterExample()
    {
        #region Register Event
        this.Register(new APIQueryTest(APIQueryTest));
        #endregion
    }
}
