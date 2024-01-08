using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UniRx;
using UniRx.Triggers;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

namespace Trick
{
    public class LevelPart : MonoBehaviour
    {
        public bool IsDay;

        [ShowInInspector]
        [HideInEditorMode]
        [NonSerialized]
        public List<RoadSign> Roads = new List<RoadSign>();

        private Camera _cam;
        private Camera _camClone;
        private LevelRoot _levelRoot;
        private RectTransform _view;

        public RenderTexture Rt;

        public void Init(bool isDay)
        {
            _skyGo = transform.Find("sky").gameObject;
            _efxGo = transform.Find(IsDay ? "DayEfx" : "NightEfx").gameObject;
            IsDay = isDay;
            Roads.AddRange(GetComponentsInChildren<RoadSign>());

            // Roads.Sort((x,y)=>x.Index.CompareTo(y.Index));
            string layerName = IsDay ? "Part1" : "Part2";
            foreach (RoadSign partRoad in Roads)
            {
                partRoad.gameObject.layer = LayerMask.NameToLayer(layerName);
                if (partRoad.HaveKey)
                {
                    partRoad.Key.IsDay = IsDay;
                }

                partRoad.Init();
            }

            _levelRoot = GetComponentInParent<LevelRoot>();
            _view = _levelRoot.Views[isDay ? 0 : 1];
            _cam = GetComponentInChildren<Camera>();
            _cam.cullingMask &= 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer(layerName);
            _cam.cullingMask |= 1 << LayerMask.NameToLayer("Tower");
            _camClone = Instantiate(_cam, _cam.transform.parent);
            _camClone.cullingMask = 1 << LayerMask.NameToLayer("Player");
            _camClone.clearFlags = CameraClearFlags.Depth;
            _camClone.depth = 100;
            PostProcessLayer postProcessLayer = _camClone.GetComponent<PostProcessLayer>();
            DestroyImmediate(postProcessLayer);
            
            if (_lookAtPointGo == null)
            {
                _lookAtPointGo = new GameObject("LookAtPoint");
                _lookAtPointGo.transform.SetParent(transform);
                _lookAtPointGo.transform.localPosition = LocalPos1;
                _lookAtPointGo.transform.localRotation = _cam.transform.localRotation;
                _cam.transform.SetParent(_lookAtPointGo.transform);
            }

            Camera camClone2 = Instantiate(_camClone, _camClone.transform.parent);
            camClone2.clearFlags = CameraClearFlags.SolidColor;
            camClone2.targetTexture = Rt;
            RawImage img = _view.Find("Image").GetComponent<RawImage>();
            RawImage imgClone = Instantiate(img, _view);
            imgClone.texture = Rt;
            imgClone.raycastTarget = false;
            bool isDraging = false;
            RectTransform rt1 = img.GetComponent<RectTransform>();
            RectTransform rt2 = imgClone.GetComponent<RectTransform>();
            UIManager.Instance.OnStartDragUI.Subscribe(drag =>
            {
                isDraging = true;
                imgClone.transform.SetParent(_view);
                rt2.localPosition = rt1.localPosition;
            }).AddTo(this);
            UIManager.Instance.OnEndDragUI.Subscribe(drag =>
            {
                isDraging = false;
                rt2.localPosition = rt1.localPosition;
                imgClone.transform.SetParent(_view.parent);
                imgClone.transform.SetAsFirstSibling();
            }).AddTo(this);
            // imgClone.gameObject.LateUpdateAsObservable()
            //         .Subscribe(unit =>
            //         {
            //             if (isDraging)
            //             {
            //                 imgClone
            //             }
            //         })
        }

        [ReadOnly]
        public GameObject LookAtPoint;


        [BoxGroup("镜头")]
        [ReadOnly]
        public Vector3 LocalPos1;

        [BoxGroup("镜头")]
        [ReadOnly]
        public Vector3 LocalPos2;

        private GameObject _lookAtPointGo;
        private Sequence _sequence;
        private GameObject _skyGo;
        private GameObject _efxGo;

