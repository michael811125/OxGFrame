public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	// OxGFrame.AssetLoader.Runtime.dll
	// UniTask.dll
	// UnityEngine.CoreModule.dll
	// mscorlib.dll
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// Cysharp.Threading.Tasks.UniTask<object>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<object>
	// System.Func<Cysharp.Threading.Tasks.UniTaskVoid>
	// }}

	public void RefMethods()
	{
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixTester.<>c__DisplayClass4_0.<<Update>b__0>d>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixTester.<>c__DisplayClass4_0.<<Update>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<HotfixTester.<>c__DisplayClass4_0.<<Update>b__0>d>(HotfixTester.<>c__DisplayClass4_0.<<Update>b__0>d&)
		// Cysharp.Threading.Tasks.UniTask<object> OxGFrame.AssetLoader.AssetLoaders.InstantiateAssetAsync<object>(string,OxGFrame.AssetLoader.Progression)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<HotfixTester.<Start>d__3>(HotfixTester.<Start>d__3&)
		// object UnityEngine.GameObject.AddComponent<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
	}
}