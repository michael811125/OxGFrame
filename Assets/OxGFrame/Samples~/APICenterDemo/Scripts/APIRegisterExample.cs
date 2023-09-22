using Newtonsoft.Json.Linq;
using Cysharp.Threading.Tasks;
using OxGFrame.AgencyCenter.APICenter;

public class APIQueryTest : APIBase
{
    // Custom your response handler
    public delegate void ResponseHandler(object obj);

    public void ReqPost(string[] guids, ResponseHandler rh)
    {
        Http.Acax(
             "url",
             "POST",
             new string[,] {
                 { "Content-Type", "application/json"},
                 { "Ticket", "none"},
                 { "Account", "none"}
             },
             new object[,] {
                 { "guids", guids},
             },
             (json) =>
             {
                 JObject response = JObject.Parse(json);
                 // response: 
                 // {   
                 //     "status": true,
                 //     "message": "success",
                 //     "data": []
                 // }   
                 rh?.Invoke(response["data"]);
             }
        );
    }

    public async UniTask ReqPostAsync(string[] guids, ResponseHandler rh)
    {
        await Http.AcaxAsync(
             "url",
             "POST",
             new string[,] {
                 { "Content-Type", "application/json"},
                 { "Ticket", "none"},
                 { "Account", "none"}
             },
             new object[,] {
                 { "guids", guids},
             },
             (json) =>
             {
                 JObject response = JObject.Parse(json);
                 // response: 
                 // {   
                 //     "status": true,
                 //     "message": "success",
                 //     "data": []
                 // }   
                 rh?.Invoke(response["data"]);
             }
        );
    }
}