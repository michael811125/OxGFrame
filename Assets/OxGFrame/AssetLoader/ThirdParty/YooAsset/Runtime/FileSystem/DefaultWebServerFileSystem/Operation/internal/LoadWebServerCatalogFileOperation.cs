using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    internal sealed class LoadWebServerCatalogFileOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            RequestData,
            LoadCatalog,
            Done,
        }

        private readonly DefaultWebServerFileSystem _fileSystem;
        private UnityWebTextRequestOperation _webTextRequestOp;
        private ESteps _steps = ESteps.None;
        private string _textData = null;

        internal LoadWebServerCatalogFileOperation(DefaultWebServerFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.RequestData;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestData)
            {
                if (_webTextRequestOp == null)
                {
                    string filePath = _fileSystem.GetCatalogFileLoadPath();
                    string url = DownloadSystemHelper.ConvertToWWWPath(filePath);
                    _webTextRequestOp = new UnityWebTextRequestOperation(url);
                    _webTextRequestOp.StartOperation();
                    AddChildOperation(_webTextRequestOp);
                }

                _webTextRequestOp.UpdateOperation();
                if (_webTextRequestOp.IsDone == false)
                    return;

                if (_webTextRequestOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.LoadCatalog;
                    _textData = _webTextRequestOp.Result;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _webTextRequestOp.Error;
                }
            }

            if (_steps == ESteps.LoadCatalog)
            {
                if (string.IsNullOrEmpty(_textData))
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Buildin catalog file content is empty !";
                    return;
                }

                try
                {
                    var catalog = JsonUtility.FromJson<DefaultBuildinFileCatalog>(_textData);
                    if (catalog.PackageName != _fileSystem.PackageName)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"Catalog file package name {catalog.PackageName} cannot match the file system package name {_fileSystem.PackageName}";
                        return;
                    }

                    foreach (var wrapper in catalog.Wrappers)
                    {
                        var fileWrapper = new DefaultWebServerFileSystem.FileWrapper(wrapper.FileName);
                        _fileSystem.RecordCatalogFile(wrapper.BundleGUID, fileWrapper);
                    }

                    YooLogger.Log($"Package '{_fileSystem.PackageName}' buildin catalog files count : {catalog.Wrappers.Count}");
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                catch (Exception e)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Failed to load catalog file : {e.Message}";
                }
            }
        }
    }
}