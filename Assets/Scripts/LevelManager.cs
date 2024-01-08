using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Json;
using DG.Tweening;
using Newtonsoft.Json;
using RenderHeads.Media.AVProVideo;
using Sirenix.OdinInspector;
using UniRx;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace Trick
{
    public class LevelManager : MonoSingleton<LevelManager>
    {
        public int CurIndex
        {
            get
            {
                if (Config.EnterLevel != null)
                {
                    return (int) Config.EnterLevel;
                }

                return 1;
            }
            set { Config.EnterLevel = value; }
        }
        public  LevelRoot  CurLevelRoot;
        private GameObject _levels;

        public bool PlayerIsDayInTrick
        {
            get { return Player.Instance.CurRoadSign.Value.IsDay; }
        }

        public SaveInfo SaveInfo;
        public Config Config;
        public bool PlayMovie;
        public int CycleIndex
        {
            get
            {
                if (Config.PlayThrough != null)
                {
                    return (int) Config.PlayThrough;
                }

                return 0;
            }
        }

        public bool HaveSuccessBoss;
        public override void Init()
        {
            base.Init();
          
            SaveInfo= JsonUtils.ReadJson<SaveInfo>(nameof(SaveInfo));
            _levels = GameObject.Find("Levels");
            LevelRoot[] levels = _levels.GetComponentsInChildren<LevelRoot>();
            foreach (LevelRoot levelRoot in levels)
            {
                levelRoot.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 保存当前游戏数据
        /// </summary>
        public void SaveGame()
        {
            JsonUtils.WriteJson(SaveInfo,nameof(SaveInfo));
        }

        private void Update()
        {
            if(Input.GetKey(KeyCode.LeftControl)
            && Input.GetKey(KeyCode.LeftShift)
            && Input.GetKey(KeyCode.LeftAlt))
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                    EnterLevel(1);
                if (Input.GetKeyDown(KeyCode.Alpha2))
                    EnterLevel(2);
                if (Input.GetKeyDown(KeyCode.Alpha3))
                    EnterLevel(3);
                if (Input.GetKeyDown(KeyCode.Alpha4))
                    EnterLevel(4);
                if (Input.GetKeyDown(KeyCode.Alpha5))
                    EnterLevel(5);
                if (Input.GetKeyDown(KeyCode.Alpha6))
                    EnterLevel(6);
                if (Input.GetKeyDown(KeyCode.Alpha7))
                    EnterLevel(7);
                if (Input.GetKeyDown(KeyCode.Alpha8))
                    EnterLevel(8);
                if (Input.GetKeyDown(KeyCode.Alpha9))
                    EnterLevel(9);
                if (Input.GetKeyDown(KeyCode.Alpha0))
                    EnterLevel(10);
                if (Input.GetKeyDown(KeyCode.F1))
                    EnterLevel(11);
                if (Input.GetKeyDown(KeyCode.F2))
                    EnterLevel(12);
                if (Input.GetKeyDown(KeyCode.F3))
                    EnterLevel(13);
                if (Input.GetKeyDown(KeyCode.F4))
                    EnterLevel(14);
                if (Input.GetKeyDown(KeyCode.F5))
                    EnterLevel(15);
                if (Input.GetKeyDown(KeyCode.F6))
                    EnterLevel(16);

                if (Input.GetKeyDown(KeyCode.Delete))
                {
                    ClearData();
                    Application.Quit();
                }
            }

#if UNITY_EDITOR

            if (Selection.gameObjects.Length > 0)
            {
                GameObject go = Selection.gameObjects[0];
                Box box = go.GetComponent<Box>();
                if(box)
                {
                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        if (Input.GetKeyDown(KeyCode.UpArrow))
                            box.JumpTo(KeyOp.W);
                        if (Input.GetKeyDown(KeyCode.DownArrow))
                            box.JumpTo(KeyOp.S);
                        if (Input.GetKeyDown(KeyCode.LeftArrow))
                            box.JumpTo(KeyOp.A);
                        if (Input.GetKeyDown(KeyCode.RightArrow))
                            box.JumpTo(KeyOp.D);
                    }
                    else if(Input.GetKey(KeyCode.LeftShift))
                    {
                        if (Input.GetKeyDown(KeyCode.UpArrow))
                            box.SkatingTo(KeyOp.W);
                        if (Input.GetKeyDown(KeyCode.DownArrow))
                            box.SkatingTo(KeyOp.S);
                        if (Input.GetKeyDown(KeyCode.LeftArrow))
                            box.SkatingTo(KeyOp.A);
                        if (Input.GetKeyDown(KeyCode.RightArrow))
                            box.SkatingTo(KeyOp.D);
                    }
                    else
                    {
                        if (Input.GetKeyDown(KeyCode.UpArrow))
                            box.PushTo(KeyOp.W);
                        if (Input.GetKeyDown(KeyCode.DownArrow))
                            box.PushTo(KeyOp.S);
                        if (Input.GetKeyDown(KeyCode.LeftArrow))
                            box.PushTo(KeyOp.A);
                        if (Input.GetKeyDown(KeyCode.RightArrow))
                            box.PushTo(KeyOp.D);
                    }
                }
            }

#endif
        }

        [Button]
        public void EnterNextLevel()
        {
            if (_revertIndex)
            {
                EnterLevel(CurIndex - 1);
            }
            else
            {
                EnterLevel(CurIndex + 1);
            }
        }

        public void RestartLevel()
        {
            SaveInfo.LevelInfos[CurIndex]=new RoadInfo()
            {
                X1 = CurLevelRoot.StartView1IndexX,
                Y1 = CurLevelRoot.StartView1IndexY,
                X2 = CurLevelRoot.StartView2IndexX,
                Y2 = CurLevelRoot.StartView2IndexY,
            };
            EnterLevel(CurIndex);
        }

        public void LeaveLevel()
        {
            Player.Instance.IsStop = false;
            Player.Instance.transform.SetParent(null);

            if (CurLevelRoot != null)
            {
                Destroy(CurLevelRoot.gameObject);
            }
        }

        /// <summary>
        /// 倒叙关卡
        /// </summary>
        private bool _revertIndex
        {
            get { return Config.PlayThrough >0; }
        }
        [Button]
        public void EnterLevel(int index)
        {
            if (SwitchingLevel)
            {
                return;
            }

            Player.Instance.EnableTransferToTower = false;
            if (CurLevelRoot != null && CurLevelRoot.Index != index)
            {
                //进入别的关卡
                CurLevelRoot.SaveLevel();
            }

            UIManager.Instance.TrickGamePanel.gameObject.SetActive(true);
            UIManager.Instance.LevelTip.text = $"关卡：{17-index}";
            EnterLevelAsync(index).ToObservable().Subscribe().AddTo(this);
        }

        public bool SwitchingLevel = false;
        public bool SwitchingTower = false;
        public bool BoxMoving = false;
        private IEnumerator EnterLevelAsync(int index)
        {
            AudioManager.Instance.LoadAudio(index);
            if (index > 16 || index <= 0)
            {
                Config.PlayThrough++;
                Debug.Log("您已通过所有关卡");
                if (Config.PlayThrough == 1)
                {
                    //动画
                    PlayMovie = true;
                    UIManager.Instance.TrickGamePanel.alpha = 0;
                    GameRoot.Instance.DisplayUgui.gameObject.SetActive(true);
                    GameRoot.Instance.DisplayUgui._mediaPlayer = GameRoot.Instance.mediaPlayer3;
                    bool complete = false;
                    GameRoot.Instance.mediaPlayer3.Events.AsObservable().Subscribe(tuple =>
                    {
                        if (tuple.Item2 == MediaPlayerEvent.EventType.Started)
                        {
                            GameRoot.Instance.Canvas.sortingOrder = 1;
                            EnterLevel(16);
                        }
                        if (tuple.Item2 == MediaPlayerEvent.EventType.FinishedPlaying)
                        {
                            complete = true;
                        }
                    });
                    GameRoot.Instance.mediaPlayer3.Control.Play();
                    while (complete==false)
                    {
                        yield return null;
                    }

                    int index1 = 0;
                    IDisposable disposable = Observable.Interval(TimeSpan.FromSeconds(0.5f))
                                                       .Subscribe(l =>
                                                       {
                                                           index1++;
                                                           GameRoot.Instance.anyKeyGo.SetActive(index1 % 2 == 0);
                                                       });
                    while (true)
                    {
                        if (Input.anyKeyDown)
                        {
                            UIManager.Instance.TrickGamePanel.alpha = 1;
                            GameRoot.Instance.anyKeyGo.SetActive(false);
                            disposable.Dispose();
                            break;
                        }
                        yield return null;
                    }

                    PlayMovie = false;
                    GameRoot.Instance.Canvas.sortingOrder = -2;
                    GameRoot.Instance.DisplayUgui.gameObject.SetActive(false);
                }
                else
                {
                    EnterLevel(16);
                }
                yield break;
            }
            if (CurLevelRoot != null && CurLevelRoot.InTower.Value)
            {
                //先切回诡计之城
                bool complete = false;
                CurLevelRoot.ExitTower(() =>
                {
                    complete = true;
                });
                while (complete==false)
                {
                    yield return null;
                }

                yield return EnterLevelAsync(index);
                yield break;
            }
            
            UIManager.Instance.HideRightTopUI();
            SwitchingLevel = true;
            Player.Instance.IsStop = false;
            Player.Instance.transform.SetParent(null);
            CurIndex = index;

            Config.SaveGame();
            JsonUtils.WriteJson(SaveInfo,nameof(SaveInfo));
            if (CurLevelRoot != null)
            {
                CurLevelRoot.PassLevel();
                CanvasGroup canvasGroup = CurLevelRoot.GetComponentInChildren<CanvasGroup>();
                DOTween.To(() => 1, x => canvasGroup.alpha = x, 0f, 1f).SetEase(Ease.Linear);
                yield return new WaitForSeconds(1f);
                if (CurLevelRoot != null)
                {
                    Destroy(CurLevelRoot.gameObject);
                }
            }

            GameObject levelPrefab = ResManager.Instance.LoadLevel(index);
            
            GameObject level = Instantiate(levelPrefab, _levels.transform);
            UIManager.Instance.IsDraging = false;
            UIExpand uiExpand = level.GetComponentInChildren<UIExpand>();
            UIManager.Instance.CanvasX = uiExpand.XCount * 2 * UIManager.XAdsord + 0.01f;
            UIManager.Instance.CanvasY = uiExpand.YCount * 2 * UIManager.YAdsord + 0.01f;
            CanvasGroup newGroup = level.GetComponentInChildren<CanvasGroup>();
            newGroup.alpha = 0;
            CanvasScaler canvasScaler = newGroup.GetComponent<CanvasScaler>();
            canvasScaler.referenceResolution = new Vector2(UIManager.Instance.CanvasX, UIManager.Instance.CanvasY);

            //调整UI
            UIDrag[] uis = canvasScaler.GetComponentsInChildren<UIDrag>();
            foreach (UIDrag uiDrag in uis)
            {
                uiDrag.AdjustUI();
            }

            DOTween.To(() => 0, x => newGroup.alpha = x, 1f, 1f).SetEase(Ease.Linear);
            LevelRoot levelRoot = level.GetComponent<LevelRoot>();
            levelRoot.OnPassLevel.Subscribe(i =>
            {
                // AudioManager.Instance.PlayAudio("通关");
                EnterLevel(i + 1);
            }).AddTo(level);

            CurLevelRoot = levelRoot;
            CurLevelRoot.gameObject.SetActive(true);
            CurLevelRoot.Init();
            CurLevelRoot.ReloadLevel(SaveInfo.LevelInfos[index]);
            yield return new WaitForSeconds(1f);
            UIManager.Instance.OnEndDragUI.OnNext(uis[0]);
            SwitchingLevel = false;
            Player.Instance.IsStop = true;
        }

        [Button("清空数据")]
        public void ClearData()
        {
            SaveInfo=new SaveInfo()
            {
                LevelInfos = Enumerable.Range(0,17).Select(i => new RoadInfo()
                {
                    BoxInfos = new List<Tuple<int, int>>()
                }).ToList(),
                TowerPlayerInfo = new TowerPlayerInfo()
                {
                    HP = 400,
                    Attack = 10,
                    Defend = 10,
                },
                TowerRoadInfos = Enumerable.Range(0,17).Select(i => new TowerRoadInfo()
                {
                    HaveDos = new List<int>()
                }).ToList()
            };
            SaveGame();
            Config=new Config()
            {
                Volume = 100,
                Music = true,
                Acoustics = true,
                GameSpeed = 1,
                KeyboardUp = "W",
                KeyboardDown = "S",
                KeyboardLeft = "A",
                KeyboardRight = "D"
            };
            Config.SaveGame();
            
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                if (Config != null)
                {
                    Config.ReadGame();
                }
            }
        }
    }

    
}