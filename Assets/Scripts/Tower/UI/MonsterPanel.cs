using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Trick.Tower.UI
{
    public class MonsterPanel: MonoBehaviour
    {
        public Image HeadImg;
        public Text HPText;
        public Text AttackText;
        public Text DefendText;
        public Text DamageText;
        [HideInEditorMode]
        [Button]
        public void UpdatePanel(Monster monster)
        {
            monster.Init();
            bool? configMonsterAttributes = LevelManager.Instance.Config.MonsterAttributes;
            if (configMonsterAttributes != null)
            {
                bool monsterAttributes = (bool) configMonsterAttributes;
                HPText.gameObject.SetActive(monsterAttributes);
                AttackText.gameObject.SetActive(monsterAttributes);
                DefendText.gameObject.SetActive(monsterAttributes);
            }

            bool? configCombatPrediction = LevelManager.Instance.Config.CombatPrediction;
            if (configCombatPrediction != null)
            {
                bool combatPrediction = (bool) configCombatPrediction;
                DamageText.gameObject.SetActive(combatPrediction);
            }


            HeadImg.sprite = monster.sprite1;
            HPText.text = monster.HP.ToString();
            AttackText.text = monster.Attack.ToString();
            DefendText.text = monster.Defend.ToString();
            TowerPlayer towerPlayer = LevelManager.Instance.CurLevelRoot.TowerPlayer;
            int damage = towerPlayer.AttackRoleCostHp(monster);
            string damageText=String.Empty;
            if (damage < 0)
            {
                damageText = "<color=red>????</color>";
            }
            else if(damage<towerPlayer.HP)
            {
                damageText = damage.ToString();
            }
            else
            {
                damageText = $"<color=red>{damage}</color>";
            }
            
            DamageText.text = damageText;
        }
    }
}