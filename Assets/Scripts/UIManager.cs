using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Sirenix.OdinInspector;
using Trick.Tower.UI;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Trick
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;
        public const float XAdsord = 173.4552f;
        public const float YAdsord = 122.2336f;

        public float CanvasX;
        public float CanvasY;

        public int IndexX1;
        public int IndexY1;
        public int IndexX2;
        public int IndexY2;

        public Subject<UIDrag> OnStartDragUI = new Subject<UIDrag>();
        public Subject<UIDrag> OnDragStepUI = new Subject<UIDrag>();
        public Subject<UIDrag> OnEndDragUI = new Subject<UIDrag>();

        public bool CanDrag
        {
            get
            {
                return (HalfRoadHavePlayer == false)
                       && (HalfRoadHaveBox == false)
                       && (LevelManager.Instance.SwitchingTower == false)
                       && (LevelManager.Instance.BoxMoving == false)
                       && Player.Instance.IsStop
                       && LevelManager.Instance.CurLevelRoot.InTower.Value == false
                       && DialogManager.Instance.Dialoging == false
                       && UltimateCanDrag
                       && LevelManager.Instance.PlayMovie==false
                       && LevelManager.Instance.HaveSuccessBoss==false;
            }
        }

        public bool UltimateCanDrag;

        [NonSerialized]
        public bool HalfRoadHavePlayer;

        [NonSerialized]
        public bool HalfRoadHaveBox;

        [NonSerialized]
        public bool IsDraging = false;

        public Text DislogText;
        public GameObject DayHead;
        public GameObject NightHead;
        public GameObject DayHeadTower;
        public GameObject NightHeadTower;

        public Text LevelTip;
        public Text StepTip;
        public Text TimeTip;

        public GameObject RightTopUIDay;
        public GameObject RightTopUINight;
        private CanvasGroup _dayCanvas;
        private CanvasGroup _nightCanvas;
        private TweenerCore<float, float, FloatOptions> _dayTweener;
        private TweenerCore<float, float, FloatOptions> _nightTweener;

        public CanvasGroup TrickPanel;
        public CanvasGroup TrickGamePanel;
        public CanvasGroup TowerPanel;
        public TowerPanel TowerPanelInstance;

        public void ShowTowerPanel(bool isShow)
        {
            TowerPanel.DOFade(isShow ? 1f : 0f, 0.5f);
            TrickPanel.DOFade(isShow ? 0f : 1f, 0.5f);
            TrickGamePanel.DOFade(isShow ? 0f : 1f, 0.5f);
        }

        [Button]
        public void ShowDayUI(bool isDay)
        {
            _dayTweener?.Kill();
            _nightTweener?.Kill();

            _dayCanvas.interactable = isDay;
            _nightCanvas.interactable = !isDay;
            if (isDay)
            {
                _dayCanvas.gameObject.SetActive(true);
                _dayTweener = DOTween.To(() => _dayCanvas.alpha, x => _dayCanvas.alpha = x, 1f, 0.3f)
                                     .SetEase(Ease.Linear);
                _nightTweener = DOTween.To(() => _nightCanvas.alpha, x => _nightCanvas.alpha = x, 0f, 0.3f)
                                       .SetEase(Ease.Linear)
                                       .OnComplete(() => _nightCanvas.gameObject.SetActive(false));
            }
            else
            {
                _dayTweener = DOTween.To(() => _dayCanvas.alpha, x => _dayCanvas.alpha = x, 0f, 0.3f)
                                     .SetEase(Ease.Linear)
                                     .OnComplete(() => _dayCanvas.gameObject.SetActive(false));
                _nightCanvas.gameObject.SetActive(true);
                _nightTweener = DOTween.To(() => _nightCanvas.alpha, x => _nightCanvas.alpha = x, 1f, 0.3f)
                                       .SetEase(Ease.Linear);
            }
        }

        public void Init()
        {
            
        }

        public GameObject CreateDialog(int isDay, bool isTower, string msg, float duration)
        {
            if (isDay == 1)
            {
                if (isTower)
                {
                    DayHead.SetActive(false);
                    NightHead.SetActive(false);
                    DayHeadTower.SetActive(true);
                    NightHeadTower.SetActive(false);
                }
                else
                {
                    DayHead.SetActive(true);
                    NightHead.SetActive(false);
                    DayHeadTower.SetActive(false);
                    NightHeadTower.SetActive(false);
                }
            }
            else
            {
                if (isTower)
                {
                    DayHead.SetActive(false);
                    NightHead.SetActive(false);
                    DayHeadTower.SetActive(false);
                    NightHeadTower.SetActive(true);
                }
                else
                {
                    DayHead.SetActive(false);
                    NightHead.SetActive(true);
                    DayHeadTower.SetActive(false);
                    NightHeadTower.SetActive(false);
                }
            }

            Text text = Instantiate(DislogText, DislogText.transform.parent);
            text.gameObject.SetActive(true);
            text.text = string.Format(msg);
            Sequence seq = DOTween.Sequence();
            Color color = text.color;
            color.a = 0;
            text.color = color;
            seq.Append(DOTween.To(() => 0, x =>
            {
                color.a = x;
                text.color = color;
            }, 1f, 0.3f));
            seq.Append(DOTween.To(() => 0, x =>
            {
            }, 1f, duration - 0.6f));
            seq.Append(DOTween.To(() => 1, x =>
            {
                color.a = x;
                text.color = color;
            }, 0f, 0.3f));
            return text.gameObject;
        }

        public void CloseDia()
        {
            DayHead.gameObject.SetActive(false);
            NightHead.gameObject.SetActive(false);
            DayHeadTower.gameObject.SetActive(false);
            NightHeadTower.gameObject.SetActive(false);
        }

        private void Awake()
        {
            Instance = this;
            _dayCanvas = RightTopUIDay.GetComponent<CanvasGroup>();
            _nightCanvas = RightTopUINight.GetComponent<CanvasGroup>();
            _dayCanvas.alpha = 0;
            _nightCanvas.alpha = 0;
        }

        public void HideRightTopUI()
        {
            RightTopUIDay.SetActive(false);
            RightTopUINight.SetActive(false);
        }
    }
}