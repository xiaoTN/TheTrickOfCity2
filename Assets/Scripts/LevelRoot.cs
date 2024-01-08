using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Common.Json;

using DG.Tweening;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Trick
{
    [ExecuteInEditMode]
    public partial class LevelRoot : MonoBehaviour
    {
        [ReadOnly]
        [Required]
        [ChildGameObjectsOnly]
        [LabelText("出生点")]
        public RoadSign StartRoadSign;

        [ReadOnly]
        [Required]
        [ChildGameObjectsOnly]
        [LabelText("终点")]
        public RoadSign EndRoadSign;

        [LabelText("第几关")]
        public int Index = -1;

        [NonSerialized]
        [ReadOnly]
        public int Length;

        [LabelText("连接的格子(MatchInfos)")]
        [ReadOnly]
        public List<MatchInfo> MatchInfos;

        private List<RoadSign> _allRoadSigns = new List<RoadSign>();

        public List<RoadSign> AllRoadSigns
        {
            get { return _allRoadSigns; }
        }

        // private string         _jsonPath;

        [Required]
        [ReadOnly]
        public Transform Part1;

        [Required]
        [ReadOnly]
        public Transform Part2;

        public Subject<int> OnPassLevel = new Subject<int>();

        [NonSerialized]
        public FloatReactiveProperty Timer = new FloatReactiveProperty(0);

        private CompositeDisposable _disposable;

        [SerializeField]
        [ReadOnly]
        public int StartView1IndexX;
        [SerializeField]
        [ReadOnly]
        public int StartView1IndexY;
        [SerializeField]
        [ReadOnly]
        public int StartView2IndexX;
        [SerializeField]
        [ReadOnly]
        public int StartView2IndexY;

        private void Awake()
        {
#if UNITY_EDITOR
            if (Part1 == null)
                Part1 = transform.Find("Part1");
            if (Part2 == null)
                Part2 = transform.Find("Part2");
#endif
        }

    
        /// <summary>
        /// 当前关卡的移动步数
        /// </summary>
        [ReadOnly]
        [NonSerialized]
        [ShowInInspector]
        public IntReactiveProperty MoveCount = new IntReactiveProperty(0);
        private void Update()
        {
            Timer.Value += Time.deltaTime;
        }

        public void PassLevel()
        {
            _disposable?.Dispose();
        }

        [Button]
        private void Reset()
        {
            Part1 = transform.Find("Part1");
            Part2 = transform.Find("Part2");
        }

        public void Init()
        {
            if (Index == 16 && LevelManager.Instance.Config.PlayThrough > 0)
            {
                EndRoadSign.BreakAllRoad();
            }
            MoveCount.Subscribe(count =>
            {
                UIManager.Instance.StepTip.text = $"步数：{count}";
            }).AddTo(this);
            //设置画布的Mask属性
            Mask[] masks = transform.Find("Canvas").GetComponentsInChildren<Mask>();
            foreach (Mask mask in masks)
            {
                mask.showMaskGraphic = false;
            }
            //设置画布偏移
            _drags = GetComponentsInChildren<UIDrag>();
            Views = _drags.Select(drag => drag.GetComponent<RectTransform>()).ToArray();
            foreach (UIDrag uiDrag in _drags)
            {
                if (uiDrag.IsFirstTexture)
                {
                    uiDrag.AdjustUIToBound(StartView1IndexX,StartView1IndexY);
                    Length = uiDrag.GetComponent<UIExpand>().XCount;
                }
                else
                {
                    uiDrag.AdjustUIToBound(StartView2IndexX,StartView2IndexY);
                }
            }
            _disposable = new CompositeDisposable();
            Timer.Select(f => $"时间：{(int) f}").SubscribeToText(UIManager.Instance.TimeTip).AddTo(_disposable);

            //创建出生点特效
            GameObject startPoint = ResManager.Instance.LoadStartPoint();
            GameObject startEfx = Instantiate(startPoint);
            startEfx.transform.SetParent(StartRoadSign.TopPoint);
            startEfx.transform.localPosition = new Vector3(0, 0.1f, 0);
            Observable.EveryUpdate()
                      .First(_ => Player.Instance.CurRoadSign.Value.Equals(StartRoadSign) == false)
                      .Subscribe(_ =>
                      {
                          //出生点消失
                          startEfx.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InBounce).OnComplete(() =>
                          {
                              Destroy(startEfx);
                          });
                      })
                      .AddTo(this);
            
            _levelPart1 = Part1.gameObject.GetComponent<LevelPart>();
            _levelPart2 = Part2.gameObject.GetComponent<LevelPart>();
            _levelPart1.Init(true);
            _levelPart2.Init(false);
            EndRoadSign.SetEnd();
            _allRoadSigns.AddRange(_levelPart1.Roads);
            _allRoadSigns.AddRange(_levelPart2.Roads);

            _originPos = Part2.position;
            Player.Instance.Init();
            Player.Instance.TryChangeModelIm(StartRoadSign.IsDay);
            Player.Instance.CurRoadSign.SetValueAndForceNotify(StartRoadSign);
            Player.Instance.transform.position = StartRoadSign.TopPoint.position;
            Player.Instance.transform.SetParent(StartRoadSign.transform);
            UIManager.Instance.ShowDayUI(StartRoadSign.IsDay);
            
          

            UIManager.Instance.OnEndDragUI.Subscribe(unit =>
            {
                foreach (RoadSign roadSign in _lastSameRoadSign)
                {
                    roadSign.SameRoadSign = null;
                }

                _lastSameRoadSign.Clear();

                TryLinkAllSameRoad();
            }).AddTo(this);

            UIManager.Instance.OnStartDragUI.Subscribe(unit =>
            {
            }).AddTo(this);

            gameObject.ObserveEveryValueChanged(_ => Player.Instance.CurRoadSign.Value)
                      .Subscribe(sign =>
                      {
                          if (sign.Equals(EndRoadSign))
                          {
                              Observable.EveryUpdate()
                                        .First(l => Player.Instance.IsStop)
                                        .Subscribe(l =>
                                        {
                                            Debug.Log("通关，准备进入下一关");
                                            OnPassLevel?.OnNext(Index);
                                        });
                          }
                      })
                      .AddTo(this);

            //玩家移动到边界处时，不能拖动UI
            //判断边界是否有box
            Player.Instance.CurRoadSign
                  .Subscribe(road =>
                  {
                      if (road.HalfRoad)
                      {
                          UIManager.Instance.HalfRoadHavePlayer = true;
                      }
                      else
                      {
                          UIManager.Instance.HalfRoadHavePlayer = false;
                      }
                  })
                  .AddTo(this);

            Player.Instance.CurRoadSign
                  .Subscribe(sign =>
                  {
                      if (Player.Instance.EnableTransferToTower == false)
                      {
                          return;
                      }
                      if (sign.HaveMagicDoor)
                      {
                          Observable.TimerFrame(1).Subscribe(l =>
                          {
                              gameObject.UpdateAsObservable()
                                        .First(unit => Player.Instance.IsStop==true)
                                        .TakeUntil(Player.Instance.OnDisableAsObservable())
                                        .Subscribe(unit =>
                                        {
                                            EnterTower();
                                        });
                          });

                      }
                  })
                  .AddTo(this);

            UIManager.Instance.OnDragStepUI.Subscribe(drag =>
            {
                TryLinkAllSameRoad();
            }).AddTo(this);

            InitUnDo();
            InitTower();
        }


        [ReadOnly]
        [HideInEditorMode]
        [ShowInInspector]
        [NonSerialized]
        [LabelText("是否拼接了")]
        public bool IsLinkBound;

        [ReadOnly]
        [HideInEditorMode]
        [ShowInInspector]
        [NonSerialized]
        [LabelText("是否拼接了特殊边界")]
        public bool IsLinkSpecialBound;

        [Button]
        public void TryLinkAllSameRoad()
        {
            IsLinkBound = false;
            IsLinkSpecialBound = false;
            int offsetX = UIManager.Instance.IndexX2 - UIManager.Instance.IndexX1;
            int offsetY = UIManager.Instance.IndexY2 - UIManager.Instance.IndexY1;
            MatchInfo matchInfo = MatchInfos.Find(info => (info.XOffset == offsetX) && (info.YOffset == offsetY));
            if (matchInfo != null)
            {
                IsLinkBound = true;
                // 开始对接地图
                RoadSign dayRoad = matchInfo.LinkRoad1;
                RoadSign nightRoad = matchInfo.LinkRoad2;
                if (dayRoad.IsSpecialBound)
                {
                    IsLinkSpecialBound = true;
                }
                if (nightRoad.IsDay)
                {
                    RoadSign temp = nightRoad;
                    nightRoad = dayRoad;
                    dayRoad = temp;
                }

                Debug.DrawLine(dayRoad.TopPoint.position, nightRoad.TopPoint.position, Color.black, 5f);
                Vector3 offsetPos = dayRoad.TopPoint.position - nightRoad.TopPoint.position;
                Part2.position += offsetPos;

                // 对接内存
                LinkAllSameRoad();
            }
            else
            {
                BreakLinkRoad();
            }
        }

        [Button]
        private void BreakLinkRoad()
        {
            IsLinkBound = false;
            IsLinkSpecialBound = false;
            Part2.position = _originPos;
        }

        private List<RoadSign> _lastSameRoadSign = new List<RoadSign>();

        private List<RoadSign> _part1Roads=> _levelPart1.Roads;
        private List<RoadSign> _part2Roads=> _levelPart2.Roads;
        /// <summary>
        /// 将所有位置相同的Road连接起来
        /// </summary>
        private void LinkAllSameRoad()
        {
            foreach (RoadSign part1Road in _part1Roads)
            {
                foreach (RoadSign part2Road in _part2Roads)
                {
                    if (Vector3.SqrMagnitude(part1Road.transform.position - part2Road.transform.position) < 0.1f)
                    {
                        //找到相同的了
                        Debug.Log("找到相同的了", part1Road);
                        part1Road.SameRoadSign = part2Road;
                        part2Road.SameRoadSign = part1Road;
                        _lastSameRoadSign.Add(part1Road);
                        _lastSameRoadSign.Add(part2Road);
                    }
                }
            }

            _lastSameRoadSign= _lastSameRoadSign.Distinct().ToList();
        }

        private bool _askWethearSaveStartIndex = false;

        public void SaveStartIndex()
        {
            _askWethearSaveStartIndex = true;
        }
        private Vector3 _originPos;

        private bool _askWethearSaveMatchInfo = false;
        private UIDrag[] _drags;
        [NonSerialized]
        public RectTransform[] Views;
        private LevelPart _levelPart1;
        private LevelPart _levelPart2;

        public LevelPart LevelPart1
        {
            get { return _levelPart1; }
        }

        public LevelPart LevelPart2
        {
            get { return _levelPart2; }
        }

        public void DeleteMatchInfo(int x, int y)
        {
            MatchInfos.RemoveAll(info => info.XOffset == x && info.YOffset == y);
            Debug.Log($"remove :{x},{y}");
            _askWethearSaveMatchInfo = true;
        }

        private void LinkTowerRoadX(List<RoadSign> roads)
        {
            roads.Sort((x, y) => x.transform.position.z.CompareTo(y.transform.position.z));
            IEnumerable<IGrouping<int, RoadSign>> groupBy = roads.GroupBy(sign => Mathf.RoundToInt(sign.transform.position.z));
            List<List<RoadSign>> xs = groupBy.Select(roadSigns => roadSigns.Select(sign => sign).ToList()).ToList();
            xs.Sort((x, y) => x[0].transform.position.z.CompareTo(y[0].transform.position.z));
            foreach (List<RoadSign> roadSigns in xs)
            {
                int roadSignsCount = roadSigns.Count;
                if (roadSignsCount > 1)
                {
                    roadSigns.Sort((x, y) => x.transform.position.x.CompareTo(y.transform.position.x));
                    for (int i = 1; i < roadSignsCount; i++)
                    {
                        if (Mathf.Abs(Mathf.Abs(roadSigns[i].transform.position.x - roadSigns[i - 1].transform.position.x) - 1) < 0.1f)
                        {
                            //相邻
                            roadSigns[i].ARoad = roadSigns[i - 1];
                            roadSigns[i - 1].DRoad = roadSigns[i];
                            Debug.DrawLine(roadSigns[i - 1].transform.position, roadSigns[i].transform.position, Color.blue, 10f);
                        }
                    }
                }
            }
        }

        private void LinkTowerRoadZ(List<RoadSign> roads)
        {
            roads.Sort((x, y) => x.transform.position.x.CompareTo(y.transform.position.x));
            IEnumerable<IGrouping<int, RoadSign>> groupBy = roads.GroupBy(sign => Mathf.RoundToInt(sign.transform.position.x));
            List<List<RoadSign>> xs = groupBy.Select(roadSigns => roadSigns.Select(sign => sign).ToList()).ToList();
            xs.Sort((x, y) => x[0].transform.position.x.CompareTo(y[0].transform.position.x));
            foreach (List<RoadSign> roadSigns in xs)
            {
                int roadSignsCount = roadSigns.Count;
                if (roadSignsCount > 1)
                {
                    roadSigns.Sort((x, y) => x.transform.position.z.CompareTo(y.transform.position.z));
                    for (int i = 1; i < roadSignsCount; i++)
                    {
                        if (Mathf.Abs(Mathf.Abs(roadSigns[i].transform.position.z - roadSigns[i - 1].transform.position.z) - 1) < 0.1f)
                        {
                            //相邻
                            roadSigns[i].SRoad = roadSigns[i - 1];
                            roadSigns[i - 1].WRoad = roadSigns[i];
                            Debug.DrawLine(roadSigns[i - 1].transform.position, roadSigns[i].transform.position, Color.red, 10f);
                        }
                    }
                }
            }
        }

        [Button("读取保存的关卡信息")]
        public void ReloadLevel(RoadInfo roadInfo)
        {
            if(roadInfo==null) return;
            foreach (Tuple<int, int> item in roadInfo.BoxInfos)
            {
                int originIndex = item.Item1;
                int newIndex = item.Item2;
                if(originIndex==newIndex) continue;
                RoadSign roadSign = _allRoadSigns.Find(sign => sign.Index == originIndex);
                RoadSign newRoad = _allRoadSigns.Find(sign => sign.Index == newIndex);
                roadSign.Box.SetToRoadAndSetPos(newRoad);
            }

            Player.Instance.SetToRoadAndSetPos(StartRoadSign);
        }

        [Button]
        public void SaveLevel()
        {
            RoadInfo roadInfo = LevelManager.Instance.SaveInfo.LevelInfos[Index];
            roadInfo.X1 = UIManager.Instance.IndexX1;
            roadInfo.Y1 = UIManager.Instance.IndexY1;
            roadInfo.X2 = UIManager.Instance.IndexX2;
            roadInfo.Y2 = UIManager.Instance.IndexY2;
            roadInfo.BoxInfos=new List<Tuple<int, int>>();
            foreach (RoadSign roadSign in _allRoadSigns)
            {
                if (roadSign.HaveBox)
                {
                    roadInfo.BoxInfos.Add(new Tuple<int, int>(roadSign.Box.OriginRoad.Index, roadSign.Index));
                }
            }
            
            LevelManager.Instance.SaveGame();
        }
    }

    /// <summary>
    /// 匹配的信息
    /// </summary>
    [Serializable]
    public class MatchInfo
    {
        [ReadOnly]
        public int XOffset; // 第二张图 - 第一张图

        [ReadOnly]
        public int YOffset;

        [ReadOnly]
        public RoadSign LinkRoad1;

        [ReadOnly]
        public RoadSign LinkRoad2;
    }

    public enum KeyOp
    {
        W,
        A,
        S,
        D
    }
}