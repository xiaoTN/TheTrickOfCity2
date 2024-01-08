using System;
using Sirenix.OdinInspector;
using Trick.Tower;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Trick
{
    public partial class RoadSign
    {
        [BoxGroup("魔塔")]
        [DisableInPlayMode]
        [LabelText("魔塔资源")]
        [AssetList(Path = "/Resources/Tower/")]
        [PreviewField(ObjectFieldAlignment.Center)]
        public GameObject TowerObjectPrefab;
        [BoxGroup("魔塔")]
        [HideInEditorMode]
        [NonSerialized, ShowInInspector]
        public BaseObject TowerObjectInstance;
        [BoxGroup("魔塔")]
        [HideInEditorMode]
        [ReadOnly]
        [NonSerialized, ShowInInspector]
        public bool HaveDo;
        [BoxGroup("魔塔")]
        [HideInEditorMode]
        [NonSerialized, ShowInInspector]
        public RoadSign WRoad;
        [BoxGroup("魔塔")]
        [HideInEditorMode]
        [NonSerialized, ShowInInspector]
        public RoadSign SRoad;
        [BoxGroup("魔塔")]
        [HideInEditorMode]
        [NonSerialized, ShowInInspector]
        public RoadSign ARoad;
        [BoxGroup("魔塔")]
        [HideInEditorMode]
        [NonSerialized, ShowInInspector]
        public RoadSign DRoad;


        public RoadSign GetTowerNextRoad(KeyOp keyOp)
        {
            switch (keyOp)
            {
                case KeyOp.W:
                    return WRoad;
                    break;
                case KeyOp.A:
                    return ARoad;
                    break;
                case KeyOp.S:
                    return SRoad;
                    break;
                case KeyOp.D:
                    return DRoad;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(keyOp), keyOp, null);
            }
        }
        
        [BoxGroup("魔塔")]
        [ShowInInspector]
        [HideInEditorMode]
        private Transform _towerRoot;

        public Transform TowerRoot
        {
            get { return _towerRoot; }
        }

        [BoxGroup("魔塔")]
        [ReadOnly]
        [SerializeField]
        public Sprite SpriteRoad;

        private GameObject _magicDoorGo;
        [NonSerialized]
        public GameObject TowerTxtGo;

        private const float _defaultSpriteScale=3.131634f;
        
        public void SwitchToTower()
        {
            CreateTowerSprite(SpriteRoad);
            CreateTowerObject();
            if (HaveTxtTower)
            {
                DialogIndexTower = 10;
                TowerTxtGo = Instantiate(TxtPrefab,TowerRoot);
                TowerTxtGo.name = TxtTower.TxtFile;
                TowerTxtGo.transform.GetChild(0).localEulerAngles=new Vector3(0,0,0);
                TowerTxtGo.SetLayers("Tower");
            }
        }

        private void InitTower()
        {
          
        }

        private void CreateTowerObject()
        {
            if(TowerObjectPrefab==null) return;
            MagicDoor magicDoor = TowerObjectPrefab.GetComponent<MagicDoor>();
            if(HaveDo&& (magicDoor == false)) return;
            GameObject go = GameObject.Instantiate(TowerObjectPrefab);
            go.transform.SetParent(_towerRoot);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one*_defaultSpriteScale;
            if (magicDoor)
            {
                go.transform.localScale=Vector3.one;
            }
            go.SetLayers("Tower");
            TowerObjectInstance = go.GetComponent<BaseObject>();
            TowerObjectInstance.Init();
        }
        
        private void CreateTowerSprite(Sprite sprite,int sortOrder=0)
        {
            if (sprite == null)
            {
                return;
            }
            if (_towerRoot == null)
            {
                _towerRoot = new GameObject($"{_index}").transform;
                _towerRoot.SetParent(TopPoint.parent);
                _towerRoot.localPosition = new Vector3(0, 1.42f, 0);
                _towerRoot.eulerAngles = new Vector3(90, 0, 0);
                _towerRoot.localScale = Vector3.one;
            }

            GameObject towerSpriteGo = new GameObject(sprite.name);
            towerSpriteGo.layer = LayerMask.NameToLayer("Tower");
            SpriteRenderer sr = towerSpriteGo.AddComponent<SpriteRenderer>();
            towerSpriteGo.transform.SetParent(_towerRoot);
            towerSpriteGo.transform.localPosition = Vector3.zero;
            towerSpriteGo.transform.localRotation=Quaternion.identity;
            towerSpriteGo.transform.localScale = Vector3.one*_defaultSpriteScale;
            sr.sprite = sprite;
            sr.sortingOrder = sortOrder;
        }

        [BoxGroup("魔塔")]
        [ShowIf("@_towerRoot!=null")]
        [HideInEditorMode]
        [Button("显示/隐藏魔塔贴图")]
        public void ShowTowerSprite(bool isShow)
        {
            if(_towerRoot==null) return;
            _towerRoot.gameObject.SetActive(isShow);
        }

        public bool HaveMagicDoor
        {
            get { return _magicDoorGo != null; }
        }
        private void CreateTowerMagicDoor()
        {
            _magicDoorGo = Instantiate(TowerObjectPrefab, TopPoint);
            _magicDoorGo.layer = gameObject.layer;
            _magicDoorGo.transform.localPosition=new Vector3(0,0.01f,0);
            _magicDoorGo.transform.localRotation=Quaternion.Euler(90,0,45);
            _magicDoorGo.transform.localScale=Vector3.one;
            _magicDoorGo.SetLayers(IsDay? "Part1":"Part2");
        }
    }
}