using OxGFrame.AgencyCenter;
using OxGFrame.AgencyCenter.APICenter;

public class APICenterExample : CenterBase<APICenterExample, APIBase>
{
    #region Default API
    public static void Add<T>() where T : APIBase, new()
    {
        GetInstance().Register<T>();
    }

    public static void Add<T>(int apiId) where T : APIBase, new()
    {
        GetInstance().Register<T>(apiId);
    }

    public static void Add(int apiId, APIBase apiBase)
    {
        GetInstance().Register(apiId, apiBase);
    }

    public static T Find<T>() where T : APIBase
    {
        return GetInstance().Get<T>();
    }

    public static T Find<T>(int apiId) where T : APIBase
    {
        return GetInstance().Get<T>(apiId);
    }
    #endregion

    public APICenterExample()
    {
        // easy
        this.Register<APIQueryTest>();

        // or 

        //this.Register<APIQueryTest>(0x01);

        // or

        //this.Register(0x01, new APIQueryTest());
    }

    /*
    public async void UseExample()
    {
        // Get API and Request Callback
        APICenterExample.Find<APIQueryTest>()?.Req
        (
             new string[] { "id1", "id2" },
             (data) =>
             {

             }
        );

        // Get API and Request Async
        var api = APICenterExample.Find<APIQueryTest>();
        if (api != null)
        {
            await APICenterExample.Find<APIQueryTest>().ReqAsync
            (
                 new string[] { "id1", "id2" },
                 (data) =>
                 {

                 }
            );
        }
    }
    */
}
