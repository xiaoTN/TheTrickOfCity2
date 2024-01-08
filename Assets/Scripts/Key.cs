using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Trick
{
    public class Key : MonoBehaviour
    {
        [LabelText("光面")]
        public bool IsDay;

        [NonSerialized]
        private RoadSign _roadSign;

        public GameObject SunModel;
        public GameObject NightModel;

        [LabelText("是否被激活")]
        public bool IsActive
        {
            get
            {
                return _roadSign != null &&
                       _roadSign.Box != null &&
                       IsDay == _roadSign.Box.IsDay;
            }
        }
        
        public void Init(RoadSign roadSign)
        {
            _roadSign = roadSign;
            GameObject.Destroy(transform.GetChild(0).gameObject);
            _go = GameObject.Instantiate(IsDay ? SunModel : NightModel, transform);
        }

        public GameObject HighDayEfx;
        public GameObject HighNightEfx;
        private GameObject _highEfxInstance;
        private GameObject _go;

        [Button]
        public void HightKey(bool high)
        {
            if (high)
            {
                _highEfxInstance = Instantiate(IsDay ? HighDayEfx : HighNightEfx, transform.parent);
                _highEfxInstance.transform.localPosition = new Vector3(0, 0.02f, 0);
                _highEfxInstance.transform.localScale = Vector3.zero;
                _highEfxInstance.transform.DOScale(1f, 0.3f);

                _go.transform.DOLocalMoveY(-0.15f, 0.3f).SetEase(Ease.Linear);
            }
            else
            {
                _go.transform.DOLocalMoveY(0, 0.3f).SetEase(Ease.Linear);
                if (_highEfxInstance)
                {
                    _highEfxInstance.transform.DOScale(0f, 0.3f).OnComplete(() =>
                    {
                        Destroy(_highEfxInstance);
                    });
                }
            }

            // go.transform.localRotation=Quaternion.identity;
        }

        public void ShowNPC(bool isShow)
        {
            gameObject.SetActive(isShow);
            if (_highEfxInstance != null)
            {
                _highEfxInstance.SetActive(isShow);
            }
        }
    }
}