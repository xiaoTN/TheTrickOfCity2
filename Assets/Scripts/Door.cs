using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UniRx;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Trick
{
    public class Door : MonoBehaviour
    {
        [Required]
        [ReadOnly]
        public RoadSign       CurRoad;
        public List<RoadSign> CanLinks;

        [ChildGameObjectsOnly]
        public Animation SunAnim;
        [ChildGameObjectsOnly]
        public Animation NightAnim;

        private Animation _curAnim;
        public List<RoadSign> KeyRoads;

        public bool IsOpen = false;
        private const string _openAnimName="open";

        public void Init()
        {
            SunAnim.gameObject.SetActive(CurRoad.IsDay);
            NightAnim.gameObject.SetActive(!CurRoad.IsDay);
            _curAnim = CurRoad.IsDay ? SunAnim : NightAnim;
            Observable.TimerFrame(1).Subscribe(l =>
            {
                _curAnim.Play();
            });
            foreach (RoadSign roadSign in KeyRoads)
            {
                roadSign.ObserveEveryValueChanged(sign => sign.HaveKey && sign.HaveBox&& sign.Key.IsDay==sign.Box.IsDay)
                        .Skip(1)
                        .Subscribe(b =>
                        {
                            //key 播放动画
                            if (b)
                            {
                                AudioManager.Instance.PlayAudio("开关被压下或弹起");
                                roadSign.Key.HightKey(true);
                            }
                            else
                            {
                                AudioManager.Instance.PlayAudio("开关被压下或弹起");
                                roadSign.Key.HightKey(false);
                            }
                        })
                        .AddTo(roadSign);
            }
            
            gameObject.ObserveEveryValueChanged(_ =>
            {
                bool isAllSuccess = true;
                foreach (RoadSign roadSign in KeyRoads)
                {
                    isAllSuccess &= roadSign.Key.IsActive;
                }

                return isAllSuccess;
            }).Subscribe(b =>
            {
                if (b)
                {
                    Open();
                }
                else
                {
                    Close();
                }

                RoadSign playerCurRoad = Player.Instance.CurRoadSign.Value;
                if (playerCurRoad != null)
                {
                    playerCurRoad.ForceUpdateUI();
                }

            }).AddTo(this);
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            KeyRoads.RemoveAll(sign =>
            {
                if (sign == null)
                {
                    EditorUtility.SetDirty(this);
                    return true;
                }

                return false;
            });
#endif
        }

        public GameObject DayEfx;
        public GameObject NightEfx;

        [Button]
        public void Open()
        {
            IsOpen = true;
            AudioManager.Instance.PlayAudio("开门");
            _curAnim[_openAnimName].time = 0f;
            _curAnim[_openAnimName].speed = 1f;
            _curAnim.CrossFade(_openAnimName);
            Repair();
            DayEfx.SetActive(false);
            NightEfx.SetActive(false);
        }

        [Button]
        public void Close()
        {
            DayEfx.SetActive(true);
            NightEfx.SetActive(true);
            IsOpen = false;
            AudioManager.Instance.PlayAudio("关门");
            _curAnim[_openAnimName].time = _curAnim[_openAnimName].clip.length;
            _curAnim[_openAnimName].speed = -1f;
            _curAnim.CrossFade(_openAnimName);
            Break();
        }

        [Button]
        public void Break()
        {
            foreach (RoadSign roadSign in CanLinks)
            {
                roadSign.Links.Remove(CurRoad);
            }
        }

        public void Repair()
        {
            foreach (RoadSign sign in CanLinks)
            {
                CurRoad.LinkRoad(sign);
            }
        }
    }
}