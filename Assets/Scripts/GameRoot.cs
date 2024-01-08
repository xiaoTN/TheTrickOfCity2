using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using DG.Tweening;
using RenderHeads.Media.AVProVideo;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Trick
{
    [DefaultExecutionOrder(100)]
    public class GameRoot : MonoBehaviour
    {
        public static GameRoot Instance;

        public Button RestartBtn;
        public Button RebackBtn;
        public Button RestartBtn1;
        public Button RebackBtn1;

        public GameObject MainUI;

        public DisplayUGUI DisplayUgui;
        public MediaPlayer mediaPlayer1;
        public MediaPlayer mediaPlayer2;
        public MediaPlayer mediaPlayer3;
        public GameObject anyKeyGo;
        public Image BlackImg;
        public Text Text1;

        public Canvas Canvas;
        private void Awake()
        {
            float height = Screen.currentResolution.height*0.67f;
            Screen.SetResolution((int) (height * 1734 / 1222f), (int)height, false);
            BlackImg.gameObject.SetActive(true);
            Color blackImgColor = BlackImg.color;
            blackImgColor.a = 1;
            BlackImg.color = blackImgColor;
        }

        private IEnumerator Start()
        {
            GameObject findWithTag = GameObject.FindWithTag("LevelRoot");
            foreach (Transform child in findWithTag.transform)
            {
                Destroy(child.gameObject);
            }
            DisplayUgui._mediaPlayer = mediaPlayer1;
            DisplayUgui._mediaPlayer.Control.Play();
            bool complete = false;
            mediaPlayer1.Events.AsObservable().Subscribe(tuple =>
            {
                (MediaPlayer mediaPlayer, MediaPlayerEvent.EventType eventType, ErrorCode errorCode) = tuple;
                if (eventType == MediaPlayerEvent.EventType.FirstFrameReady)
                {
                    DisplayUgui.gameObject.SetActive(true);
                    BlackImg.DOFade(0, 0.3f).SetEase(Ease.Linear);
                }
                if (eventType == MediaPlayerEvent.EventType.FinishedPlaying)
                {
                    complete = true;
                }
            }).AddTo(this);
            while (complete==false)
            {
                yield return null;
            }
            complete = false;
            DisplayUgui._mediaPlayer = mediaPlayer2;
            mediaPlayer2.Control.Play();
            // mediaPlayer2.Events.AsObservable().Subscribe(tuple =>
            // {
            //     (MediaPlayer mediaPlayer, MediaPlayerEvent.EventType eventType, ErrorCode errorCode) = tuple;
            //     if (eventType == MediaPlayerEvent.EventType.FinishedPlaying)
            //     {
            //         complete = true;
            //     }
            // }).AddTo(this);
            // while (complete==false)
            // {
            //     yield return null;
            // }
            //显示任意键跳过
            // anyKeyGo.SetActive(true);
            int index = 0;
            IDisposable disposable = Observable.Interval(TimeSpan.FromSeconds(0.5f))
                                               .Subscribe(l =>
                                               {
                                                   index++;
                                                   anyKeyGo.SetActive(index % 2 == 0);
                                               });
            while (true)
            {
                yield return null;
                if (Input.anyKeyDown)
                {
                    DisplayUgui.gameObject.SetActive(false);
                    anyKeyGo.SetActive(false);
                    disposable.Dispose();
                    break;
                }
            }

            Color blackImgColor = BlackImg.color;
            blackImgColor.a = 1;
            BlackImg.color = blackImgColor;
            yield return Text1.DOFade(1, 0.1f).WaitForCompletion();
            yield return new WaitForSeconds(3f);
            Init();
            BlackImg.DOFade(0, 1f).WaitForCompletion();
            Text1.DOFade(0, 1f).WaitForCompletion();
            yield return new WaitForSeconds(1f);
            BlackImg.gameObject.SetActive(false);
            Text1.gameObject.SetActive(false);

            AudioSource audioSource = GetComponent<AudioSource>();
            DOTween.To(() => 1, x =>
            {
                audioSource.volume = x;
            }, 0f, 1f).SetEase(Ease.Linear).OnComplete(() =>
            {
                Destroy(audioSource);
            });
        }

        private void Init()
        {
            Instance = this;
            DOTween.Init();
            ResManager.Instance.Init();
            LevelManager.Instance.Init();
            UIManager.Instance.Init();
            DialogManager.Instance.Init();
            UIManager.Instance.TrickGamePanel.gameObject.SetActive(false);

            RestartBtn.OnClickAsObservable()
                      .Merge(RestartBtn1.OnClickAsObservable())
                      .Subscribe(unit =>
                      {
                          LevelManager.Instance.RestartLevel();
                      });

            RebackBtn.OnClickAsObservable()
                     .Merge(RebackBtn1.OnClickAsObservable())
                     .Subscribe(unit =>
                     {
                         LevelManager.Instance.CurLevelRoot.UnDo();
                     });

            LevelManager.Instance.Config=new Config();
            LevelManager.Instance.Config.ReadGame();
            gameObject.UpdateAsObservable()
                      .Subscribe(unit =>
                      {
                          KeyCode WCode = KeyCode.W;
                          Enum.TryParse(LevelManager.Instance.Config.KeyboardUp, out WCode);
                          KeyCode SCode = KeyCode.S;
                          Enum.TryParse(LevelManager.Instance.Config.KeyboardDown, out SCode);
                          KeyCode ACode = KeyCode.A;
                          Enum.TryParse(LevelManager.Instance.Config.KeyboardLeft, out ACode);
                          KeyCode DCode = KeyCode.D;
                          Enum.TryParse(LevelManager.Instance.Config.KeyboardRight, out DCode);
                          if (Input.GetKeyDown(WCode))
                          {
                              MessageBroker.Default.Publish(KeyOp.W.ToString());
                          }

                          if (Input.GetKeyDown(DCode))
                          {
                              MessageBroker.Default.Publish(KeyOp.D.ToString());
                          }

                          if (Input.GetKeyDown(SCode))
                          {
                              MessageBroker.Default.Publish(KeyOp.S.ToString());
                          }

                          if (Input.GetKeyDown(ACode))
                          {
                              MessageBroker.Default.Publish(KeyOp.A.ToString());
                          }
                      });
            LevelManager.Instance.EnterLevel(LevelManager.Instance.CurIndex);
            MainUI.SetActive(false);
        }

        public void ShowReback(bool show)
        {
            RebackBtn.gameObject.SetActive(show);
            RebackBtn1.gameObject.SetActive(show);
        }

        public void ShowRestart(bool show)
        {
            RestartBtn.gameObject.SetActive(show);
            RestartBtn1.gameObject.SetActive(show);
        }
    }
}