using NetFrame.APICenter;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APIQueryTest : APIBase
{
    public delegate void Rh(object obj);

    public APIQueryTest(int funcId) : base(funcId) { }

    public void Req(string[] guids, Rh rh)
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

}