using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Trick.Tower
{
    public class TowerPlayer : BaseRole
    {
        public RuntimeAnimatorController DayController;
        public RuntimeAnimatorController NightController;

        public Animator Anim;

        public void Init(int hp, int attack, int defend)
        {
            HP = hp;
            Attack = attack;
            Defend = defend;
            CurRoadSign.Subscribe(sign =>
            {
                if (sign == null)
                    return;
                if (sign.DialogIndexTower == -1)
                    return;
                if (sign.HaveTxtTower)
                {
                    Observable.TimerFrame(1).Subscribe(l =>
                    {
                        // StartCoroutine(DialogManager.Instance.Dia(sign.DialogIndexTower));
                        Utils.OpenFile(sign.TxtTower.TxtFile,sign.TxtTower.Text);
                        Destroy(sign.TowerTxtGo);
                        sign.HaveTxtTower = false;
                    });
                }
            }).AddTo(this);
        }

        public override void Init()
        {
        }

        [HideInEditorMode]
        [Button]
        public void Switch(bool isDay)
        {
            Anim.runtimeAnimatorController = isDay ? DayController : NightController;
        }

        public void PickObject(BasePickObject pickObject)
        {
            pickObject.BePick(this);
        }

        [Button]
        public void MoveTo(KeyOp keyOp)
        {
            RoadSign towerNextRoad = CurRoadSign.Value.GetTowerNextRoad(keyOp);
            if (towerNextRoad == null)
            {
                Debug.Log("魔塔没路了");
                return;
            }

            bool isSuccess = towerNextRoad.TowerObjectInstance == null || towerNextRoad.TowerObjectInstance.BeDo(this);
            if (isSuccess)
            {
                if (towerNextRoad.TowerObjectInstance != null
                    && (towerNextRoad.TowerObjectInstance is MagicDoor == false))
                {
                    towerNextRoad.HaveDo = true;
                    UIManager.Instance.TowerPanelInstance.UpdateUI();
                }

                SetToRoad(towerNextRoad);
            }
        }

        public override bool BeDo(TowerPlayer towerPlayer)
        {
            throw new NotImplementedException();
        }

        public override bool TryAttackRole(BaseRole otherRole)
        {
            bool success = base.TryAttackRole(otherRole);
            LevelManager.Instance.SaveInfo.TowerPlayerInfo.HP = HP;
            LevelManager.Instance.SaveInfo.TowerPlayerInfo.Attack = Attack;
            LevelManager.Instance.SaveInfo.TowerPlayerInfo.Defend = Defend;
            return success;
        }
    }
}