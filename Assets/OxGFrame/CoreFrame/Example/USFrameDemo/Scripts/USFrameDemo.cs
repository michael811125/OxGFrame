using OxGFrame.AssetLoader;
using OxGFrame.CoreFrame;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class USFrameDemo : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
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
                // if use prefix "build#" will load from build else will from bundle
                await CoreFrames.USFrame.LoadSceneAsync("build#LevelDemo01", LoadSceneMode.Single, (float progress, float reqSize, float totalSize) =>
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
                // if use prefix "build#" will load from build else will from bundle
                await CoreFrames.USFrame.LoadSceneAsync("build#LevelDemo02", LoadSceneMode.Single, (float progress, float reqSize, float totalSize) =>
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
                // if use prefix "build#" will load from build else will from bundle
                await CoreFrames.USFrame.LoadSceneAsync("build#LevelDemo03", LoadSceneMode.Additive, (float progress, float reqSize, float totalSize) =>
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
                await CoreFrames.USFrame.LoadSceneAsync("LevelDemo01", LoadSceneMode.Single, true, 100, (float progress, float reqSize, float totalSize) =>
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
                await CoreFrames.USFrame.LoadSceneAsync("LevelDemo02", LoadSceneMode.Single, true, 100, (float progress, float reqSize, float totalSize) =>
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
                await CoreFrames.USFrame.LoadSceneAsync("LevelDemo03", LoadSceneMode.Additive, true, 100, (float progress, float reqSize, float totalSize) =>
                {
                    Debug.Log($"Progress: {progress}, ReqSize: {reqSize}, TotalSize: {totalSize}");
                });
            };
            asyncHandler.Invoke();
        }
        #endregion

        if (Keyboard.current.numpad7Key.wasReleasedThisFrame)
        {
            CoreFrames.USFrame.Unload(true, "LevelDemo01");
        }

        if (Keyboard.current.numpad8Key.wasReleasedThisFrame)
        {
            CoreFrames.USFrame.Unload(true, "LevelDemo02");
        }

        if (Keyboard.current.numpad9Key.wasReleasedThisFrame)
        {
            CoreFrames.USFrame.Unload(false, "build#LevelDemo03");
        }

        if (Keyboard.current.numpad0Key.wasReleasedThisFrame)
        {
            CoreFrames.USFrame.Unload(false, "LevelDemo03");
        }
    }
}
