using Cysharp.Threading.Tasks;
using NUnit.Framework;
using OxGFrame.CenterFrame.APICenter;
using System.Collections;
using UnityEngine.TestTools;

namespace OxGFrame.CenterFrame.Editor.Tests
{
    public class AcaxTests
    {
        // Reference: https://httpbin.org/

        [Test]
        public void GetAPICallback()
        {
            Http.Acax
            (
                "https://httpbin.org/get",
                "GET",
                new string[,]
                {
                    { "Content-Type", "application/json; charset=utf-8" }
                },
                null,
                (json) =>
                {
                    UnityEngine.Debug.Log($"{nameof(Http)} [GET]\n{json}");
                }
            );

            HttpNativeWebRequest.Acax
            (
                "https://httpbin.org/get",
                "GET",
                new string[,]
                {
                    { "Content-Type", "application/json; charset=utf-8" }
                },
                null,
                (json) =>
                {
                    UnityEngine.Debug.Log($"{nameof(HttpNativeWebRequest)} [GET]\n{json}");
                }
            );

            HttpNativeClient.Acax
            (
                "https://httpbin.org/get",
                "GET",
                new string[,]
                {
                    { "Content-Type", "application/json; charset=utf-8" }
                },
                null,
                (json) =>
                {
                    UnityEngine.Debug.Log($"{nameof(HttpNativeClient)} [GET]\n{json}");
                }
            );
        }

        [Test]
        public void PostAPICallback()
        {
            Http.Acax
             (
                 "https://httpbin.org/post",
                 "POST",
                 new string[,]
                 {
                     { "Content-Type", "application/json; charset=utf-8" },
                 },
                 new object[,]
                 {
                     { "guids", "71bcaca574c91d413e2b7c252a2429d2" }
                 },
                 (json) =>
                 {
                     UnityEngine.Debug.Log($"{nameof(Http)} [POST]\n{json}");
                 }
             );

            HttpNativeWebRequest.Acax
             (
                 "https://httpbin.org/post",
                 "POST",
                 new string[,]
                 {
                     { "Content-Type", "application/json; charset=utf-8" },
                 },
                 new object[,]
                 {
                     { "guids", "71bcaca574c91d413e2b7c252a2429d2" }
                 },
                 (json) =>
                 {
                     UnityEngine.Debug.Log($"{nameof(HttpNativeWebRequest)} [POST]\n{json}");
                 }
             );

            HttpNativeClient.Acax
             (
                 "https://httpbin.org/post",
                 "POST",
                 new string[,]
                 {
                     { "Content-Type", "application/json; charset=utf-8" },
                 },
                 new object[,]
                 {
                     { "guids", "71bcaca574c91d413e2b7c252a2429d2" }
                 },
                 (json) =>
                 {
                     UnityEngine.Debug.Log($"{nameof(HttpNativeClient)} [POST]\n{json}");
                 }
             );
        }

        [UnityTest]
        public IEnumerator GetAPIAsync()
        {
            yield return Task().ToCoroutine();
            async UniTask Task()
            {
                await Http.AcaxAsync
                 (
                     "https://httpbin.org/get",
                     "GET",
                     new string[,]
                     {
                         { "Content-Type", "application/json; charset=utf-8" }
                     },
                     null,
                     (json) =>
                     {
                         UnityEngine.Debug.Log($"{nameof(Http)} [GET]\n{json}");
                     }
                 );

                await HttpNativeWebRequest.AcaxAsync
                (
                    "https://httpbin.org/get",
                    "GET",
                    new string[,]
                    {
                        { "Content-Type", "application/json; charset=utf-8" }
                    },
                    null,
                    (json) =>
                    {
                        UnityEngine.Debug.Log($"{nameof(HttpNativeWebRequest)} [GET]\n{json}");
                    }
                );

                await HttpNativeClient.AcaxAsync
                (
                    "https://httpbin.org/get",
                    "GET",
                    new string[,]
                    {
                        { "Content-Type", "application/json; charset=utf-8" }
                    },
                    null,
                    (json) =>
                    {
                        UnityEngine.Debug.Log($"{nameof(HttpNativeClient)} [GET]\n{json}");
                    }
                );
            }
        }

        [UnityTest]
        public IEnumerator PostAPIAsync()
        {
            yield return Task().ToCoroutine();
            async UniTask Task()
            {
                await Http.AcaxAsync
                (
                    "https://httpbin.org/post",
                    "POST",
                    new string[,]
                    {
                        { "Content-Type", "application/json; charset=utf-8" },
                    },
                    new object[,]
                    {
                        { "guids", "71bcaca574c91d413e2b7c252a2429d2" }
                    },
                    (json) =>
                    {
                        UnityEngine.Debug.Log($"{nameof(Http)} [POST]\n{json}");
                    }
                );

                await HttpNativeWebRequest.AcaxAsync
                 (
                     "https://httpbin.org/post",
                     "POST",
                     new string[,]
                     {
                         { "Content-Type", "application/json; charset=utf-8" },
                     },
                     new object[,]
                     {
                         { "guids", "71bcaca574c91d413e2b7c252a2429d2" }
                     },
                     (json) =>
                     {
                         UnityEngine.Debug.Log($"{nameof(HttpNativeWebRequest)} [POST]\n{json}");
                     }
                 );

                await HttpNativeClient.AcaxAsync
                 (
                     "https://httpbin.org/post",
                     "POST",
                     new string[,]
                     {
                         { "Content-Type", "application/json; charset=utf-8" },
                     },
                     new object[,]
                     {
                         { "guids", "71bcaca574c91d413e2b7c252a2429d2" }
                     },
                     (json) =>
                     {
                         UnityEngine.Debug.Log($"{nameof(HttpNativeClient)} [POST]\n{json}");
                     }
                 );
            }
        }
    }
}
