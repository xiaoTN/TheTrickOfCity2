using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Trick
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        public List<AudioClip> Clips;
        private Dictionary<string, AudioClip> _dict = new Dictionary<string, AudioClip>();
        public AudioSource DayAudio;
        public AudioSource NightAudio;
        public AudioSource DayAudioTower;
        public AudioSource NightAudioTower;

        public void EnableMusicBg(bool enable)
        {
            DayAudio.enabled = enable;
            NightAudio.enabled = enable;
            DayAudioTower.enabled = enable;
            NightAudioTower.enabled = enable;
        }

        private float _volumn;
        public void SetMusicVolumn(int volumn)
        {
            _volumn = volumn/100f;
            _volumn = Mathf.Clamp(_volumn, 0f, 1f);
        }

        private bool _enableAcoustics;
        public void EnableAcoustics(bool enable)
        {
            _enableAcoustics = enable;
        }
        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this);
            foreach (AudioClip clip in Clips)
            {
                _dict.Add(clip.name, clip);
            }
        }

        public void LoadAudio(int levelIndex)
        {
            if (levelIndex <= 6)
            {
                DayAudio.clip = _dict["光1-普通"];
                NightAudio.clip = _dict["暗1-普通"];
                DayAudioTower.clip = _dict["光1-8bit"];
                NightAudioTower.clip = _dict["暗1-8bit"];
            }
            else if (levelIndex <= 11)
            {
                DayAudio.clip = _dict["光2-普通"];
                NightAudio.clip = _dict["暗2-普通"];
                DayAudioTower.clip = _dict["光2-8bit"];
                NightAudioTower.clip = _dict["暗2-8bit"];
            }
            else if (levelIndex <= 16)
            {
                DayAudio.clip = _dict["光3-普通"];
                NightAudio.clip = _dict["暗3-普通"];
                DayAudioTower.clip = _dict["光3-8bit"];
                NightAudioTower.clip = _dict["暗3-8bit"];
            }
            DayAudio.Play();
            NightAudio.Play();
            DayAudioTower.Play();
            NightAudioTower.Play();
        }

        [Button]
        public void SwitchBg(bool day, bool tower)
        {
            if (day)
            {
                if (tower)
                {
                    DOTween.To(() => DayAudio.volume, x => DayAudio.volume = x, 0, 1f);
                    DOTween.To(() => NightAudio.volume, x => NightAudio.volume = x, 0, 1f);
                    DOTween.To(() => DayAudioTower.volume, x => DayAudioTower.volume = x, _volumn, 1f);
                    DOTween.To(() => NightAudioTower.volume, x => NightAudioTower.volume = x, 0, 1f);
                }
                else
                {
                    DOTween.To(() => DayAudio.volume, x => DayAudio.volume = x, _volumn, 1f);
                    DOTween.To(() => NightAudio.volume, x => NightAudio.volume = x, 0, 1f);
                    DOTween.To(() => DayAudioTower.volume, x => DayAudioTower.volume = x, 0, 1f);
                    DOTween.To(() => NightAudioTower.volume, x => NightAudioTower.volume = x, 0, 1f);
                }
            }
            else
            {
                if (tower)
                {
                    DOTween.To(() => DayAudio.volume, x => DayAudio.volume = x, 0, 1f);
                    DOTween.To(() => NightAudio.volume, x => NightAudio.volume = x, 0, 1f);
                    DOTween.To(() => DayAudioTower.volume, x => DayAudioTower.volume = x, 0, 1f);
                    DOTween.To(() => NightAudioTower.volume, x => NightAudioTower.volume = x, _volumn, 1f);
                }
                else
                {
                    DOTween.To(() => DayAudio.volume, x => DayAudio.volume = x, 0, 1f);
                    DOTween.To(() => NightAudio.volume, x => NightAudio.volume = x, _volumn, 1f);
                    DOTween.To(() => DayAudioTower.volume, x => DayAudioTower.volume = x, 0, 1f);
                    DOTween.To(() => NightAudioTower.volume, x => NightAudioTower.volume = x, 0, 1f);
                }
            }
        }


        public void PlayAudio(string name, float volume = 1f)
        {
            if (_enableAcoustics == false)
            {
                return;
            }

            volume *= _volumn;
            AudioSource.PlayClipAtPoint(_dict[name], Camera.main.transform.position, volume);
        }
    }
}