        public void ShowSky(bool isShow)
        {
            _skyGo.SetActive(isShow);
            _efxGo.SetActive(isShow);
        }

#if UNITY_EDITOR
        [HideInPlayMode]
        [ShowIf("@LookAtPoint==null")]
        [Button]
        private void CreateCamLookAtPoint()
        {
            Camera cam = GetComponentInChildren<Camera>();
            GameObject go = new GameObject("LookAtPoint");
            LookAtPoint = go;
            go.transform.SetParent(transform);
            go.transform.SetSiblingIndex(cam.transform.GetSiblingIndex() + 1);
            go.transform.position = cam.transform.position;
            go.transform.rotation = cam.transform.rotation;
            Selection.activeObject = go;
        }

        [BoxGroup("镜头")]
        [HideInPlayMode]
        [Button]
        private void Save1()
        {
            LevelRoot levelRoot = GetComponentInParent<LevelRoot>();
            PrefabUtility.UnpackPrefabInstance(levelRoot.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            LocalPos1 = LookAtPoint.transform.localPosition;
            Camera cam = GetComponentInChildren<Camera>();
            cam.transform.SetParent(LookAtPoint.transform);
            Vector3 transformLocalEulerAngles = LookAtPoint.transform.localEulerAngles;
            transformLocalEulerAngles.x = 90;
            LookAtPoint.transform.localEulerAngles = transformLocalEulerAngles;
            EditorUtility.SetDirty(this);
            Selection.activeObject = LookAtPoint;
        }

        [BoxGroup("镜头")]
        [HideInPlayMode]
        [Button]
        private void Save2()
        {
            LevelRoot levelRoot = GetComponentInParent<LevelRoot>();
            GameObject tempGo = levelRoot.gameObject;
            LocalPos2 = LookAtPoint.transform.localPosition;
            GameObject prefab = Resources.Load<GameObject>($"Levels/Level{levelRoot.Index}");
            GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            levelRoot = go.transform.GetComponent<LevelRoot>();
            Transform levelRootPart = IsDay ? levelRoot.Part1 : levelRoot.Part2;
            levelRootPart.GetComponent<LevelPart>().LocalPos1 = LocalPos1;
            levelRootPart.GetComponent<LevelPart>().LocalPos2 = LocalPos2;
            PrefabUtility.ApplyPrefabInstance(go, InteractionMode.AutomatedAction);
            go.transform.SetParent(tempGo.transform.parent);
            DestroyImmediate(tempGo);
        }
#endif

        [Button]
        public IEnumerator EnterTowerEffect(bool isForward = true, Action complete = null)
        {
            if (isForward)
            {
                _sequence = DOTween.Sequence();
                _sequence.SetAutoKill(false);
                _sequence.Append(DOTween.To(() => 45f, x =>
                {
                    Vector3 transformLocalEulerAngles = _lookAtPointGo.transform.localEulerAngles;
                    transformLocalEulerAngles.x = x;
                    _lookAtPointGo.transform.localEulerAngles = transformLocalEulerAngles;
                }, 90f, 0.5f));
                _sequence.Join(DOTween.To(() => LocalPos1, x =>
                {
                    _lookAtPointGo.transform.localPosition = x;
                }, LocalPos2, 0.5f));
                Vector2 viewSizeDelta = _view.sizeDelta;
                _sequence.Join(DOTween.To(() => viewSizeDelta.y, y =>
                {
                    _view.sizeDelta = new Vector2(viewSizeDelta.x, y);
                }, viewSizeDelta.x, 0.5f));
                _sequence.Append(DOTween.To(() => 1, x =>
                {
                    _view.localScale = new Vector3(x, x, 1);
                }, 0.5f, 0.5f));
                _sequence.Join(DOTween.To(() => 0, x =>
                {
                    Vector3 viewLocalEulerAngles = _view.localEulerAngles;
                    viewLocalEulerAngles.z = x;
                    _view.localEulerAngles = viewLocalEulerAngles;
                }, 45f, 0.5f));
                _sequence.Join(DOTween.To(() => _view.anchoredPosition, x =>
                {
                    _view.anchoredPosition = x;
                }, new Vector2(UIManager.Instance.CanvasX / 2f, UIManager.Instance.CanvasY / 2f), 0.5f));
              
                // _skyGo.SetActive(false);
                _sequence.PlayForward();
            }
            else
            {
                _sequence.PlayBackwards();
                _sequence.SetAutoKill(true);
            }

            yield return new WaitForSeconds(1f);
            if (isForward == false)
            {
                // _skyGo.SetActive(true);
            }

            complete?.Invoke();
        }
    }
}