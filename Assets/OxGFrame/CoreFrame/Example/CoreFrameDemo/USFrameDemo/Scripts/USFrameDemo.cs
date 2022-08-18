using OxGFrame.AssetLoader.Bundle;
using OxGFrame.CoreFrame.USFrame;
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

    private async void Start()
    {
        BundleDistributor.GetInstance().Check(async () =>
        {
            await USManager.GetInstance().PreloadSceneBundle("scenes");
        });
    }

    private void Update()
    {
        if (Keyboard.current.numpad1Key.wasReleasedThisFrame)
        {
            Action asyncHandler = async () =>
            {
                await USManager.GetInstance().LoadFromBuild("LevelDemo01", LoadSceneMode.Single, (float progress, float reqSize, float totalSize) =>
                {
                    Debug.Log($"Progress: {progress}, ReqSize: {reqSize}, TotalSize: {totalSize}");
                });
            };
            asyncHandler?.Invoke();
        }

        if (Keyboard.current.numpad2Key.wasReleasedThisFrame)
        {
            Action asyncHandler = async () =>
            {
                await USManager.GetInstance().LoadFromBuild("LevelDemo02", LoadSceneMode.Single, (float progress, float reqSize, float totalSize) =>
                {
                    Debug.Log($"Progress: {progress}, ReqSize: {reqSize}, TotalSize: {totalSize}");
                });
            };
            asyncHandler?.Invoke();
        }

        if (Keyboard.current.numpad3Key.wasReleasedThisFrame)
        {
            Action asyncHandler = async () =>
            {
                await USManager.GetInstance().LoadFromBuild("LevelDemo03", LoadSceneMode.Additive, (float progress, float reqSize, float totalSize) =>
                {
                    Debug.Log($"Progress: {progress}, ReqSize: {reqSize}, TotalSize: {totalSize}");
                });
            };
            asyncHandler?.Invoke();
        }

        if (Keyboard.current.numpad4Key.wasReleasedThisFrame)
        {
            Action asyncHandler = async () =>
            {
                await USManager.GetInstance().LoadFromBundle("scenes", "LevelDemo01", LoadSceneMode.Single, (float progress, float reqSize, float totalSize) =>
                {
                    Debug.Log($"Progress: {progress}, ReqSize: {reqSize}, TotalSize: {totalSize}");
                });
            };
            asyncHandler?.Invoke();
        }

        if (Keyboard.current.numpad5Key.wasReleasedThisFrame)
        {
            Action asyncHandler = async () =>
            {
                await USManager.GetInstance().LoadFromBundle("scenes", "LevelDemo02", LoadSceneMode.Single, (float progress, float reqSize, float totalSize) =>
                {
                    Debug.Log($"Progress: {progress}, ReqSize: {reqSize}, TotalSize: {totalSize}");
                });
            };
            asyncHandler?.Invoke();
        }

        if (Keyboard.current.numpad6Key.wasReleasedThisFrame)
        {
            Action asyncHandler = async () =>
            {
                await USManager.GetInstance().LoadFromBundle("scenes", "LevelDemo03", LoadSceneMode.Additive, (float progress, float reqSize, float totalSize) =>
                {
                    Debug.Log($"Progress: {progress}, ReqSize: {reqSize}, TotalSize: {totalSize}");
                });
            };
            asyncHandler?.Invoke();
        }

        if (Keyboard.current.numpad7Key.wasReleasedThisFrame)
        {
            USManager.GetInstance().Unload("LevelDemo01");
        }

        if (Keyboard.current.numpad8Key.wasReleasedThisFrame)
        {
            USManager.GetInstance().Unload("LevelDemo02");
        }

        if (Keyboard.current.numpad9Key.wasReleasedThisFrame)
        {
            USManager.GetInstance().UnloadAll("LevelDemo03");
        }
    }
}
