using System;
using UnityEngine;

namespace Trick.Tower
{
    public class Item : BasePickObject
    {
        public string Name;
        public int HP;
        public int Attack;
        public int Defend;

        private int _originHp;
        private int _originAttack;
        private int _originDefend;
        private void Awake()
        {
            _originHp = HP;
            _originAttack = Attack;
            _originDefend = Defend;
        }

        public override void BePick(TowerPlayer towerPlayer)
        {
            base.BePick(towerPlayer);
            HP = _originHp;
            Attack = _originAttack;
            Defend = _originDefend;
            int index = 0;
            while (true)
            {
                index++;
                if (index >= LevelManager.Instance.CycleIndex)
                {
                    break;
                }

                HP = 2 * HP;
                Attack = 2 * Attack;
                Defend = 2 * Defend;
            }
            
            LevelManager.Instance.SaveInfo.TowerPlayerInfo.HP += HP;
            LevelManager.Instance.SaveInfo.TowerPlayerInfo.Attack += Attack;
            LevelManager.Instance.SaveInfo.TowerPlayerInfo.Defend += Defend;
            towerPlayer.HP = LevelManager.Instance.SaveInfo.TowerPlayerInfo.HP;
            towerPlayer.Attack = LevelManager.Instance.SaveInfo.TowerPlayerInfo.Attack;
            towerPlayer.Defend = LevelManager.Instance.SaveInfo.TowerPlayerInfo.Defend;
        }

        public override void Init()
        {
          
        }

        public override bool BeDo(TowerPlayer towerPlayer)
        {
            if (Name.Contains("血瓶"))
            {
                AudioManager.Instance.PlayAudio("喝药");
            }
            else
            {
                AudioManager.Instance.PlayAudio("喝药");
            }
            if(Name.Equals("大宝剑"))
            {
                LevelManager.Instance.SaveInfo.TowerPlayerInfo.HaveSword = true;
            }
            BePick(towerPlayer);
            return true;
        }
    }
}