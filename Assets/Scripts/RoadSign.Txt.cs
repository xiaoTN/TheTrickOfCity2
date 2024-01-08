using System;
using System.IO;
using Sirenix.OdinInspector;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Trick
{
    public partial class RoadSign
    {
        [BoxGroup("可修改的")]
        [LabelText("TXT")]
        public bool HaveTxt;
        [BoxGroup("可修改的")]
        [ShowIf(nameof(HaveTxt))]
        public Txt Txt;
        [BoxGroup("魔塔")]
        [LabelText("魔塔TXT")]
        public bool HaveTxtTower;
        [BoxGroup("魔塔")]
        [ShowIf(nameof(HaveTxtTower))]
        public Txt TxtTower;
        [ReadOnly]
        public GameObject TxtPrefab;

        public void InitTxt()
        {
            if (HaveTxt)
            {
                DialogIndex = 10;
                GameObject go = Instantiate(TxtPrefab,TopPoint);
                go.transform.Find("Sprite").GetComponent<SpriteRenderer>().sortingOrder = -1;
                go.transform.localPosition=new Vector3(0,0.01f,0);
                go.name = Txt.TxtFile;
                go.SetLayers(IsDay? "Part1":"Part2");

                HavePlayer.FirstOrDefault(b => b).Subscribe(b =>
                {
                    gameObject.UpdateAsObservable()
                              .First(unit => Player.Instance.IsStop)
                              .Subscribe(unit =>
                              {
                                  Utils.OpenFile(Txt.TxtFile,Txt.Text);
                                  Destroy(go);
                                  HaveTxt = false;
                              });
                }).AddTo(this);
            }
        }

       
    }
}