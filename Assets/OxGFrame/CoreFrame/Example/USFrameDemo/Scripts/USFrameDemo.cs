using OxGFrame.AssetLoader;
using OxGFrame.CoreFrame;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public static class UnityScene
{
    public static class Build
    {
        // If use prefix "build#" will load from build else will from bundle
        private const string _prefix = "buil#";

        // Unity Scenes
        public static readonly string LevelDemo01 = $"{_prefix}LevelDemo01";
        public static readonly string LevelDemo02 = $"{_prefix}LevelDemo02";
        public static readonly string LevelDemo03 = $"{_prefix}LevelDemo03";
    }

    public static class Bundle
    {
        // If use prefix "build#" will load from build else will from bundle
        private const string _prefix = "";

        // Unity Scenes
        public static readonly string LevelDemo01 = $"{_prefix}LevelDemo01";
        public static readonly string LevelDemo02 = $"{_prefix}LevelDemo02";
        public static readonly string LevelDemo03 = $"{_prefix}LevelDemo03";
    }
}

public class USFrameDemo : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);

        // If Init instance can more efficiency
        CoreFrames.USFrame.InitInstance();
    }

    private void Update()
    {
        // Make sure play mode is initialized
        if (!AssetPatcher.IsInitialized()) return;

        #region From Build
        if (Keyboard.current.numpad1Key.wasReleasedThisFrame)
        {
            Action asyncHandler = async () =>
            {
                await CoreFrames.USFrame.LoadSceneAsync(UnityScene.Build.LevelDemo01, LoadSceneMode.Single, true, 100, (float progress, float reqSize, float totalSize) =>
                {
                    Debug.Log($"Progress: {progress}, ReqSize: {reqSize}, TotalSize: {totalSize}");
                });
            };
            asyncHandler.Invoke();
        }

        if (Keyboard.current.numpad2Key.wasReleasedThisFrame)
        {
            Action asyncHandler = async () =>
            {
                await CoreFrames.USFrame.LoadSceneAsync(UnityScene.Build.LevelDemo02, LoadSceneMode.Single, true, 100, (float progress, float reqSize, float totalSize) =>
                {
                    Debug.Log($"Progress: {progress}, ReqSize: {reqSize}, TotalSize: {totalSize}");
                });
            };
            asyncHandler.Invoke();
        }

        if (Keyboard.current.numpad3Key.wasReleasedThisFrame)
        {
            Action asyncHandler = async () =>
            {
                await CoreFrames.USFrame.LoadSceneAsync(UnityScene.Build.LevelDemo03, LoadSceneMode.Additive, true, 100, (float progress, float reqSize, float totalSize) =>
                {
                    Debug.Log($"Progress: {progress}, ReqSize: {reqSize}, TotalSize: {totalSize}");
                });
            };
            asyncHandler.Invoke();
        }
        #endregion

        #region From Bundle
        if (Keyboard.current.numpad4Key.wasReleasedThisFrame)
        {
            Action asyncHandler = async () =>
            {
                await CoreFrames.USFrame.LoadSceneAsync(UnityScene.Bundle.LevelDemo01, LoadSceneMode.Single, true, 100, (float progress, float reqSize, float totalSize) =>
                {
                    Debug.Log($"Progress: {progress}, ReqSize: {reqSize}, TotalSize: {totalSize}");
                });
            };
            asyncHandler.Invoke();
        }

        if (Keyboard.current.numpad5Key.wasReleasedThisFrame)
        {
            Action asyncHandler = async () =>
            {
                await CoreFrames.USFrame.LoadSceneAsync(UnityScene.Bundle.LevelDemo02, LoadSceneMode.Single, true, 100, (float progress, float reqSize, float totalSize) =>
                {
                    Debug.Log($"Progress: {progress}, ReqSize: {reqSize}, TotalSize: {totalSize}");
                });
            };
            asyncHandler.Invoke();
        }

        if (Keyboard.current.numpad6Key.wasReleasedThisFrame)
        {
            Action asyncHandler = async () =>
            {
                await CoreFrames.USFrame.LoadSceneAsync(UnityScene.Bundle.LevelDemo03, LoadSceneMode.Additive, true, 100, (float progress, float reqSize, float totalSize) =>
                {
                    Debug.Log($"Progress: {progress}, ReqSize: {reqSize}, TotalSize: {totalSize}");
                });
            };
            asyncHandler.Invoke();
        }
        #endregion

        if (Keyboard.current.numpad7Key.wasReleasedThisFrame)
        {
            CoreFrames.USFrame.Unload(true, UnityScene.Bundle.LevelDemo01);
        }

        if (Keyboard.current.numpad8Key.wasReleasedThisFrame)
        {
            CoreFrames.USFrame.Unload(true, UnityScene.Bundle.LevelDemo02);
        }

        if (Keyboard.current.numpad9Key.wasReleasedThisFrame)
        {
            CoreFrames.USFrame.Unload(false, UnityScene.Build.LevelDemo03);
        }

        if (Keyboard.current.numpad0Key.wasReleasedThisFrame)
        {
            CoreFrames.USFrame.Unload(false, UnityScene.Bundle.LevelDemo03);
        }
    }
}
