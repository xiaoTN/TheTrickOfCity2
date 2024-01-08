using System;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Trick
{
    public partial class RoadSign
    {
        private Material _mat;

        [HideInEditorMode]
        [ShowIf(nameof(IsBound))]
        [RangeReactiveProperty(0, 1f)]
        public ReactiveProperty<float> VIntensity = new ReactiveProperty<float>();

        private const float _maxFlickerIntensity = 0.3f;
        private const float _flickerSpeed=1f;
        private float _targetFlickerIntensity;
        private float _curFlickerIntensity;
        private bool _isFlicker;

        [ShowIf(nameof(IsBound))]
        [Button("闪烁开关")]
        public void Flicker(bool isFlicker)
        {
            bool? configBorderBlink = LevelManager.Instance.Config.BorderBlink;
            if (configBorderBlink!=null)
            {
                if (((bool) configBorderBlink)==false)
                {
                    return;
                }
            }
            else
            {
                return;
            }

            _isFlicker = isFlicker;
        }

        private void OnHighLightUpdate()
        {
            if (_isFlicker|| Math.Abs(_curFlickerIntensity) > 0.001f)
            {
                if (Mathf.Abs(_targetFlickerIntensity - _curFlickerIntensity) > 0.01f)
                {
                    float sign = Mathf.Sign(_targetFlickerIntensity - _curFlickerIntensity);
                    _curFlickerIntensity += sign * Time.deltaTime * _flickerSpeed;
                    _curFlickerIntensity = Mathf.Clamp(_curFlickerIntensity, 0, _maxFlickerIntensity);
                }
                else
                {
                    _curFlickerIntensity = _targetFlickerIntensity;
                    if (Math.Abs(_targetFlickerIntensity) > 0.001f)
                    {
                        _targetFlickerIntensity = 0;
                    }
                    else
                    {
                        _targetFlickerIntensity = 0.3f;
                    }
                }
            }
            else
            {
                _targetFlickerIntensity = 0;
                _curFlickerIntensity = 0;
            }

            VIntensity.Value = _curFlickerIntensity;
        }
    }
}