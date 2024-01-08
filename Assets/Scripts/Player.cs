using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Trick
{
    public class Player : MonoBehaviour
    {
        public static Player Instance;
        private Vector3 _upAxis;
        private Quaternion _startRotation;

        private void Awake()
        {
            Instance = this;
            _curSpriteCom = transform.Find("sprite").GetComponent<SpriteRenderer>();
            ChangeModelIm(true);
        }

        [ShowInInspector]
        [ReadOnly]
        [NonSerialized]
        public ReactiveProperty<RoadSign> CurRoadSign=new ReactiveProperty<RoadSign>();
        
        /// <summary>
        /// 是否停下来了
        /// </summary>
        public bool IsStop;

        public float MoveDuration = 0.5f;
        private Tweener _moveTweener;

        public bool CanMove
        {
            get
            {
                return LevelManager.Instance.SwitchingLevel == false
                       && LevelManager.Instance.SwitchingTower == false
                       && UIManager.Instance.IsDraging == false
                       && LevelManager.Instance.BoxMoving == false
                       && LevelManager.Instance.CurLevelRoot.InTower.Value == false
                    && DialogManager.Instance.Dialoging==false
                    && LevelManager.Instance.PlayMovie==false
                    && LevelManager.Instance.HaveSuccessBoss==false;

                // && _isMoving == false;
            }
        }

        public bool CanSwitchCloth
        {
            get
            {
                return LevelManager.Instance.Config.PlayThrough > 0;
            }
        }
        
        public Subject<RoadSign> OnMoveStart = new Subject<RoadSign>();
        public Subject<RoadSign> OnMoveComplete = new Subject<RoadSign>();
        public Subject<Tuple<RoadSign,Box>> OnPushBoxStart = new Subject<Tuple<RoadSign, Box>>();
        
        private bool _isMoving = false;
        private bool _isDayCloth = false;
        private bool _isInit = false;
        public void Init()
        {
            if (_isInit)
            {
                return;
            }
            _isInit = true;
            gameObject.ObserveEveryValueChanged(_ => CanMove)
                      .Subscribe(b =>
                      {
                          if(b)
                          {
                              if (CurRoadSign.Value != null)
                              {
                                  CurRoadSign.Value.ShowAllAroundText(true);
                              }
                          }
                          else
                          {
                              Observable.TimerFrame(1).Subscribe(l =>
                              {
                                  if (CurRoadSign.Value != null)
                                  {
                                      CurRoadSign.Value.ShowAllAroundText(false);
                                  }
                              });
                          }
                      });
            transform.eulerAngles = new Vector3(45, -45, 0);
            _upAxis = transform.up;
            _startRotation = transform.rotation;
            MessageBroker.Default.Receive<string>()
                         .Where(s => s.Equals(KeyCode.W.ToString()))
                         .Where(_ => _isMoving == false && CanMove)
                         .Subscribe(unit =>
                         {
                             MoveTo((KeyOp.W));
                         })
                         .AddTo(this);
            MessageBroker.Default.Receive<string>()
                         .Where(s => s.Equals(KeyCode.S.ToString()))
                         .Where(_ => _isMoving == false && CanMove)
                         .Subscribe(unit =>
                         {
                             MoveTo((KeyOp.S));
                         })
                         .AddTo(this);
            MessageBroker.Default.Receive<string>()
                         .Where(s => s.Equals(KeyCode.A.ToString()))
                         .Where(_ => _isMoving == false && CanMove)
                         .Subscribe(unit =>
                         {
                             MoveTo((KeyOp.A));
                         })
                         .AddTo(this);
            MessageBroker.Default.Receive<string>()
                         .Where(s => s.Equals(KeyCode.D.ToString()))
                         .Where(_ => _isMoving == false && CanMove)
                         .Subscribe(unit =>
                         {
                             MoveTo((KeyOp.D));
                         })
                         .AddTo(this);

            _isDayCloth = true;
            CurRoadSign.Skip(1).Subscribe(road =>
            {
                bool isDay=road.IsDay;
                if (road.HalfRoad)
                {
                    return;
                }
                AudioManager.Instance.SwitchBg(isDay,false);
                if (road.IsDay == _isDayCloth)
                {
                    return;
                }
                if(CanSwitchCloth==false) return;
                // CanMove = false;
                _isDayCloth = isDay;
                AudioManager.Instance.PlayAudio("旋转");
                DOTween.To(() => 0f, x =>
                            {
                                transform.rotation = Quaternion.AngleAxis(x, _upAxis) * _startRotation;
                            }, 360 * 3, ChangeModelDuration).OnComplete(() =>
                        {
                            // CanMove = true;
                            ChangeModelIm(isDay);
                        });
                
                UIManager.Instance.ShowDayUI(isDay);
            }).AddTo(this);

            CurRoadSign.Skip(1).DelayFrame(1).Subscribe(sign =>
            {
                if (_lastRoad != null)
                {
                    _lastRoad.ShowAllAroundText(false);
                }
                sign.ShowAllAroundText(true);

                _lastRoad = sign;
            }).AddTo(this);
        }
       private RoadSign _lastRoad = null;

       public void TryChangeModelIm(bool isDay)
       {
           if(CanSwitchCloth==false) return;
           ChangeModelIm(isDay);
       }
        public void ChangeModelIm(bool isDay)
        {
            if (isDay)
            {
                _curSpriteCom.sprite = DaySprite;
            }
            else
            {
                _curSpriteCom.sprite = NightSprite;
            }
        }

        public Sprite DaySprite;
        public Sprite NightSprite;
        private SpriteRenderer _curSpriteCom;
        public float ChangeModelDuration = 0.3f;


        [Button]
        public void MoveTo(KeyOp keyOp)
        {
            if (CanMove == false)
            {
                AudioManager.Instance.PlayAudio("无法拖动时拿起");
                return;
            }

            RoadSign roadSign = CurRoadSign.Value.FindNextRoadCanMove(keyOp);
            if (roadSign == null)
            {
                Debug.Log("玩家找不到下一条路", CurRoadSign.Value);
                AudioManager.Instance.PlayAudio("无法拖动时拿起");
                return;
            }

            EnableTransferToTower = true;
            OnMoveStart?.OnNext(CurRoadSign.Value);
            if (roadSign.HaveBox)
            {
                bool pushTo = roadSign.Box.PushTo(keyOp);
                if (pushTo==false)
                {
                    return;
                }
            }
            MoveToAsync(roadSign, MoveDuration).Subscribe().AddTo(this);
        }

      

        private IObservable<Unit> MoveToAsync(RoadSign roadSign, float moveDuration)
        {
            IEnumerator Temp()
            {
                AudioManager.Instance.PlayAudio("移动", 0.5f);
                _isMoving = true;
                _moveTweener= transform.DoJump(roadSign.TopPoint.transform.position, 1f, moveDuration).SetEase(Ease.Linear);
                yield return _moveTweener.WaitForCompletion();
                
                IsStop = true;
                transform.SetParent(CurRoadSign.Value.transform);
                _isMoving = false;
                OnMoveComplete?.OnNext(roadSign);
            }

            IsStop = false;
            transform.SetParent(null);
            SetToRoad(roadSign);

            return Temp().ToObservable().DoOnCancel(() =>
            {
                _moveTweener?.Kill();
            });
        }

        private void SetToRoad(RoadSign roadSign)
        {
            if (CurRoadSign.Value != null)
                CurRoadSign.Value.HavePlayer.Value = false;
            CurRoadSign.Value = roadSign;
            CurRoadSign.Value.HavePlayer.Value = true;
        }

        [ReadOnly]
        [LabelText("是否允许传送")]
        [ShowInInspector]
        public bool EnableTransferToTower { get; set; }
        public void SetToRoadAndSetPos(RoadSign roadSign)
        {
            SetToRoad(roadSign);
            transform.SetParent(CurRoadSign.Value.transform);
            transform.position = roadSign.TopPoint.transform.position;
        }
    }
}