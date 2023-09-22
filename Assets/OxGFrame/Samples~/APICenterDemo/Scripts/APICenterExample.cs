using OxGFrame.AgencyCenter;
using OxGFrame.AgencyCenter.APICenter;

public class APICenterExample : CenterBase<APICenterExample, APIBase>
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
        // Find API and Request Callback
        APICenterExample.Find<APIQueryTest>()?.ReqPost
        (
             new string[] { "id1", "id2" },
             (data) =>
             {

             }
        );

        // Find API and Request Async
        var api = APICenterExample.Find<APIQueryTest>();
        if (api != null)
        {
            await APICenterExample.Find<APIQueryTest>().ReqPostAsync
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
