using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"OxGFrame.AssetLoader.Runtime.dll",
		"UniTask.dll",
		"UnityEngine.CoreModule.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// Cysharp.Threading.Tasks.UniTask.Awaiter<object>
	// Cysharp.Threading.Tasks.UniTask<object>
	// System.Func<Cysharp.Threading.Tasks.UniTaskVoid>
	// }}

	public void RefMethods()
	{
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixTester.<>c__DisplayClass4_0.<<Update>b__0>d>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixTester.<>c__DisplayClass4_0.<<Update>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<HotfixTester.<>c__DisplayClass4_0.<<Update>b__0>d>(HotfixTester.<>c__DisplayClass4_0.<<Update>b__0>d&)
		// Cysharp.Threading.Tasks.UniTask<object> OxGFrame.AssetLoader.AssetLoaders.InstantiateAssetAsync<object>(string,OxGFrame.AssetLoader.Progression)
		// object UnityEngine.GameObject.AddComponent<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
	}
}