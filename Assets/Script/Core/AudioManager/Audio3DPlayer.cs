﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class Audio3DPlayer: AudioPlayerBase
    {

        private  Dictionary<GameObject, Dictionary<int, AudioAsset>> bgMusicDic = new Dictionary<GameObject, Dictionary<int, AudioAsset>>();
        private  Dictionary<GameObject, List<AudioAsset>> sfxDic = new Dictionary<GameObject, List<AudioAsset>>();

        public Audio3DPlayer(MonoBehaviour mono) : base(mono) { }

    public override void SetMusicVolume(float volume)
    {
        foreach (var dics in bgMusicDic.Values)
        {
            foreach (var item in dics.Values)
            {
                item.TotleVolume = volume;
            }

        }
        musicVolume = volume;
    }
    public override void SetSFXVolume(float volume)
    {
        foreach (var item in sfxDic.Values)
        {
            for (int i = 0; i < item.Count; i++)
            {
                item[i].TotleVolume = volume;
            }
        }
        sfxVolume = volume;
    }

    public  void PlayMusic(GameObject owner, string audioName, int channel = 0, bool isLoop = true, float volumeScale = 1, float delay = 0f, float fadeTime = 0.5f)
        {
            if (owner == null)
            {
                Debug.LogError("can not play 3d player, owner is null");
                return;
            }
            AudioAsset au;
            Dictionary<int, AudioAsset> tempDic;
            if (bgMusicDic.ContainsKey(owner))
            {
                tempDic = bgMusicDic[owner];
            }
            else
            {
                tempDic = new Dictionary<int, AudioAsset>();
                bgMusicDic.Add(owner, tempDic);
            }
            if (tempDic.ContainsKey(channel))
            {
                au = tempDic[channel];
            }
            else
            {
                au = CreateAudioAsset(owner, true,true);
                tempDic.Add(channel, au);
            }

            if (au.assetName == audioName)
            {
                if (au.PlayState != AudioPlayState.Playing)
                    au.Play(delay);
            }
            else
            {
                au.assetName = audioName;
                au.SetPlaying();
                mono.StartCoroutine(EaseToChangeVolume(au, audioName, isLoop, volumeScale, delay, fadeTime));
            }
        }
        public  void PauseMusic(GameObject owner, int channel, bool isPause)
        {
            if (owner == null)
            {
                Debug.LogError("can not Pause , owner is null");
                return;
            }
            if (bgMusicDic.ContainsKey(owner))
            {
                Dictionary<int, AudioAsset> tempDic = bgMusicDic[owner];
                if (tempDic.ContainsKey(channel))
                {
                    if (isPause)
                    {
                        if (tempDic[channel].PlayState == AudioPlayState.Playing)
                            tempDic[channel].Pause();
                    }
                    else
                    {
                        if (tempDic[channel].PlayState == AudioPlayState.Pause)
                            tempDic[channel].Play();
                    }
                }

            }
        }
        public  void PauseMusicAll(bool isPause)
        {
            foreach (GameObject i in bgMusicDic.Keys)
            {
                foreach (int t in bgMusicDic[i].Keys)
                    PauseMusic(i, t, isPause);
            }
        }

        public  void StopMusic(GameObject owner, int channel)
        {
            if (bgMusicDic.ContainsKey(owner))
            {
                Dictionary<int, AudioAsset> tempDic = bgMusicDic[owner];
                if (tempDic.ContainsKey(channel))
                {
                    tempDic[channel].Stop();
                }
            }

        }
        public  void StopMusicOneAll(GameObject owner)
        {
            if (bgMusicDic.ContainsKey(owner))
            {
                List<int> list = new List<int>(bgMusicDic[owner].Keys);
                for (int i = 0; i < list.Count; i++)
                {
                    StopMusic(owner, list[i]);
                }
            }

        }
        public  void StopMusicAll()
        {
            List<GameObject> list = new List<GameObject>(bgMusicDic.Keys);
            for (int i = 0; i < list.Count; i++)
            {
                StopMusicOneAll(list[i]);
            }
        }
        public  void ReleaseMusic(GameObject owner)
        {
            if (bgMusicDic.ContainsKey(owner))
            {
                StopMusicOneAll(owner);
                List<AudioAsset> list = new List<AudioAsset>(bgMusicDic[owner].Values);
                for (int i = 0; i < list.Count; i++)
                {
                    Object.Destroy(list[i].audioSource);
                }
                list.Clear();
            }
            bgMusicDic.Remove(owner);
        }
        public  void ReleaseMusicAll()
        {
            List<GameObject> list = new List<GameObject>(sfxDic.Keys);
            for (int i = 0; i < list.Count; i++)
            {
                ReleaseMusic(list[i]);
            }
            bgMusicDic.Clear();
        }

        public  void PlaySFX(GameObject owner, string name, float volumeScale = 1f, float delay = 0f)
        {
            AudioClip ac = GetAudioClip(name);
            AudioAsset aa = GetEmptyAudioAssetFromSFXList(owner);
            aa.audioSource.clip = ac;
            aa.assetName = name;
            aa.Play(delay);
            aa.VolumeScale = volumeScale;
            ClearMoreAudioAsset(owner);
        }
        public  void PlaySFX(Vector3 position, string name, float volumeScale = 1f, float delay = 0f)
        {
            AudioClip ac = GetAudioClip(name);
            if (ac)
                mono.StartCoroutine(PlaySFXIEnumerator(position, ac, AudioPlayManager.TotleVolume* AudioPlayManager.SFXVolume * volumeScale, delay));
        }
        private  IEnumerator PlaySFXIEnumerator(Vector3 position, AudioClip ac, float volume, float delay)
        {
            yield return new WaitForSeconds(delay);
            AudioSource.PlayClipAtPoint(ac, position, volume);
        }
        public  void PauseSFXAll(bool isPause)
        {
            List<GameObject> list = new List<GameObject>(sfxDic.Keys);
            for (int j = 0; j < list.Count; j++)
            {
                List<AudioAsset> sfxList = sfxDic[list[j]];
                for (int i = 0; i < sfxList.Count; i++)
                {
                    if (isPause)
                    {
                        if (sfxList[i].PlayState == AudioPlayState.Playing)
                            sfxList[i].Pause();
                    }
                    else
                    {
                        if (sfxList[i].PlayState == AudioPlayState.Pause)
                            sfxList[i].Play();
                    }
                }
            }
        }
        public  void ReleaseSFX(GameObject owner)
        {
            if (owner && sfxDic.ContainsKey(owner))
            {
                List<AudioAsset> sfxList = sfxDic[owner];
                for (int i = 0; i < sfxList.Count; i++)
                {
                    Object.Destroy(sfxList[i].audioSource);
                }
                sfxList.Clear();
                sfxDic.Remove(owner);
            }
        }
        public  void ReleaseSFXAll()
        {
            List<GameObject> list = new List<GameObject>(sfxDic.Keys);
            for (int i = 0; i < list.Count; i++)
            {
                ReleaseSFX(list[i]);
            }
            sfxDic.Clear();
        }
        private  AudioAsset GetEmptyAudioAssetFromSFXList(GameObject owner)
        {
            AudioAsset au = null;
            List<AudioAsset> sfxList = null;
            if (sfxDic.ContainsKey(owner))
            {
                sfxList = sfxDic[owner];
                if (sfxList.Count > 0)
                {
                    for (int i = 0; i < sfxList.Count; i++)
                    {
                        if (sfxList[i].PlayState == AudioPlayState.Stop)
                            au = sfxList[i];
                    }

                }

            }
            else
            {
                sfxList = new List<AudioAsset>();
                sfxDic.Add(owner, sfxList);

            }
            if (au == null)
            {
                au = CreateAudioAsset(owner, true,false);
                sfxList.Add(au);
            }

            return au;
        }

        private List<AudioAsset> tempClearList = new List<AudioAsset>();
        private  void ClearMoreAudioAsset(GameObject owner)
        {
            if (sfxDic.ContainsKey(owner))
            {
                List<AudioAsset> sfxList = sfxDic[owner];
                if (sfxList.Count > maxSFXAudioAssetNum)
                {
                    for (int i = 0; i < sfxList.Count; i++)
                    {
                        if (sfxList[i].PlayState == AudioPlayState.Stop)
                        {
                            tempClearList.Add(sfxList[i]);
                        }
                    }

                    for (int i = 0; i < tempClearList.Count; i++)
                    {
                        if (sfxList.Count <= maxSFXAudioAssetNum)
                            break;
                        Object.Destroy(tempClearList[i].audioSource);
                        sfxList.Remove(tempClearList[i]);
                    }
                    tempClearList.Clear();
                }
            }
        }

        private List<GameObject> clearList = new List<GameObject>();
        public  void ClearDestroyObjectData()
        {
            if (bgMusicDic.Count > 0)
            {
                clearList.Clear();
                clearList.AddRange(bgMusicDic.Keys);

                for (int i = 0; i < clearList.Count; i++)
                {
                    if (clearList[i] == null)
                        bgMusicDic.Remove(clearList[i]);
                }
            }
            if (sfxDic.Count > 0)
            {
                clearList.Clear();
                clearList.AddRange(sfxDic.Keys);
                for (int i = 0; i < clearList.Count; i++)
                {
                    if (clearList[i] == null)
                        sfxDic.Remove(clearList[i]);
                }
            }
        }

    }

