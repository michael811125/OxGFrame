using OxGFrame.APICenter;

public class APICenterExample : APICenterBase<APICenterExample>
{
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
        APICenterExample.GetInstance().GetAPI<APIQueryTest>()?.Req
        (
             new string[] { "id1", "id2" },
             (data) =>
             {

             }
        );

        // Get API and Request Async
        var api = APICenterExample.GetInstance().GetAPI<APIQueryTest>();
        if (api != null)
        {
            await APICenterExample.GetInstance().GetAPI<APIQueryTest>().ReqAsync
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
