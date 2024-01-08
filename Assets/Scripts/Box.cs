using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UniRx;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Trick
{
    public partial class Box : MonoBehaviour
    {
        [LabelText("光面")]
        public bool IsDay;

        [ShowInInspector]
        [ReadOnly]
        [NonSerialized]
        public RoadSign OriginRoad;

        [ShowInInspector]
        [ReadOnly]
        [NonSerialized]
        public ReactiveProperty<RoadSign> CurRoad=new ReactiveProperty<RoadSign>();

        public GameObject SunEfx;

        public GameObject NoonEfx;

        public Material SunMat;
        public Material NoonMat;

        public Transform TopPoint;
        
        [ReadOnly]
        [SerializeField]
        public Box NearestTopBox;
        [ReadOnly]
        [SerializeField]
        public Box NearestBottomBox;

        [AssetsOnly]
        public GameObject BrithEfxPrefab;

        // todo init
        public void Init()
        {
            OriginRoad = GetComponentInParent<RoadSign>();
            transform.localScale=Vector3.one*0.98f;
            CurRoad.Value = OriginRoad;
            CurRoad.Subscribe(sign =>
            {
                if(sign==null) return;
                if (sign.HalfRoad)
                {
                    UIManager.Instance.HalfRoadHaveBox = true;
                }
                else
                {
                    UIManager.Instance.HalfRoadHaveBox = false;
                }
            }).AddTo(this);
            if (TopPoint is null)
            {
                TopPoint = transform.Find("TopPoint");
            }

            ChangeType(IsDay);
        }

        public void SetMaterialEditor(bool isDay)
        {
#if UNITY_EDITOR
            ChangeType(isDay);
            EditorUtility.SetDirty(this);
#endif
        }

        public void Highlight(bool isHigh)
        {
            if (isHigh)
            {
                SunEfx.SetActive(IsDay);
                NoonEfx.SetActive(!IsDay);
            }
            else
            {
                SunEfx.SetActive(false);
                NoonEfx.SetActive(false);
            }
        }

        public void ChangeType(bool isDay)
        {
            SunEfx.SetActive(isDay);
            NoonEfx.SetActive(!isDay);
            GetComponent<MeshRenderer>().material = isDay ? SunMat : NoonMat;
        }

        public Box AddTopBox(Box box)
        {
            if (NearestTopBox != null)
            {
                return NearestTopBox.AddTopBox(box);
            }
            else
            {
                //箱子堆叠会变暗
                box.Highlight(false);
                Highlight(false);
                NearestTopBox = box;
                box.NearestBottomBox = this;
                box.transform.SetParent(TopPoint);
                return this;
            }
        }


        /// <summary>
        /// 将箱子的数据转移到指定的RoadSign
        /// </summary>
        /// <param name="targetRoad"></param>
        public void SetToRoad(RoadSign targetRoad)
        {
            if (targetRoad==null)
            {
                transform.SetParent(null);
                CurRoad.Value.Box = null;
                CurRoad.Value.HaveBox = false;
                CurRoad.Value = null;
                Debug.Log("检测到箱子没有所属Road，如果箱子目前处于虚空状态就没问题");
                return;
            }

            targetRoad.AddTopBox(this);
        }

        [Button("设置箱子的位置和数据")]
        public void SetToRoadAndSetPos(RoadSign targetRoad)
        {
            if (targetRoad == null)
            {
                Debug.LogError("Road是null");
                return;
            }

            transform.position = targetRoad.TopestPosition;
            SetToRoad(targetRoad);
        }

        public void MoveToOriginRoad()
        {
            SetToRoadAndSetPos(OriginRoad);
            PlayScaleAnimation();
        }

        public void TryMoveToOriginRoadConsiderPlayer()
        {
            Observable.EveryUpdate()
                      .FirstOrDefault(l => OriginRoad.HavePlayer.Value == false)
                      .Subscribe(l =>
                      {
                          MoveToOriginRoad();
                      })
                      .AddTo(this);
        }

        public void PlayScaleAnimation()
        {
            transform.localScale=Vector3.zero;
            DOTween.To(() => 0, x =>
            {
                transform.localScale = Vector3.one * x;
            }, 1f, 0.15f);
            //创建特效
            if (CurRoad.Value == null)
            {
                CurRoad.Value = GetComponentInParent<RoadSign>();
                Debug.LogWarning("检测到手动创建了Box",CurRoad.Value);
            }
            GameObject efxGo = GameObject.Instantiate(BrithEfxPrefab, CurRoad.Value.TopPoint);
            efxGo.transform.localPosition=Vector3.zero;
            efxGo.transform.localRotation=Quaternion.identity;
            Destroy(efxGo,5);
        }

        public int TopBoxCount
        {
            get
            {
                if (NearestTopBox == null)
                {
                    return 0;
                }
                return NearestTopBox.TopBoxCount+1;
            }
        }

        public Vector3 TopestPosition
        {
            get
            {
                if (NearestTopBox == null)
                {
                    return TopPoint.position;
                }

                return NearestTopBox.TopestPosition;
            }
        }

        [Button]
        public bool PushTo(KeyOp keyOp)
        {
            RoadSign tempRoad = CurRoad.Value;
            if(tempRoad.HalfRoad)
            {
                RoadSign findBeforeRoad = tempRoad.FindNextRoad(keyOp.Revert());
                if (findBeforeRoad == null)
                {
                    Debug.LogError("不可能在这个方向推箱子的");
                    return false;
                }

                if (findBeforeRoad.IsDay==false)
                {
                    bool jumpTo = JumpTo(keyOp);
                    if (jumpTo)
                    {
                        Player.Instance.OnPushBoxStart?.OnNext(new Tuple<RoadSign, Box>(tempRoad,this));
                        return true;
                    }
                }
                else
                {
                    bool skatingTo = SkatingTo(keyOp);
                    if (skatingTo)
                    {
                        Player.Instance.OnPushBoxStart?.OnNext(new Tuple<RoadSign, Box>(tempRoad,this));
                        return true;
                    }
                }
            }
            else
            {
                if (tempRoad.IsDay)
                {
                    bool jumpTo = JumpTo(keyOp);
                    if (jumpTo)
                    {
                        Player.Instance.OnPushBoxStart?.OnNext(new Tuple<RoadSign, Box>(tempRoad,this));
                        return true;
                    }
                }
                else
                {
                    bool skatingTo = SkatingTo(keyOp);
                    if (skatingTo)
                    {
                        Player.Instance.OnPushBoxStart?.OnNext(new Tuple<RoadSign, Box>(tempRoad,this));
                        return true;
                    }
                }
            }

            return false;
        }

        [Button]
        public bool JumpTo(KeyOp keyOp)
        {
            if (TopBoxCount > 0)
            {
                Debug.Log("不能推叠起来的箱子",CurRoad.Value);
                return false;
            }

            if (CurRoad.Value == null)
            {
                Debug.LogError("为什么存在这种情况？叠起来的箱子为什么还能移动？",this);
                return false;
            }
            RoadSign findNextRoad = CurRoad.Value.FindNextRoad(keyOp);
            if (findNextRoad == null)
            {
                Debug.Log("无法平推箱子，尝试查找推箱子路径", CurRoad.Value);
                findNextRoad = CurRoad.Value.FindNextJumpBoxRoad(keyOp);
                if (findNextRoad == null)
                {
                    findNextRoad = CurRoad.Value.FindNextJumpBoxFailedRoad(keyOp);
                    Debug.Log("尝试查找推箱子失败路径");
                    if (findNextRoad == null)
                    {
                        //推到虚空
                        Debug.Log("无法找到推箱子失败路径，将箱子推到虚空下");
                        Box tempBox = CurRoad.Value.Box;
                        LevelManager.Instance.BoxMoving = true;
                        CurRoad.Value.Box.PushJumpToEmpty(keyOp);
                        Observable.Timer(TimeSpan.FromSeconds(1f))
                                  .Subscribe(l =>
                                  {
                                      LevelManager.Instance.BoxMoving = false;
                                      tempBox.TryMoveToOriginRoadConsiderPlayer();
                                  });
                        return true;
                    }

                    //无法推箱子
                    Debug.Log("找到推箱子失败路径，禁止推箱子", findNextRoad);
                    return false;
                }

                Debug.Log("可以将箱子推到下面", findNextRoad);
                return CurRoad.Value.Box.JumpToRoadConsiderBoxCount(findNextRoad);
            }

            if (findNextRoad.Stair
                || (findNextRoad.IsFullRoad == false)
                || findNextRoad.HaveBox
                || findNextRoad.HaveBlock)
            {
                Debug.Log("检测到阻挡，无法推箱子",findNextRoad);
                return false;
            }

            Debug.Log("平推箱子");
            JumpToRoadIgnoreBoxCount(findNextRoad);
            return true;
        }
        
        

        [HideInEditorMode]
        [Button("跳到指定Road（考虑箱子数量）")]
        public bool JumpToRoadConsiderBoxCount(RoadSign targetRoad)
        {
            if(targetRoad.HaveBox)
            {
                int boxCount = 1;
                Box curBox = targetRoad.Box;
                while (true)
                {
                    if (curBox.NearestTopBox!=null)
                    {
                        boxCount++;
                        if (boxCount >= 2)
                        {
                            Debug.Log("不能叠三个箱子");
                            return false;
                        }
                        curBox = curBox.NearestTopBox;
                    }
                    else
                    {
                        JumpToBox(curBox);
                        return true;
                    }
                }
            }
            JumpToRoadIgnoreBoxCount(targetRoad);
            return true;
        }

        [HideInEditorMode]
        [Button("跳到指定Road（忽略箱子数量）")]
        private void JumpToRoadIgnoreBoxCount(RoadSign targetRoad)
        {
            if(targetRoad.HaveBox==false)
            {
                LevelManager.Instance.BoxMoving = true;
                Vector3 p3 = targetRoad.TopPoint.position;
                transform.DoJump(p3,1f,0.3f).OnComplete(() =>
                {
                    LevelManager.Instance.BoxMoving = false;
                    AudioManager.Instance.PlayAudio("箱子落地");
                });
                SetToRoad(targetRoad);
            }
            else
            {
                JumpToBox(targetRoad.TopsideBox);
            }
        }
        
        [HideInEditorMode]
        [Button("跳到箱子上面")]
        public void JumpToBox(Box box)
        {
            LevelManager.Instance.BoxMoving = true;
            transform.SetParent(box.TopPoint);
            CurRoad.Value.Box = null;
            CurRoad.Value.HaveBox = false;
            CurRoad.Value = null;
            Box bottomBox = box.AddTopBox(this);
            transform.DoJump(bottomBox.TopPoint.position, 1f, 0.3f).OnComplete(() =>
            {
                LevelManager.Instance.BoxMoving = false;
            });
        }

        /// <summary>
        /// 滑到指定的Road(忽略箱子)
        /// </summary>
        /// <param name="targetRoad"></param>
        [HideInEditorMode]
        [Button]
        private void SkatingToRoadIgnoreBlock(RoadSign targetRoad,Action complete=null)
        {
            LevelManager.Instance.BoxMoving = true;
            Vector3 p3 = targetRoad.TopPoint.position;
            float distance = Vector3.Distance(targetRoad.TopPoint.position, transform.position);
            transform.DOMove(p3, 0.15f * distance).SetEase(Ease.Linear).OnComplete(() =>
            {
                LevelManager.Instance.BoxMoving = false;
                complete?.Invoke();
            });
            SetToRoad(targetRoad);
        }
        [HideInEditorMode]
        [Button]
        private void SkatingToAndSkatingEmpty(RoadSign targetRoad,KeyOp keyOp)
        {
            LevelManager.Instance.BoxMoving = true;
            Vector3 p3 = targetRoad.TopPoint.position;
            float distance = Vector3.Distance(targetRoad.TopPoint.position, transform.position);
            SetToRoad(targetRoad);
            transform.DOMove(p3, 0.15f * distance).SetEase(Ease.Linear).OnComplete(() =>
            {
                // Player.Instance.CanMove = false;
                targetRoad.Box.PushJumpToEmpty(keyOp);
                Observable.Timer(TimeSpan.FromSeconds(1f))
                          .Subscribe(l =>
                          {
                              LevelManager.Instance.BoxMoving = false;
                              TryMoveToOriginRoadConsiderPlayer();
                          });
            });
            // todo 这段时间不能移动UI和Player
        }
        [HideInEditorMode]
        [Button]
        private void SkatingToAndSkatingToRoad(RoadSign targetRoad, RoadSign targetRoad2)
        {
            LevelManager.Instance.BoxMoving = true;
            Vector3 p3 = targetRoad.TopPoint.position;
            float distance = Vector3.Distance(targetRoad.TopPoint.position, transform.position);
            SetToRoad(targetRoad);
            transform.DOMove(p3, 0.15f * distance).SetEase(Ease.Linear).OnComplete(() =>
            {
                LevelManager.Instance.BoxMoving = false;
                int targetRoad2TopBoxCount = targetRoad2.TopBoxCount;
                if (targetRoad2TopBoxCount >= 2)
                {
                    return;
                }
                SkatingToDownRoad(targetRoad2);
            });
            // todo 这段时间不能移动UI和Player
        }

        [HideInEditorMode]
        [Button]
        private void SkatingToNearestRoad(RoadSign nearRoad)
        {
            if(nearRoad.HaveBox
                || nearRoad.HaveBlock
                || nearRoad.Stair
                || (nearRoad.Door&& !nearRoad.DoorIsOpen))
            {
                return;
            }
            
            SkatingToRoadIgnoreBlock(nearRoad);
        }

        [HideInEditorMode]
        [Button]
        private void SkatingToDownRoad(RoadSign targetRoad)
        {
            LevelManager.Instance.BoxMoving = true;
            // float distance = Vector3.Distance(targetRoad.TopPoint.position, transform.position);
            transform.DoJump(targetRoad.TopestPosition, 0,0.3f).SetEase(Ease.Linear).OnComplete(() =>
            {
                LevelManager.Instance.BoxMoving = false;
            });
            SetToRoad(targetRoad);
        }
        
        [HideInEditorMode]
        [Button("朝指定方向滑动")]
        public bool SkatingTo(KeyOp keyOp)
        {
            RoadSign tempRoad = CurRoad.Value;
            if (TopBoxCount > 0)
            {
                Debug.Log("不能推叠起来的箱子",tempRoad);
                return false;
            }
            Debug.Log("正在查找末尾的格子，考虑碰撞",tempRoad);
            if (tempRoad.IsLinkBound)
            {
                RoadSign findNextRoad = tempRoad.FindNextRoad(keyOp.Revert());
                if (findNextRoad.IsDay)
                {
                    if (tempRoad.IsDay)
                    {
                        tempRoad = tempRoad.SameRoadSign;
                        SetToRoadAndSetPos(tempRoad);
                        return SkatingTo(keyOp);
                    }
                }
                else
                {
                    if (tempRoad.IsDay == false)
                    {
                        tempRoad = tempRoad.SameRoadSign;
                        SetToRoadAndSetPos(tempRoad);
                        return SkatingTo(keyOp);
                    }
                }
            }
            RoadSign skatingEndRoad = tempRoad.FindLastRoadInLineConsiderBlock(keyOp);
            
            if (skatingEndRoad == null)
                return false;
            if (skatingEndRoad == CurRoad.Value)
            {
                //往下推
                RoadSign nearRoad = skatingEndRoad.FindNextJumpBoxRoad(keyOp);
                if (nearRoad == null)
                {
                    nearRoad = skatingEndRoad.FindNextJumpBoxFailedRoad(keyOp);
                    if (nearRoad == null)
                    {
                        //虚空
                        SkatingToAndSkatingEmpty(skatingEndRoad, keyOp);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                
                SkatingToAndSkatingToRoad(skatingEndRoad, nearRoad);
                return true;
            }

            if (skatingEndRoad.IsLinkBound&& (skatingEndRoad.IsSpecialBound == false))
            {
                SkatingToRoadIgnoreBlock(skatingEndRoad);
                return true;
            }
            if (skatingEndRoad.IsFullRoad)
            {
                RoadSign nearRoad = skatingEndRoad.FindNextJumpBoxRoad(keyOp);
                if (nearRoad == null)
                {
                    nearRoad = skatingEndRoad.FindNextJumpBoxFailedRoad(keyOp);
                    if (nearRoad == null)
                    {
                        if (skatingEndRoad.FindNextRoad(keyOp) == null)
                        {
                            //虚空
                            SkatingToAndSkatingEmpty(skatingEndRoad, keyOp);
                            return true;
                        }
                    }
                    else
                    {
                        SkatingToRoadIgnoreBlock(skatingEndRoad);
                        return true;
                    }
                }

                SkatingToAndSkatingToRoad(skatingEndRoad, nearRoad);
                return true;
            }


            SkatingToRoadIgnoreBlock(skatingEndRoad);
            return true;
        }

    }
}