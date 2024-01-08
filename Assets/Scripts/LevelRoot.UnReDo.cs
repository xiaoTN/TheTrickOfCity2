using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Trick
{
    public partial class LevelRoot
    {
        [NonSerialized]
        [ShowInInspector]
        private Stack<IList<BaseOper>> _baseOpers=new Stack<IList<BaseOper>>();

        private int _tempIndex=-1;
        private IList<BaseOper> _tempBaseOper=new List<BaseOper>();
        
        
        public void PushOper(int index, BaseOper baseOper)
        {
            if (index != _tempIndex)
            {
                _tempBaseOper=new List<BaseOper>();
            }
            _tempBaseOper.Add(baseOper);
            if (_tempIndex != index)
            {
                _baseOpers.Push(_tempBaseOper);
            }
            _tempIndex = index;
        }
        
        public void PushOper(IList<BaseOper> list)
        {
            _baseOpers.Push(list);
        }
        
        public void ReDo()
        {
            
        }

        [HideInEditorMode]
        [ShowInInspector]
        [NonSerialized]
        [ReadOnly]
        public int OperCount=0;
        public void InitUnDo()
        {
            Player.Instance.OnMoveStart.Subscribe(_ =>
            {
                OperCount++;
                Debug.Log($"OnMoveStart:{OperCount}");
                PushOper(OperCount,new PlayerOper(Player.Instance.CurRoadSign.Value));
            }).AddTo(this);
            Player.Instance.OnMoveComplete.Subscribe(sign =>
            {
                MoveCount.Value++;
            }).AddTo(this);
            Player.Instance.OnPushBoxStart.Subscribe(sign =>
            {
                PushOper(OperCount,new BoxOper(sign.Item2,sign.Item1));
            }).AddTo(this);
            UIManager.Instance.OnStartDragUI.Subscribe(uiDrag =>
            {
                MoveCount.Value++;
                OperCount++;
                PushOper(OperCount, new ViewOper(uiDrag.IsFirstTexture, uiDrag.XIndex.Value, uiDrag.YIndex.Value));
                PushOper(OperCount, new ViewOper(!uiDrag.IsFirstTexture, uiDrag.AnotherUI.XIndex.Value, uiDrag.AnotherUI.YIndex.Value));
            }).AddTo(this);
        }

        [Button("回退")]
        public void UnDo()
        {
            if(MoveCount.Value==0) return;
            if(_baseOpers.Count==0) return;
            MoveCount.Value--;
            IList<BaseOper> baseOpers = _baseOpers.Pop();
            foreach (BaseOper baseOper in baseOpers)
            {
                baseOper.UnDo();
            }
        }

        [Button]
        public void SetPlayerTo(RoadSign roadSign)
        {
            Player.Instance.SetToRoadAndSetPos(roadSign);
        }

        [Button]
        public void SetBoxTo(Box box,RoadSign roadSign)
        {
            box.SetToRoadAndSetPos(roadSign);
        }

        [Button]
        public void SetView(bool isFirst, int xIndex, int yIndex)
        {
            foreach (UIDrag uiDrag in _drags)
            {
                if (uiDrag.IsFirstTexture != isFirst)
                {
                    continue;
                }
                uiDrag.AdjustUIToBound(xIndex,yIndex);
                uiDrag.UpdateUI();
            }
        }
    }

    [Serializable]
    public abstract class BaseOper
    {
        protected LevelRoot _levelRoot;

        protected BaseOper()
        {
            _levelRoot = LevelManager.Instance.CurLevelRoot;
            if (_levelRoot == null)
            {
                Debug.LogError("当前关卡为null");
            }
        }

        protected BaseOper(LevelRoot levelRoot)
        {
            _levelRoot = levelRoot;
        }

        public abstract void UnDo();
    }

    [Serializable]
    public class PlayerOper: BaseOper
    {
        public RoadSign Road;
        public override void UnDo()
        {
            _levelRoot.SetPlayerTo(Road);
        }

        public PlayerOper(RoadSign road)
        {
            Road = road;
        }

        public PlayerOper(LevelRoot levelRoot, RoadSign road) : base(levelRoot)
        {
            Road = road;
        }
    }

    [Serializable]
    public class BoxOper : BaseOper
    {
        public Box Box;
        public RoadSign Road;
        public override void UnDo()
        {
            _levelRoot.SetBoxTo(Box,Road);
        }

        public BoxOper(Box box, RoadSign road)
        {
            Box = box;
            Road = road;
        }

        public BoxOper(LevelRoot levelRoot, Box box, RoadSign road) : base(levelRoot)
        {
            Box = box;
            Road = road;
        }
    }

    [Serializable]
    public class ViewOper : BaseOper
    {
        public bool IsFirst;
        public int XIndex;
        public int YIndex;
        public override void UnDo()
        {
            _levelRoot.SetView(IsFirst,XIndex,YIndex);
            _levelRoot.TryLinkAllSameRoad();
        }

        public ViewOper(bool isFirst, int xIndex, int yIndex)
        {
            IsFirst = isFirst;
            XIndex = xIndex;
            YIndex = yIndex;
        }

        public ViewOper(LevelRoot levelRoot, bool isFirst, int xIndex, int yIndex) : base(levelRoot)
        {
            IsFirst = isFirst;
            XIndex = xIndex;
            YIndex = yIndex;
        }
    }
}