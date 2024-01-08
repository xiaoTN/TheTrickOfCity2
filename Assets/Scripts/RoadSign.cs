using System;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using Sirenix.OdinInspector;
using Trick.Tower;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Trick
{
    /// <summary>
    /// 路标
    /// </summary>
    public partial class RoadSign : MonoBehaviour
    {
        [ShowInInspector]
        [ReadOnly]
        [NonSerialized]
        [HideInEditorMode]
        public BoolReactiveProperty HavePlayer=new BoolReactiveProperty();
        [BoxGroup("可修改的")]
        [DisableInPlayMode]
        [LabelText("行走路径(Links)")]
        [ShowInInspector]
        public List<RoadSign> Links = new List<RoadSign>();

        [BoxGroup("可修改的")]
        [DisableInPlayMode]
        [LabelText("推箱子路径(JumpLinks)")]
        [ShowInInspector]
        public List<RoadSign> JumpLinks = new List<RoadSign>();

        [BoxGroup("可修改的")]
        [DisableInPlayMode]
        [LabelText("推箱子失败路径(JumpFailedLinks)")]
        [ShowInInspector]
        public List<RoadSign> JumpFailedLinks = new List<RoadSign>();

        [ShowInInspector]
        [NonSerialized]
        [ReadOnly]
        public RoadSign SameRoadSign;

        [HideInInspector]
        [Required(InfoMessageType.Error, ErrorMessage = "缺少Top节点")]
        public Transform TopPoint;

        [BoxGroup("可修改的")]
        [LabelText("楼梯")]
        [InlineButton("ChangeToOtherRoad", "切换道路类型")]
        [DisableInPlayMode]
        public bool Stair;

        [BoxGroup("可修改的")]
        [LabelText("箱子")]
        [DisableInPlayMode]
        [GUIColor(0.81f, 0.21f, 0.14f)]
        [HideIf("Stair")]
        [HideIf("HaveBlock")]
        [HideIf("HaveDoor")]
        [InlineButton("DestroyBox", "销毁箱子")]
        [InlineButton("CreateBoxEditor", "创建箱子")]
        public bool HaveBox;
        
        public bool HaveBoxConsiderBound
        {
            get
            {
                if (HaveBox)
                {
                    return true;
                }
                if (IsBound)
                {
                    if (SameRoadSign != null)
                    {
                        return SameRoadSign.HaveBox;
                    }
                }

                return false;
            }
        }

        public void AddTopBox(Box box)
        {
            if (box.CurRoad.Value != null)
            {
                box.CurRoad.Value.HaveBox = false;
                box.CurRoad.Value.Box = null;
            }

            if (box.NearestBottomBox != null)
            {
                box.NearestBottomBox.NearestTopBox = null;
                box.NearestBottomBox = null;
            }


            if(HaveBox==false)
            {
                box.CurRoad.Value = this;
            }
            else
            {
                box.CurRoad.Value = null;
            }

            if (HaveBox == false)
            {
                HaveBox = true;
                Box = box;
                box.transform.SetParent(TopPoint);
            }
            else
            {
                Box.AddTopBox(box);
            }
        }

        [BoxGroup("可修改的")]
        [ShowIf("HaveBox")]
        [OnValueChanged("OnIsDayBoxChange")]
        [LabelText("是否为白天的箱子")]
        [ShowInInspector]
        [HideInPlayMode]
        private bool _isDayBox;

       

        [BoxGroup("可修改的")]
        [DisableInPlayMode]
        [LabelText("边界")]
        [HideIf("Stair")]
        public bool IsBound;


        [BoxGroup("可修改的")]
        [HideInEditorMode]
        [ReadOnly]
        [LabelText("边界是否连接")]
        [ShowIf("IsBound")]
        [ShowPropertyResolver]
        [ShowInInspector]
        public bool IsLinkBound
        {
            get { return SameRoadSign != null; }
        }

        [BoxGroup("可修改的")]
        [DisableInPlayMode]
        [InfoBox("处于特殊边界的位置，不受是否存在箱子的影响")]
        [LabelText("特殊边界")]
        [ShowIf("IsBound")]
        [HideIf("Stair")]
        public bool IsSpecialBound;

        /// <summary>
        /// 是否为半个平面
        /// </summary>
        [HideInPlayMode]
        [ShowInInspector]
        [ShowPropertyResolver]
        public bool HalfRoad
        {
            get { return IsBound && (IsSpecialBound == false); }
        }

        /// <summary>
        /// 是否算一个完整的平面
        /// </summary>
        public bool IsFullRoad
        {
            get
            {
                if (HalfRoad)
                {
                    if (IsLinkBound)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }

                return false;
            }
        }

        [BoxGroup("可修改的")]
        [DisableInPlayMode]
        [LabelText("障碍物")]
        [GUIColor(0.29f, 0.63f, 1f)]
        [HideIf("Stair")]
        [HideIf("HaveBox")]
        [HideIf("HaveKey")]
        [HideIf("HaveDoor")]
        [InlineButton("DestroyBlock", "销毁障碍物")]
        [InlineButton("CreateBlock", "创建障碍物")]
        public bool HaveBlock;

        [ReadOnly]
        [LabelText("障碍物实例")]
        [HideIf("Stair")]
        [ShowIf("HaveBlock")]
        public GameObject BlockGo;

      

      


        [ReadOnly]
        [LabelText("箱子实例")]
        [HideIf("Stair")]
        [ShowIf("HaveBox")]
        public Box Box;

        /// <summary>
        /// 最上层的箱子
        /// </summary>
        public Box TopsideBox
        {
            get
            {
                Box boxTopBox = Box;
                while (true)
                {
                    Box tempBox = boxTopBox;
                    boxTopBox = boxTopBox.NearestTopBox;
                    if (boxTopBox == null)
                    {
                        return tempBox;
                    }
                }
            }
        }

        public Vector3 TopestPosition
        {
            get
            {
                Vector3 p3;
                if (HaveBox)
                {
                    p3 = Box.TopestPosition;
                }
                else
                {
                    p3= TopPoint.position;
                }

                return p3;
            }
        }

        [HideInInspector]
        [SerializeField]
        private int _index;
        
        public int Index
        {
            get => _index;
        }

        public void SetStandardName()
        {
            SetStandardName(_index);
        }
        public void SetStandardName(int index)
        {
            _index = index;
            StringBuilder sb = new StringBuilder();
            sb.Append($"{index}\t");
            if (IsDay)
            {
                sb.Append("光 ");
            }
            else
            {
                sb.Append("暗 ");
            }

            if (Stair)
            {
                sb.Append("楼梯 ");
            }

            if (IsBound)
            {
                sb.Append("边界 ");
            }

            if (HaveBlock)
            {
                sb.Append("障碍物 ");
            }

            if (HaveBox)
            {
                string s = _isDayBox ? "光" : "暗";
                sb.Append($"箱子({s}) ");
            }

            if (HaveKey)
            {
                sb.Append("机关 ");
            }

            if (HaveDoor)
            {
                sb.Append("门 ");
            }

            gameObject.name = sb.ToString();
        }

        public void SetName(string objName)
        {
            gameObject.name = $"{_index} {objName}";
        }

        private MeshRenderer _mr;
        public void Init()
        {
            _mr = GetComponent<MeshRenderer>();
            if (LevelManager.Instance.CycleIndex > 0)
            {
                if (TowerObjectPrefab != null)
                {
                    BaseObject baseObject = TowerObjectPrefab.GetComponent<BaseObject>();
                    if (baseObject is MagicDoor)
                    {
                        //创建魔塔传送门
                        CreateTowerMagicDoor();
                    }
                }
            }
            
            if(IsBound)
            {
                _mat = GetComponent<MeshRenderer>().material;
                VIntensity.Subscribe(v =>
                {
                    _mat.SetColor("_EmissionColor", new Color(v, v, v));
                }).AddTo(this);
                Observable.EveryUpdate()
                          .Subscribe(l =>
                          {
                              OnHighLightUpdate();
                          })
                          .AddTo(this);
                UIManager.Instance.OnStartDragUI.Subscribe(unit =>
                {
                    Flicker(true);
                }).AddTo(this);
                UIManager.Instance.OnEndDragUI.Subscribe(unit =>
                {
                    Flicker(false);
                }).AddTo(this);
                
            }
            
            if (Stair && IsBound)
            {
                Debug.LogError("楼梯不许在边界");
            }

            if (HaveBox)
            {
                Box.Init();
            }
            
            if (HaveKey)
            {
                Key.Init(this);
            }

            Door door = GetComponentInChildren<Door>();
            HaveDoor = door != null;
            if (HaveDoor)
            {
                door.Init();
                door.ObserveEveryValueChanged(_ => door.IsOpen)
                    .Subscribe(b => DoorIsOpen = b)
                    .AddTo(this);
            }

            gameObject.ObserveEveryValueChanged(_ => HaveBox)
                      .Where(_ => Box != null)
                      .Subscribe(b =>
                      {
                          bool high = !(HaveBox && HaveKey);
                          if (HaveBox)
                          {
                              Box.Highlight(high);
                          }
                      })
                      .AddTo(this);

            InitTxt();
            InitTower();
            InitLayer();
            InitDialog();
        }

        private bool _isEnd;
        private GameObject _endGo;

        public void SetEnd()
        {
            _isEnd = true;
            _endGo = TopPoint.GetChild(0).gameObject;
            if(LevelManager.Instance.CurIndex==16)
            {
                if (LevelManager.Instance.Config.PlayThrough > 0)
                {
                    _endGo.SetActive(false);
                    _endGo = TopPoint.GetChild(1).gameObject;
                    _endGo.SetActive(true);
                }
            }
            InitLayer();
        }

        private void InitLayer()
        {
            int layerIndex = LayerMask.NameToLayer(IsDay ? "Part1" : "Part2");
            if (HaveKey)
            {
                Key.gameObject.SetLayers(layerIndex);
            }

            if (HaveBox)
            {
                Box.gameObject.SetLayers(layerIndex);
            }

            if (HaveBlock)
            {
                BlockGo.SetLayers(layerIndex);
            }

            if (HaveDoor)
            {
                Door.gameObject.SetLayers(layerIndex);
            }

            if (_isEnd)
            {
                _endGo.SetLayers(layerIndex);
            }
        }


        [HideInEditorMode]
        [Button]
        public void CreateBoxRuntimeRequest(bool isDay)
        {
            CreateBoxRuntimeRequestAsObservable(isDay).Subscribe().AddTo(this);
        }

        public IObservable<Box> CreateBoxRuntimeRequestAsObservable(bool isDay)
        {
            return Observable.ReturnUnit()
                             .DelayFrame(1) //确保角色先动
                             .Select(unit =>
                             {
                                 if (HavePlayer.Value == false)
                                 {
                                     CreateBoxRuntime(isDay);
                                     return Observable.Return(Box);
                                 }
                                 else
                                 {
                                     //等待玩家离开
                                     return Observable.EveryUpdate()
                                                      .First(l => HavePlayer.Value == false)
                                                      .Select(l =>
                                                      {
                                                          CreateBoxRuntime(isDay);
                                                          return Box;
                                                      });
                                 }
                             })
                             .Switch();
        }
        
        private void CreateBoxRuntime(bool isDay)
        {
            HaveBox = true;
            if (Box != null)
            {
                return;
            }

            GameObject boxPrefab = Resources.Load<GameObject>("box");
            GameObject go = GameObject.Instantiate(boxPrefab);
            go.transform.SetParent(TopPoint);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            Box = go.GetComponent<Box>();
            _isDayBox = isDay;
            Box.Init();
            Box.IsDay = isDay;
            Box.ChangeType(_isDayBox);

            //添加箱子缩放动画
            Box.PlayScaleAnimation();

            RoadSign playerCurRoad = Player.Instance.CurRoadSign.Value;
            if (playerCurRoad != null)
            {
                playerCurRoad.ForceUpdateUI();
            }
        }

     

        [BoxGroup("可修改的")]
        [DisableInPlayMode]
        [HideIf("Stair")]
        [HideIf("HaveBlock")]
        [HideIf("HaveDoor")]
        [LabelText("机关")]
        [GUIColor(1f, 0.9f, 0.18f)]
        [InlineButton("DestroyKey", "销毁机关")]
        [InlineButton("CreateKey", "创建机关")]
        public bool HaveKey;

        [ReadOnly]
        [HideIf("Stair")]
        [ShowIf("HaveKey")]
        public Key Key;

   

        [ShowIf("HaveDoor")]
        [ShowInInspector]
        [ReadOnly]
        public Door Door;

        [BoxGroup("可修改的")]
        [GUIColor(0.6f, 0.43f, 0.33f)]
        [InlineButton("DestoryDoor", "销毁门")]
        [InlineButton("CreateDoor", "创建门")]
        [LabelText("门")]
        [HideIf("Stair")]
        [HideIf("HaveBlock")]
        [HideIf("HaveBox")]
        [HideIf("HaveKey")]
        public bool HaveDoor;

       

        [HideInEditorMode]
        [NonSerialized]
        [ShowInInspector]
        [ReadOnly]
        public bool DoorIsOpen;


        [LabelText("是否属于白天")]
        [ShowInInspector]
        [ReadOnly]
        public bool IsDay;

        /// <summary>
        /// 找到下一个相邻的Road
        /// 连线的Road
        /// </summary>
        /// <param name="keyOp"></param>
        /// <returns></returns>
        [Button]
        public RoadSign FindNearestRoadIgnoreBound(KeyOp keyOp)
        {
            RoadSign roadSign = null;
            roadSign = Links.Find(GetRoadByKeyOp(keyOp));
            return roadSign;
        }

        [Button]
        public RoadSign FindLastRoadOnPartConsiderCanMove(KeyOp keyOp)
        {
            RoadSign nextRoad = this;
            while (true)
            {
                RoadSign tempRoad = nextRoad;
                nextRoad = nextRoad.Links.Find(nextRoad.GetRoadByKeyOp(keyOp));
                if (nextRoad == null)
                {
                    return tempRoad;
                }
                
                if (nextRoad.HalfRoad)
                {
                    if (nextRoad.IsLinkBound)
                    {
                        return nextRoad;
                    }
                    return tempRoad;
                }
            }
        }

        [Button]
        public RoadSign FindLastRoadInLineConsiderBlock(KeyOp keyOp)
        {
            RoadSign nextRoad = this;
            while (true)
            {
                RoadSign tempRoad = nextRoad;
                nextRoad = nextRoad.Links.Find(nextRoad.GetRoadByKeyOp(keyOp));
                if (nextRoad == null)
                {
                    return tempRoad;
                }

                if (nextRoad.Stair
                    || nextRoad.HaveBox)
                {
                    return tempRoad;
                }

                if (nextRoad.HalfRoad)
                {
                    if (nextRoad.IsLinkBound)
                    {
                        return nextRoad;
                    }

                    return tempRoad;
                }
            }
        }



        public RoadSign FindNearestFailedRoad(KeyOp keyOp)
        {
            if (IsLinkBound)
            {
                return SameRoadSign.JumpFailedLinks.Find(GetRoadByKeyOp(keyOp));
            }
            RoadSign roadSign = JumpFailedLinks.Find(GetRoadByKeyOp(keyOp));
            return roadSign;
        }

        [Button]
        public RoadSign FindNearestRoadConsiderBounds(KeyOp keyOp)
        {
            RoadSign findNearestRoadIgnoreBound = FindNearestRoadIgnoreBound(keyOp);
            if (findNearestRoadIgnoreBound != null)
            {
                if (findNearestRoadIgnoreBound.HalfRoad)
                {
                    if (findNearestRoadIgnoreBound.SameRoadSign != null)
                    {
                        return findNearestRoadIgnoreBound.SameRoadSign;
                    }

                }
            }
            else
            {
                if (IsLinkBound)
                {
                    return SameRoadSign.FindNearestRoadIgnoreBound(keyOp);
                }
            }
            return findNearestRoadIgnoreBound;
        }

        //筛选条件
        private Predicate<RoadSign> GetRoadByKeyOp(KeyOp keyOp)
        {
            Predicate<RoadSign> predicate;
            Vector3 dirByKeyOp = keyOp.GetDirByKeyOp();
            predicate = sign =>
            {
                if (sign != null)
                {
                    Vector3 dir = (sign.GetSameVisiablePositionByHeight(sign.TopPoint.position, TopPoint.position.y) - TopPoint.position);
                    dir.y = 0;
                    dir.Normalize();
                    if (Vector3.Angle(dir, dirByKeyOp) < 45)
                    {
                        return true;
                    }
                }

                return false;
            };
            return predicate;
        }

        // TODO 判断仍然存在部分漏洞，暂时只能通过Override纠正
        [Button]
        public RoadSign FindNextRoad(KeyOp keyOp)
        {
            RoadSign roadSign = FindNearestRoadIgnoreBound(keyOp);
            if (roadSign == null && SameRoadSign != null)
            {
                roadSign = SameRoadSign.Links.Find(GetRoadByKeyOp(keyOp));
            }

            return roadSign;
        }

        [Button]
        public RoadSign FindNextRoadCanMove(KeyOp keyOp)
        {
            RoadSign findNextRoad = FindNextRoad(keyOp);
            if (findNextRoad == null)
            {
                return null;
            }

            if (findNextRoad.TopBoxCount >= 2)
            {
                return null;
            }

            if (findNextRoad.HaveBox|| (findNextRoad.IsLinkBound && findNextRoad.SameRoadSign.HaveBox))
            {
                if (findNextRoad.IsLinkBound && findNextRoad.SameRoadSign.HaveBox)
                {
                    findNextRoad = findNextRoad.SameRoadSign;
                }
                RoadSign findNextRoad2 = findNextRoad.FindNextRoad(keyOp);

                if (findNextRoad2 != null)
                {
                    if (findNextRoad2.HaveBox
                        || findNextRoad2.Stair
                        || findNextRoad2.HaveBlock)
                    {
                        return null;
                    }

                    if (findNextRoad2.IsBound
                        && (findNextRoad2.IsSpecialBound == false)
                        &&!findNextRoad2.IsLinkBound)
                    {
                        return null;
                    }
                }
                else
                {
                    findNextRoad2 = findNextRoad.FindNearestFailedRoad(keyOp);
                    if (findNextRoad2!=null)
                    {
                        return null;
                    }
                }
            }
            else
            {
                if (findNextRoad.IsBound
                    && (findNextRoad.IsSpecialBound == false)
                    && !findNextRoad.IsLinkBound)
                {
                    return null;
                }
            }

            return findNextRoad;
        }

        public bool CanFindNextRoadCanMove(KeyOp keyOp)
        {
            RoadSign findNextRoad = FindNextRoad(keyOp);
            if (findNextRoad == null)
            {
                return false;
            }

            if (findNextRoad.TopBoxCount >= 2)
            {
                return false;
            }

            if (findNextRoad.HaveBox)
            {
                RoadSign findNextRoad2 = findNextRoad.FindNextRoad(keyOp);

                if (findNextRoad2 != null)
                {
                    if (findNextRoad2.HaveBox
                    || findNextRoad2.Stair
                    || findNextRoad2.HaveBlock)
                    {
                        return false;
                    }

                    if (findNextRoad2.IsBound
                        && (findNextRoad2.IsSpecialBound == false)
                        &&!findNextRoad2.IsLinkBound)
                    {
                        return false;
                    }
                }
                else
                {
                    findNextRoad2 = findNextRoad.FindNearestFailedRoad(keyOp);
                    if (findNextRoad2!=null)
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (findNextRoad.IsBound
                    && (findNextRoad.IsSpecialBound == false)
                    && !findNextRoad.IsLinkBound)
                {
                    return false;
                }
            }

            return true;
        }

        [LabelText("箱子数量")]
        [ShowIf("HaveBox")]
        [HideInEditorMode]
        [ShowInInspector]
        public int TopBoxCount
        {
            get
            {
                if (HaveBox)
                {
                    int boxTopBoxCount = Box.TopBoxCount + 1;
                    if (boxTopBoxCount > 2)
                    {
                        Debug.LogError("箱子数量大于2,逻辑出现错误", this);
                    }
                    return boxTopBoxCount;
                }

                return 0;
            }
        }
        
        public int GetPushBoxStateIndex(KeyOp keyOp,out RoadSign roadSign)
        {
            roadSign = FindNearestRoadIgnoreBound(keyOp);
            if (roadSign)
            {
                return 0;
            }

            Debug.Log("无法平推箱子，尝试将箱子推到下面");
            roadSign= FindNextJumpBoxRoad(keyOp);
            if (roadSign)
            {
                Debug.Log("可以将箱子推到下面", roadSign);
                return 1;
            }

            roadSign = FindNextJumpBoxFailedRoad(keyOp);
            Debug.Log("尝试查找推箱子失败路径");
            if (roadSign)
            {
                Debug.Log("找到推箱子失败路径，禁止推箱子", roadSign);
                return 2;
            }

            Debug.Log("无法找到推箱子失败路径，将箱子推到虚空下");
            return 3;
        }
        

        public RoadSign FindNextJumpBoxRoad(KeyOp keyOp)
        {
            RoadSign roadSign = JumpLinks.Find(GetRoadByKeyOp(keyOp));
            if (roadSign == null && SameRoadSign != null)
                roadSign = SameRoadSign.JumpLinks.Find(GetRoadByKeyOp(keyOp));
            return roadSign;
        }

        public RoadSign FindNextJumpBoxFailedRoad(KeyOp keyOp)
        {
            RoadSign roadSign = JumpFailedLinks.Find(GetRoadByKeyOp(keyOp));
            if (roadSign == null && SameRoadSign != null)
                roadSign = SameRoadSign.JumpFailedLinks.Find(GetRoadByKeyOp(keyOp));
            return roadSign;
        }



        [Button("断开当前道路与其他道路的连接", ButtonSizes.Medium)]
        public void BreakAllRoad()
        {
            foreach (RoadSign roadSign in Links)
            {
                roadSign.Links.Remove(this);
            }

            Links.Clear();
        }



        public void LinkRoad(RoadSign sign)
        {
            if (Links.Contains(sign) == false)
            {
                Links.Add(sign);
            }

            if (sign.Links.Contains(this) == false)
            {
                sign.Links.Add(this);
            }
        }



        // 没有经过充足的测试，所以可能存在部分特殊情况下判断失败的问题，但目前使用没有问题
        [Button]
        public bool IsSameVisiableRoad(RoadSign roadSign)
        {
            if (roadSign.Stair)
                return false;
            return IsSameVisiableRoad(roadSign.transform.position);
        }

        private bool IsSameVisiableRoad(Vector3 position)
        {
            Vector3 targetPos = GetSameVisiablePositionByHeight(transform.position, position.y);
            return targetPos == position;
        }

        public Vector3 GetSameVisiablePositionByHeight(Vector3 position, float y)
        {
            int offsetCount = Mathf.RoundToInt((position.y - y) / 1.41f);
            Vector3 newPos = new Vector3(position.x - offsetCount, y, position.z + offsetCount);
            Debug.DrawLine(transform.position, newPos, Color.black, 5f);
            return newPos;
        }

        /// <summary>
        /// 箱子是否能推到下面
        /// </summary>
        /// <param name="roadSign"></param>
        /// <returns></returns>
        public void CheckNearVisiableBottomRoad(RoadSign roadSign)
        {
            if (roadSign.Stair)
                return;

            //右下
            Vector3 xRightBottomPos = transform.position + new Vector3(1, -1.41f, 0);

            //左下
            Vector3 zLeftBottomPos = transform.position + new Vector3(0, -1.41f, -1);

            //能推到下面
            if (roadSign.IsSameVisiableRoad(xRightBottomPos))
            {
                JumpLinks.Add(roadSign);
            }
            else if (roadSign.IsSameVisiableRoad(zLeftBottomPos))
            {
                JumpLinks.Add(roadSign);
            }
        }

        public void ShowNPC(bool isShow)
        {
            if (HaveBox)
            {
                Box.gameObject.SetActive(isShow);
            }

            if (HaveBlock)
            {
                BlockGo.SetActive(isShow);
            }

            if (HaveKey)
            {
                Key.ShowNPC(isShow);
            }

            if (HaveDoor)
            {
                Door.gameObject.SetActive(isShow);
            }

            if (_isEnd)
            {
                _endGo.SetActive(isShow);
            }

            if (HavePlayer.Value)
            {
                Player.Instance.gameObject.SetActive(isShow);
                Observable.TimerFrame(1).Subscribe(l =>
                {
                    ShowAllAroundText(isShow);
                }).AddTo(this);
            }

            if (_magicDoorGo)
            {
                _magicDoorGo.SetActive(isShow);
            }
        }

        public void ShowRoadRenderer(bool isShow)
        {
            _mr.enabled = isShow;
        }
    }
}