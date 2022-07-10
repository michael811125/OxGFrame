using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MediaFrame
{
    public interface IMediaBase
    {
        UniTask Init();

        void Play(int loops);

        void Stop();

        void Pause();

        bool IsPlaying();

        bool IsPaused();

        bool IsLooping();

        float Length();

        float CurrentLength();

        void SetEndEvent(Action endEvent);

        void OnRelease();
    }
}
