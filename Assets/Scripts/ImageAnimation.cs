using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Trick
{
    public class ImageAnimation : MonoBehaviour
    {
        public float CycleTime = 1f;
        public Sprite[] sprites;
        public float CompleteInterval;
        private Image _image;
        private void Start()
        {
            _image = GetComponent<Image>();
            int spritesLength = sprites.Length;
            int index = 0;
            bool wait = false;
            Observable.Interval(TimeSpan.FromSeconds(CycleTime / spritesLength))
                      .Subscribe(l =>
                      {
                          if(wait) return;
                          _image.sprite = sprites[index];
                          index++;
                          index %= spritesLength;
                          if(CompleteInterval>0)
                          {
                              if (index == 0)
                              {
                                  wait = true;
                                  Observable.Timer(TimeSpan.FromSeconds(CompleteInterval))
                                            .Subscribe(l1 =>
                                            {
                                                wait = false;
                                            })
                                            .AddTo(this);
                              }
                          }
                      })
                      .AddTo(this);
        }
    }
}