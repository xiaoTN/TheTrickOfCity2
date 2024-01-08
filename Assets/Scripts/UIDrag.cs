using System;
using DG.Tweening.Core;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Trick
{
    public class UIDrag : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerDownHandler
    {
        private RectTransform _rectTransform;
        private Vector2       _offset;

        [ReadOnly]
        public bool IsFirstTexture;

        private UIExpand _uiExpand;

        public UIExpand UiExpand
        {
            get { return _uiExpand; }
        }

        private RawImage _childImg;

        [ShowInInspector]
        [NonSerialized]
        public UIDrag AnotherUI;
        [ShowInInspector]
        [NonSerialized]
        public IntReactiveProperty XIndex=new IntReactiveProperty();
        [ShowInInspector]
        [NonSerialized]
        public IntReactiveProperty YIndex=new IntReactiveProperty();

        private int _maxIndex;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _uiExpand = GetComponent<UIExpand>();
            _childImg = transform.Find("Image").GetComponent<RawImage>();
            if (IsFirstTexture)
            {
                AnotherUI = transform.parent.Find("View2").GetComponent<UIDrag>();
            }
            else
            {
                AnotherUI = transform.parent.Find("View1").GetComponent<UIDrag>();
            }
            _maxIndex = _uiExpand.XCount;

            _isOverlap.Skip(1).Subscribe(b =>
            {
                if (b)
                {
                    Debug.Log("检测到覆盖");
                    // _tempViewClone = GameObject.Instantiate(AnotherUI.gameObject, AnotherUI.transform.parent);
                    // _tempViewClone.transform.SetLocalPositionAndRotation(AnotherUI.transform.localPosition, AnotherUI.transform.localRotation);
                    // _tempViewClone.transform.SetAsFirstSibling();
                    // _tempViewClone.GetComponent<UIDrag>().Translucence(0.2f);
                    //设置真实的View的位置和透明度
                    // AnotherUI.Translucence(0.5f);
                    if (Mathf.Abs(XIndex.Value - AnotherUI.XIndex.Value) < _maxIndex)
                    {
                        if (YIndex.Value == 0)
                        {
                            if (XIndex.Value > _maxIndex / 2f)
                            {
                                AnotherUI.AdjustUIToBound(0,_maxIndex);
                            }
                            else
                            {
                                AnotherUI.AdjustUIToBound(_maxIndex,_maxIndex);
                            }
                        }
                        else if(YIndex.Value== _maxIndex)
                        {
                            if (XIndex.Value > _maxIndex / 2f)
                            {
                                AnotherUI.AdjustUIToBound(0,0);
                            }
                            else
                            {
                                AnotherUI.AdjustUIToBound(_maxIndex,0);
                            }
                        }
                        else
                        {
                            if (XIndex.Value > _maxIndex / 2f)
                            {
                                AnotherUI.AdjustUIToBound(0,AnotherUI.YIndex.Value);
                            }
                            else
                            {
                                AnotherUI.AdjustUIToBound(_maxIndex,AnotherUI.YIndex.Value);
                            }
                        }
                    }
                }
            }).AddTo(this);

            XIndex.Merge(YIndex)
                  .Subscribe(_ =>
                  {
                      UIManager.Instance.OnDragStepUI?.OnNext(this);
                  })
                  .AddTo(this);
        }

        [Button]
        public void UpdateUI()
        {
            if (IsOverlap()==false)
            {
                return;
            }
            _isOverlap.SetValueAndForceNotify(true);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            UIManager.Instance.IsDraging = true;
            transform.SetAsLastSibling();
            
            if (UIManager.Instance.CanDrag == false)
            {
                AudioManager.Instance.PlayAudio("无法拖动时拿起");
                Debug.Log("禁止拖拽");
                return;
            }

            UIManager.Instance.OnStartDragUI.OnNext(this);
            AudioManager.Instance.PlayAudio("可以拖动时拿起");
            Vector2 startRectPos = _rectTransform.anchoredPosition;
            Vector2 startPos = Camera.main.ScreenToViewportPoint(Input.mousePosition) * new Vector2(UIManager.Instance.CanvasX, UIManager.Instance.CanvasY);
            _offset = startRectPos - new Vector2(startPos.x, startPos.y);
        }

        [Button]
        public void OnEndDrag(PointerEventData eventData)
        {
            UIManager.Instance.IsDraging = false;
            if (UIManager.Instance.CanDrag == false)
                return;
            _offset = Vector2.zero;
            AudioManager.Instance.PlayAudio("拖动后放下");
            AdjustUI();
            _isOverlap.Value = false;
            UIManager.Instance.OnEndDragUI.OnNext(this);
        }
        
        

        [Button("调整UI到网格")]
        public void AdjustUI()
        {
            float x = _rectTransform.anchoredPosition.x;
            float y = _rectTransform.anchoredPosition.y;

            x = Mathf.Clamp(x, 0, UIManager.Instance.CanvasX);
            y = Mathf.Clamp(y, 0, UIManager.Instance.CanvasY);

            int xCount = (int) (UIManager.Instance.CanvasX / UIManager.XAdsord);
            int yCount = (int) (UIManager.Instance.CanvasY / UIManager.YAdsord);
            // Debug.Log($"canvas:{xCount}:{yCount}");
            float findXPos = 0;
            float findYPos = 0;
            int findXIndex = 0;
            int findYIndex = 0;
            float instanceXAdsord = 0;
            bool isFindNew = false;
            for (int i = 0; i < xCount - _uiExpand.XCount + 1; i++)
            {
                instanceXAdsord = UIManager.XAdsord * i;
                findXIndex = i;

                if (Mathf.Abs(instanceXAdsord - x) < UIManager.XAdsord / 2)
                {
                    break;
                }
            }

            findXPos = instanceXAdsord;

            float instanceYAdsord = 0;
            for (int i = 0; i < yCount - _uiExpand.YCount + 1; i++)
            {
                instanceYAdsord = UIManager.YAdsord * i;
                findYIndex = i;

                if (Mathf.Abs(instanceYAdsord - y) < UIManager.YAdsord / 2)
                {
                    break;
                }
            }

            findYPos = instanceYAdsord;

            _rectTransform.anchoredPosition = new Vector2(findXPos, findYPos);
            // Debug.Log($"find x:{findXIndex}\ty:{findYIndex}");
            AdjustUIToBound(findXIndex, findYIndex);
        }

        /// <summary>
        /// 将UI调整到边界上
        /// </summary>
        [Button("调整UI到边界")]
        public void AdjustUIToBound(int xIndex,int yIndex)
        {
            // Debug.Log($"AdjustUIToBound:{xIndex},{yIndex}");
            xIndex = Mathf.Clamp(xIndex, 0, _maxIndex);
            yIndex = Mathf.Clamp(yIndex, 0, _maxIndex);

            float halfMaxIndex = _maxIndex/2f;
            if (xIndex < halfMaxIndex)
            {
                if (yIndex < halfMaxIndex)
                {
                    if (xIndex < yIndex)
                    {
                        xIndex = 0;
                    }
                    else
                    {
                        yIndex = 0;
                    }
                }
                else
                {
                    if (xIndex < _maxIndex-yIndex)
                    {
                        xIndex = 0;
                    }
                    else
                    {
                        yIndex = _maxIndex;
                    }
                }
            }
            else
            {
                if (yIndex < halfMaxIndex)
                {
                    if (_maxIndex-xIndex < yIndex)
                    {
                        xIndex = _maxIndex;
                    }
                    else
                    {
                        yIndex = 0;
                    }
                }
                else
                {
                    if (_maxIndex-xIndex <_maxIndex- yIndex)
                    {
                        xIndex = _maxIndex;
                    }
                    else
                    {
                        yIndex = _maxIndex;
                    }
                }
            }
            
            
            _rectTransform.anchoredPosition = new Vector2(xIndex*UIManager.XAdsord, yIndex*UIManager.YAdsord);
        
            if (IsFirstTexture)
            {
                UIManager.Instance.IndexX1 = xIndex;
                UIManager.Instance.IndexY1 = yIndex;
            }
            else
            {
                UIManager.Instance.IndexX2 = xIndex;
                UIManager.Instance.IndexY2 = yIndex;
            }
            
            XIndex.Value = xIndex;
            YIndex.Value = yIndex;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (UIManager.Instance.CanDrag == false)
                return;
            Vector2 viewPos = Camera.main.ScreenToViewportPoint(Input.mousePosition) * new Vector2(UIManager.Instance.CanvasX, UIManager.Instance.CanvasY);

            // Debug.Log(viewPos);
            _rectTransform.anchoredPosition = viewPos + _offset;
            AdjustUI();
            
            
            //判断有没有压住另一个Texture
            _isOverlap.Value = IsOverlap();
        }

        private bool IsOverlap()
        {
            return !(Mathf.Abs(AnotherUI.XIndex.Value - XIndex.Value) == _maxIndex
                    || Mathf.Abs(AnotherUI.YIndex.Value - YIndex.Value) == _maxIndex);
        }

        /// <summary>
        /// UI是否覆盖
        /// </summary>
        private ReactiveProperty<bool> _isOverlap=new ReactiveProperty<bool>();
        private GameObject _tempViewClone;

        public void OnPointerDown(PointerEventData eventData)
        {
            AudioManager.Instance.PlayAudio("点击");
        }

        /// <summary>
        /// 半透明
        /// </summary>
        [Button]
        public void UnTranslucence()
        {
            Translucence(1f);
        }

        public void Translucence(float alpha)
        {
            Color tempColor = _childImg.color;
            tempColor.a = alpha;
            _childImg.color = tempColor;
        }
    }
}