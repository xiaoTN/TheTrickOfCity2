using System;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Trick.Tower
{
    public abstract class BaseObject: MonoBehaviour
    {
        [NonSerialized]
        public ReactiveProperty<RoadSign> CurRoadSign=new ReactiveProperty<RoadSign>();

        public abstract void Init();
        [Button]
        public void SetToRoad(RoadSign roadSign)
        {
            transform.position = roadSign.TowerRoot.position;
            CurRoadSign.Value = roadSign;
        }

        public abstract bool BeDo(TowerPlayer towerPlayer);
    }
}