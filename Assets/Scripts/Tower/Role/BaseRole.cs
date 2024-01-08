using Sirenix.OdinInspector;
using Trick;
using UnityEngine;

namespace Trick.Tower
{
    public abstract class BaseRole: BaseObject
    {
        [LabelText("名称")]
        public string Name;
        [LabelText("血量")]
        public int HP;
        [LabelText("攻击")]
        public int Attack;
        [LabelText("防御")]
        public int Defend;

        /// <summary>
        /// 攻击角色耗费的血量
        /// </summary>
        /// <param name="otherRole"></param>
        /// <returns></returns>
        [Button]
        public int AttackRoleCostHp(BaseRole otherRole)
        {
            if (Attack <= otherRole.Defend)
            {
                return -1;
            }

            int bRound = otherRole.HP / (Attack - otherRole.Defend);
            if (otherRole.Attack - Defend <= 0)
            {
                return 0;
            }

            int aRound = HP / (otherRole.Attack - Defend);
            // if (aRound >= bRound)
            {
                return (bRound - 1) * (otherRole.Attack - Defend);
            }
            
            
        }
        [Button]
        public virtual bool TryAttackRole(BaseRole otherRole)
        {
            int attackRoleCostHp = AttackRoleCostHp(otherRole);
            if (attackRoleCostHp > HP|| attackRoleCostHp<0)
            {
                Debug.Log("玩家打不过");
                return false;
            }
            if(Attack<= otherRole.Defend)
            {
                HP = 0;
            }
            else
            {
                int bRound = otherRole.HP / (Attack - otherRole.Defend)+1;
                if (otherRole.Attack - Defend <= 0)
                {
                }
                else
                {
                    int aRound = HP / (otherRole.Attack - Defend);
                    if (aRound >= bRound)
                    {
                        HP = HP - (bRound - 1) * (otherRole.Attack - Defend);
                    }
                }

                if (HP > 0)
                {
                    Debug.Log($"胜利！{bRound} 回合后，玩家剩余血量:{HP}");
                    return true;
                }
            }

            if (HP <= 0)
            {
                Debug.Log("玩家死亡");
                return false;
            }

            return true;
        }

    }
}