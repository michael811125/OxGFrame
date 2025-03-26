using NUnit.Framework;
using OxGFrame.CenterFrame.APICenter;

namespace OxGFrame.CenterFrame.Editor.Tests
{
    public class AcaxTests
    {
        // Reference: https://httpbin.org/

        [Test]
        public void GetAPI()
        {
            Http.Acax
            (
                "https://httpbin.org/get",
                "GET",
                new string[,]
                {
                    { "Content-Type", "application/json" }
                },
                null,
                (json) =>
                {
                    UnityEngine.Debug.Log($"[GET]\n{json}");
                }
            );
        }

        [Test]
        public void PostAPI()
        {
            Http.Acax
             (
                 "https://httpbin.org/post",
                 "POST",
                 new string[,]
                 {
                     { "Content-Type", "application/json" },
                 },
                 new object[,]
                 {
                     { "guids", "71bcaca574c91d413e2b7c252a2429d2" }
                 },
                 (json) =>
                 {
                     UnityEngine.Debug.Log($"[POST]\n{json}");
                 }
             );
        }
    }
}
