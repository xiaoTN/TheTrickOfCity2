using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Trick
{
    public partial class RoadSign
    {
        private GameObject _go;
        private SpriteRenderer _sr;
        private TextMeshPro _textInstance;
        public Sprite SpriteDay;
        public Sprite SpriteNight;
        public TextMeshPro TextMesh;

        [Button]
        public GameObject ShowTextMesh(KeyOp keyOp)
        {
            if (_go == null)
            {
                _go = Instantiate(TextMesh.gameObject, TopPoint);
                _go.transform.localPosition = Vector3.zero;
                _go.transform.eulerAngles = new Vector3(90, 0, 0);
                _textInstance = _go.GetComponent<TextMeshPro>();

                GameObject spriteGo = new GameObject("Sprite");
                spriteGo.layer = LayerMask.NameToLayer("Player");
                spriteGo.transform.SetParent(_go.transform);
                spriteGo.transform.localPosition = new Vector3(0, 0, 0.01f);
                spriteGo.transform.localScale = Vector3.one * 0.2f;
                _sr = spriteGo.AddComponent<SpriteRenderer>();
            }

            _go.SetActive(true);
            _textInstance.text = keyOp.ToString();


            _sr.sprite = IsDay ? SpriteDay : SpriteNight;

            switch (keyOp)
            {
                case KeyOp.W:
                    _sr.transform.localEulerAngles = new Vector3(0, 0, 90);
                    break;
                case KeyOp.A:
                    _sr.transform.localEulerAngles = new Vector3(0, 0, 180);
                    break;
                case KeyOp.S:
                    _sr.transform.localEulerAngles = new Vector3(0, 0, -90);
                    break;
                case KeyOp.D:
                    _sr.transform.localEulerAngles = new Vector3(0, 0, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(keyOp), keyOp, null);
            }

            return _go;
        }

        [Button]
        public void HideTextMesh()
        {
            if (_go != null)
            {
                _go.SetActive(false);
            }
        }

        private List<GameObject> _uiGos=new List<GameObject>();

        [Button("强制刷新UI")]
        public void ForceUpdateUI()
        {
            ShowAllAroundText(false);
            ShowAllAroundText(true);
        }
        
        [Button]
        public void ShowAllAroundText(bool show)
        {
            if (show)
            {
                if (CanFindNextRoadCanMove(KeyOp.W))
                {
                    _uiGos.Add(FindNextRoadCanMove(KeyOp.W)?.ShowTextMesh(KeyOp.W));
                }

                if (CanFindNextRoadCanMove(KeyOp.S))
                {
                    _uiGos.Add(FindNextRoadCanMove(KeyOp.S)?.ShowTextMesh(KeyOp.S));
                }

                if (CanFindNextRoadCanMove(KeyOp.A))
                {
                    _uiGos.Add(FindNextRoadCanMove(KeyOp.A)?.ShowTextMesh(KeyOp.A));
                }

                if (CanFindNextRoadCanMove(KeyOp.D))
                {
                    _uiGos.Add(FindNextRoadCanMove(KeyOp.D)?.ShowTextMesh(KeyOp.D));
                }
            }
            else
            {
                foreach (GameObject go in _uiGos)
                {
                    go.SetActive(false);
                }
                _uiGos.Clear();
                // FindNextRoad(KeyOp.W)?.HideTextMesh();
                // FindNextRoad(KeyOp.S)?.HideTextMesh();
                // FindNextRoad(KeyOp.A)?.HideTextMesh();
                // FindNextRoad(KeyOp.D)?.HideTextMesh();
            }
        }
    }
}