using System;
using System.Collections.Generic;
using System.Linq;
using Common.Json;
using Sirenix.OdinInspector;
using Trick.Tower;
using UniRx;
using UniRx.Triggers;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Trick
{
    public partial class LevelRoot
    {
        [BoxGroup("魔塔")]
        [NonSerialized]
        [ShowInInspector]
        public TowerPlayer TowerPlayer;

        private IDisposable _disposable1;

        private GameObject _towerPartGo;

        private GameObject _root;
        private Vector2 _v1Apo;
        private Vector2 _v1Po;
        private Vector2 _v2Apo;
        private Vector2 _v2Po;

        [BoxGroup("魔塔")]
        [ShowInInspector]
        [NonSerialized]
        [HideInEditorMode]
        [LabelText("在魔塔中")]
        public ReactiveProperty<bool> InTower = new ReactiveProperty<bool>();

        [BoxGroup("魔塔")]
        [HideInEditorMode]
        [ShowInInspector]
        private List<RoadSign> _towerRoads;
#if UNITY_EDITOR
        [BoxGroup("魔塔")]
        [Button("创建魔塔(Editor)")]
        [HideInPlayMode]
        private void CreateTowerInEditor()
        {
            _root = new GameObject("临时创建，记得删除");
            _root.hideFlags = HideFlags.DontSaveInEditor;
            RoadSign[] roadSigns = GetComponentsInChildren<RoadSign>();
            foreach (RoadSign roadSign in roadSigns)
            {
                if (roadSign.TowerObjectPrefab != null)
                {
                    GameObject instance = PrefabUtility.InstantiatePrefab(roadSign.TowerObjectPrefab) as GameObject;
                    instance.transform.SetParent(_root.transform);
                    instance.transform.position = roadSign.TopestPosition + Vector3.up * 0.1f;
                    instance.transform.eulerAngles = new Vector3(90, 0, 0);
                    instance.transform.localScale = Vector3.one * 2f;
                }
            }
        }

        [BoxGroup("魔塔")]
        [Button("销毁魔塔(Editor)")]
        [HideInPlayMode]
        private void DestroyTowerInEditor()
        {
            DestroyImmediate(_root);
        }
#endif

        private void InitTower()
        {
        }

        [BoxGroup("魔塔")]
        [HideInEditorMode]
        [Button("创建魔塔")]
        public void EnterTower(Action complete=null)
        {
            AudioManager.Instance.SwitchBg(Player.Instance.CurRoadSign.Value.IsDay,true);
            Debug.Log("进入魔塔");
            if (IsLinkSpecialBound)
            {
                BreakLinkRoad();
            }

            InTower.Value = true;
            LevelManager.Instance.SwitchingTower = true;
            UIManager.Instance.ShowTowerPanel(true);
            ShowNPC(false);
            EnterTowerEffect(true, () =>
            {
                CreateTower();
                LevelManager.Instance.SwitchingTower = false;
                complete?.Invoke();
            });
         
        }

        [HideInEditorMode]
        [BoxGroup("魔塔")]
        [Button]
        private void CreateTower(TowerRoadInfo towerRoadInfo=null)
        {
            _towerRoads = _allRoadSigns.GroupBy(sign => $"{(int) sign.transform.position.x}{(int) sign.transform.position.z}").Select(signs =>
            {
                return signs.ToList().Find(sign => sign.TowerObjectPrefab != null) ?? signs.First();
            }).ToList();
            _towerRoads.Sort((x, y) => x.Index.CompareTo(y.Index));
            foreach (RoadSign towerRoad in _towerRoads)
            {
                if (LevelManager.Instance.SaveInfo.TowerRoadInfos[Index].HaveDos.Contains(towerRoad.Index))
                {
                    towerRoad.HaveDo = true;
                }
            }
            
            _towerPartGo = new GameObject("TowerRoot");

            foreach (RoadSign tempRoad in _towerRoads)
            {
                tempRoad.SwitchToTower();
                tempRoad.TowerRoot.SetParent(_towerPartGo.transform);
            }

            LinkTowerRoadX(_towerRoads);
            LinkTowerRoadZ(_towerRoads);
            CreateTowerPlayer();
            SetPlayer(Player.Instance.CurRoadSign.Value, TowerPlayer);
            ShowRoadRenderer(false);
            UIManager.Instance.TowerPanelInstance.UpdateUI();
        }

        [HideInEditorMode]
        [BoxGroup("魔塔")]
        [Button]
        private void RecreateTower(TowerRoadInfo towerRoadInfo)
        {
            Destroy(_towerPartGo);
            Destroy(TowerPlayer.gameObject);
            CreateTower(towerRoadInfo);
        }

        [BoxGroup("魔塔")]
        [HideInEditorMode]
        [Button("销毁魔塔")]
        public void ExitTower(Action complete=null)
        {
            AudioManager.Instance.SwitchBg(Player.Instance.CurRoadSign.Value.IsDay,false);
            Debug.Log("离开魔塔");
            LevelManager.Instance.SwitchingTower = true;
            _baseOpers.Clear();
            InTower.Value = false;
            ShowRoadRenderer(true);
            UIManager.Instance.ShowTowerPanel(false);
            Destroy(_towerPartGo);
            DestroyTowerPlayer();
            Player.Instance.EnableTransferToTower = false;
            EnterTowerEffect(false, () =>
            {
                LevelManager.Instance.SwitchingTower = false;
                Player.Instance.SetToRoadAndSetPos(TowerPlayer.CurRoadSign.Value);
                TryLinkAllSameRoad();
                ShowNPC(true);
                Player.Instance.EnableTransferToTower = true;
                complete?.Invoke();
            });

            SaveTowerLevel();
        }
        [BoxGroup("魔塔")]
        [HideInEditorMode]
        [Button]
        public void SaveTowerLevel()
        {
            TowerRoadInfo saveInfoTowerRoadInfo = LevelManager.Instance.SaveInfo.TowerRoadInfos[LevelManager.Instance.CurIndex];
            foreach (RoadSign road in _towerRoads)
            {
                if (road.HaveDo)
                {
                    if (saveInfoTowerRoadInfo.HaveDos == null)
                    {
                        saveInfoTowerRoadInfo.HaveDos = new List<int>();
                    }

                    if (saveInfoTowerRoadInfo.HaveDos.Contains(road.Index) == false)
                    {
                        saveInfoTowerRoadInfo.HaveDos.Add(road.Index);
                    }
                }
            }

            saveInfoTowerRoadInfo.HaveDos.Sort();


            LevelManager.Instance.SaveGame();
        }

        private void ShowNPC(bool isShow)
        {
            _levelPart1.ShowSky(isShow);
            _levelPart2.ShowSky(isShow);
            foreach (RoadSign allRoadSign in _allRoadSigns)
            {
                allRoadSign.ShowNPC(isShow);
            }
        }

        private void ShowRoadRenderer(bool isShow)
        {
            foreach (RoadSign allRoadSign in _allRoadSigns)
            {
                allRoadSign.ShowRoadRenderer(isShow);
            }
        }

        /// <summary>
        /// 创建玩家
        /// </summary>
        public void CreateTowerPlayer()
        {
            GameObject playerRole = ResManager.Instance.LoadRole("TowerPlayer");
            GameObject playerRoleGo = Instantiate(playerRole);
            TowerPlayer = playerRoleGo.GetComponent<TowerPlayer>();
            TowerPlayer.Init(LevelManager.Instance.SaveInfo.TowerPlayerInfo.HP,
                LevelManager.Instance.SaveInfo.TowerPlayerInfo.Attack,
                LevelManager.Instance.SaveInfo.TowerPlayerInfo.Defend);
            TowerPlayer.Switch(LevelManager.Instance.PlayerIsDayInTrick);
            _disposable1 = playerRoleGo.UpdateAsObservable()
                                       .Subscribe(l =>
                                       {
                                           if (Input.GetKeyDown(KeyCode.W))
                                               TowerPlayer.MoveTo(KeyOp.W);
                                           if (Input.GetKeyDown(KeyCode.S))
                                               TowerPlayer.MoveTo(KeyOp.S);
                                           if (Input.GetKeyDown(KeyCode.A))
                                               TowerPlayer.MoveTo(KeyOp.A);
                                           if (Input.GetKeyDown(KeyCode.D))
                                               TowerPlayer.MoveTo(KeyOp.D);
                                       });
        }

        public void DestroyTowerPlayer()
        {
            Destroy(TowerPlayer.gameObject);
            _disposable1?.Dispose();
        }

        [BoxGroup("魔塔")]
        [Button]
        public void SetRole(RoadSign roadSign, BaseObject role)
        {
            role.SetToRoad(roadSign);
        }

        [BoxGroup("魔塔")]
        [Button]
        public void SetPlayer(RoadSign roadSign, TowerPlayer towerPlayer)
        {
            SetRole(roadSign, towerPlayer);
        }

        [BoxGroup("魔塔")]
        [Button]
        public void EnterTowerEffect(bool isForward, Action complete = null)
        {
            if (isForward)
            {
                SetViewPivotToCenter(false);
                StartCoroutine(_levelPart1.EnterTowerEffect(true));
                StartCoroutine(_levelPart2.EnterTowerEffect(true, () =>
                {
                    complete?.Invoke();
                }));
            }
            else
            {
                StartCoroutine(_levelPart1.EnterTowerEffect(false));
                StartCoroutine(_levelPart2.EnterTowerEffect(false, () =>
                {
                    SetViewPivotToCenter(true);
                    complete?.Invoke();
                }));
            }
        }

        private void SetViewPivotToCenter(bool reback)
        {
            if (reback == false)
            {
                _v1Apo = Views[0].anchoredPosition;
                _v1Po = Views[0].pivot;
                _v2Apo = Views[1].anchoredPosition;
                _v2Po = Views[1].pivot;

                UIDrag d1 = _drags[0];
                UIDrag d2 = _drags[1];

                int count = d1.UiExpand.XCount;
                if (Mathf.Abs(d2.XIndex.Value - d1.XIndex.Value) == count)
                {
                    //横向
                    if (d2.XIndex.Value > d1.XIndex.Value)
                    {
                        //d2在右
                        if (d2.YIndex.Value > d1.YIndex.Value)
                        {
                            //d2在上
                            float half = (d1.YIndex.Value + count - d2.YIndex.Value) / 2f;
                            Views[0].pivot = new Vector2(1, (half + d2.YIndex.Value - d1.YIndex.Value) / count);
                            Views[1].pivot = new Vector2(0, half / count);
                        }
                        else
                        {
                            float half = (d2.YIndex.Value + count - d1.YIndex.Value) / 2f;
                            Views[0].pivot = new Vector2(1, half / count);
                            Views[1].pivot = new Vector2(0, (d1.YIndex.Value - d2.YIndex.Value + half) / count);
                        }
                    }
                    else
                    {
                        //d2在左
                        if (d2.YIndex.Value > d1.YIndex.Value)
                        {
                            //d2在上
                            float half = (d1.YIndex.Value + count - d2.YIndex.Value) / 2f;
                            Views[0].pivot = new Vector2(0, (half + d2.YIndex.Value - d1.YIndex.Value) / count);
                            Views[1].pivot = new Vector2(1, half / count);
                        }
                        else
                        {
                            float half = (d2.YIndex.Value + count - d1.YIndex.Value) / 2f;
                            Views[0].pivot = new Vector2(0, half / count);
                            Views[1].pivot = new Vector2(1, (d1.YIndex.Value - d2.YIndex.Value + half) / count);
                        }
                    }
                }
                else
                {
                    //竖向
                    if (d2.XIndex.Value > d1.XIndex.Value)
                    {
                        float half = (d1.XIndex.Value + count - d2.XIndex.Value) / 2f;

                        //d2在右
                        if (d2.YIndex.Value > d1.YIndex.Value)
                        {
                            //d2在上
                            Views[0].pivot = new Vector2((d2.XIndex.Value - d1.XIndex.Value + half) / count, 1);
                            Views[1].pivot = new Vector2(half / count, 0);
                        }
                        else
                        {
                            Views[0].pivot = new Vector2((d2.XIndex.Value - d1.XIndex.Value + half) / count, 0);
                            Views[1].pivot = new Vector2(half / count, 1);
                        }
                    }
                    else
                    {
                        float half = (d2.XIndex.Value + count - d1.XIndex.Value) / 2f;

                        //d2在左
                        if (d2.YIndex.Value > d1.YIndex.Value)
                        {
                            //d2在上
                            Views[0].pivot = new Vector2(half / count, 1);
                            Views[1].pivot = new Vector2((d1.XIndex.Value - d2.XIndex.Value + half) / count, 0);
                        }
                        else
                        {
                            Views[0].pivot = new Vector2(half / count, 0);
                            Views[1].pivot = new Vector2((d1.XIndex.Value - d2.XIndex.Value + half) / count, 1);
                        }
                    }
                }

                Vector2 p1 = Views[0].pivot;
                Vector2 s1 = Views[0].sizeDelta;
                Views[0].anchoredPosition += new Vector2(p1.x * s1.x, p1.y * s1.y);
                Vector2 p2 = Views[1].pivot;
                Vector2 s2 = Views[1].sizeDelta;
                Views[1].anchoredPosition += new Vector2(p2.x * s2.x, p2.y * s2.y);
            }
            else
            {
                Views[0].pivot = _v1Po;
                Views[0].anchoredPosition = _v1Apo;
                Views[1].pivot = _v2Po;
                Views[1].anchoredPosition = _v2Apo;
            }
        }

        [BoxGroup("魔塔")]
        [Button]
        public List<Monster> GetAllMonstersInLevel()
        {
            return _allRoadSigns.Where(sign =>sign.TowerObjectInstance!=null&& sign.TowerObjectInstance is Monster)
                                .Select(sign => sign.TowerObjectInstance as Monster)
                                .GroupBy(monster => monster.Name)
                                .Select(monsters => monsters.First())
                                .ToList();
        }
    }
}