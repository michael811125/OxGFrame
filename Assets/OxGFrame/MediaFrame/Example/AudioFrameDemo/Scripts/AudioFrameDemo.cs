using AssetLoader.Bundle;
using Cysharp.Threading.Tasks;
using MediaFrame.AudioFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioFrameDemo : MonoBehaviour
{
    public const string BGM_PATH = "Audio/BGM/";
    public const string GERNERAL_SOUNDS_PATH = "Audio/General/";
    public const string FIGHT_SOUNDS_PATH = "Audio/Fight/";
    public const string VOICES_PATH = "Audio/Voice/";
    public const string ATMOSPHERE_PATH = "Audio/Atmosphere/";

    private void Start()
    {
        //BundleDistributor.GetInstance().Check();
    }

    #region Audio 【BGM】
    public async void PlayBGM()
    {
        await AudioManager.GetInstance().Play(BGM_PATH + "bgm_Example");

        // Bundle
        //await AudioManager.GetInstance().Play("mediaframe/audio/bgm/bgm_Example", "bgm_Example");
    }

    public void StopBGM()
    {
        AudioManager.GetInstance().Stop(BGM_PATH + "bgm_Example");

        // Bundle
        //AudioManager.GetInstance().Stop("bgm_Example");
    }

    public void StopBGMWithDestroy()
    {
        AudioManager.GetInstance().Stop(BGM_PATH + "bgm_Example", false, true);

        // Bundle
        //AudioManager.GetInstance().Stop("bgm_Example", true);
    }

    public void PauseBGM()
    {
        AudioManager.GetInstance().Pause(BGM_PATH + "bgm_Example");

        // Bundle
        //AudioManager.GetInstance().Pause("bgm_Example");
    }
    #endregion

    #region Audio 【GeneralFX】 
    public async void PlayGeneralFX()
    {
        await AudioManager.GetInstance().Play(GERNERAL_SOUNDS_PATH + "general_sound_Example");

        // Bundle
        //await AudioManager.GetInstance().Play("audio", "general_sound_Example");
    }

    public void StopGeneralFX()
    {
        AudioManager.GetInstance().Stop(GERNERAL_SOUNDS_PATH + "general_sound_Example");

        // Bundle
        //AudioManager.GetInstance().Stop("general_sound_Example");
    }

    public void StopGeneralFXWithDestroy()
    {
        AudioManager.GetInstance().Stop(GERNERAL_SOUNDS_PATH + "general_sound_Example", false, true);

        // Bundle
        //AudioManager.GetInstance().Stop("general_sound_Example", true);
    }

    public void PauseGeneralFX()
    {
        AudioManager.GetInstance().Pause(GERNERAL_SOUNDS_PATH + "general_sound_Example");

        // Bundle
        //AudioManager.GetInstance().Pause("general_sound_Example");
    }
    #endregion

    #region Audio 【FightFX】
    public async void PlayFightFX()
    {
        await AudioManager.GetInstance().Play(FIGHT_SOUNDS_PATH + "fight_sound_Example");
    }

    public void StopFightFX()
    {
        AudioManager.GetInstance().Stop(FIGHT_SOUNDS_PATH + "fight_sound_Example");
    }

    public void StopFightFXWithDestroy()
    {
        AudioManager.GetInstance().Stop(FIGHT_SOUNDS_PATH + "fight_sound_Example", false, true);
    }

    public void PauseFightFX()
    {
        AudioManager.GetInstance().Pause(FIGHT_SOUNDS_PATH + "fight_sound_Example");
    }
    #endregion

    #region Audio 【Voice】
    public async void PlayVoice()
    {
        await AudioManager.GetInstance().Play(VOICES_PATH + "voice_Example");
    }

    public void StopVoice()
    {
        AudioManager.GetInstance().Stop(VOICES_PATH + "voice_Example");
    }

    public void StopVoiceWithDestroy()
    {
        AudioManager.GetInstance().Stop(VOICES_PATH + "voice_Example", false, true);
    }

    public void PauseVoice()
    {
        AudioManager.GetInstance().Pause(VOICES_PATH + "voice_Example");
    }
    #endregion

    #region Audio 【Atmosphere】
    public async void PlayAtmosphere()
    {
        await AudioManager.GetInstance().Play(ATMOSPHERE_PATH + "atmosphere_Example");
    }

    public void StopAtmosphere()
    {
        AudioManager.GetInstance().Stop(ATMOSPHERE_PATH + "atmosphere_Example");
    }

    public void StopAtmosphereWithDestroy()
    {
        AudioManager.GetInstance().Stop(ATMOSPHERE_PATH + "atmosphere_Example", false, true);
    }

    public void PauseAtmosphere()
    {
        AudioManager.GetInstance().Pause(ATMOSPHERE_PATH + "atmosphere_Example");
    }
    #endregion

    #region Control All Audio
    public void ResumeAll()
    {
        AudioManager.GetInstance().ResumeAll();
    }

    public void StopAll()
    {
        AudioManager.GetInstance().StopAll();
    }

    public void StopAllWithDestroy()
    {
        AudioManager.GetInstance().StopAll(false, true);
    }

    public void PauseAll()
    {
        AudioManager.GetInstance().PauseAll();
    }
    #endregion

    #region Mixer Snapshot
    public void SnapshotNormal()
    {
        var mixer = AudioManager.GetInstance().GetMixerByName("MasterMixer");
        AudioManager.GetInstance().AutoRestoreMixerExposedParams(mixer);
        AudioManager.GetInstance().SetMixerSnapshot(mixer, "Normal");
    }

    public void SnapshotPaused()
    {
        var mixer = AudioManager.GetInstance().GetMixerByName("MasterMixer");
        AudioManager.GetInstance().AutoClearMixerExposedParams(mixer);
        AudioManager.GetInstance().SetMixerSnapshot(mixer, "Paused");
    }

    public void SnapshotReverb()
    {
        var mixer = AudioManager.GetInstance().GetMixerByName("MasterMixer");
        AudioManager.GetInstance().AutoClearMixerExposedParams(mixer);
        AudioManager.GetInstance().SetMixerSnapshot(mixer, "Reverb_Heavy");
    }
    #endregion

    #region Mixer Volume
    public void SetMasterVol(float vol)
    {
        var mixer = AudioManager.GetInstance().GetMixerByName("MasterMixer");
        AudioManager.GetInstance().SetMixerExposedParam(mixer, "Master_Vol", vol);
    }

    public void SetBGMVol(float vol)
    {
        var mixer = AudioManager.GetInstance().GetMixerByName("MasterMixer");
        AudioManager.GetInstance().SetMixerExposedParam(mixer, "BGM_Vol", vol);
    }

    public void SetGeneralSoundVol(float vol)
    {
        var mixer = AudioManager.GetInstance().GetMixerByName("MasterMixer");
        AudioManager.GetInstance().SetMixerExposedParam(mixer, "General_Vol", vol);
    }

    public void SetFightSoundVol(float vol)
    {
        var mixer = AudioManager.GetInstance().GetMixerByName("MasterMixer");
        AudioManager.GetInstance().SetMixerExposedParam(mixer, "Fight_Vol", vol);
    }

    public void SetVoiceVol(float vol)
    {
        var mixer = AudioManager.GetInstance().GetMixerByName("MasterMixer");
        AudioManager.GetInstance().SetMixerExposedParam(mixer, "Voice_Vol", vol);
    }

    public void SetAtmosphereVol(float vol)
    {
        var mixer = AudioManager.GetInstance().GetMixerByName("MasterMixer");
        AudioManager.GetInstance().SetMixerExposedParam(mixer, "Atmosphere_Vol", vol);
    }
    #endregion

    #region Mixer Snapshot Weight
    public void SetReverbWeight(float val)
    {
        var mixer = AudioManager.GetInstance().GetMixerByName("MasterMixer");

        AudioMixerSnapshot[] snapshots = new AudioMixerSnapshot[]
        {
            AudioManager.GetInstance().GetMixerSnapshot(mixer, "Reverb_Light"),
            AudioManager.GetInstance().GetMixerSnapshot(mixer, "Reverb_Heavy")
        };

        float min = (1 - val);
        float max = val;
        float[] weights = new float[]
        {
            min,
            max
        };

        AudioManager.GetInstance().SetMixerTransitionToSnapshot(mixer, snapshots, weights);
    }
    #endregion
}
