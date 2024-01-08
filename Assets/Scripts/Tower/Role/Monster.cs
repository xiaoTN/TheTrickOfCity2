using System;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Trick.Tower
{
    public class Monster : BaseRole
    {
        private int _index;

        [ReadOnly]
        public Sprite sprite1;

        [ReadOnly]
        public Sprite sprite2;

        private int _originHp;
        private int _originAttack;
        private int _originDefend;

        private void Awake()
        {
            _originHp = HP;
            _originAttack = Attack;
            _originDefend = Defend;
        }
        

        public override void Init()
        {
            HP = _originHp;
            Attack = _originAttack;
            Defend = _originDefend;
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            Observable.Interval(TimeSpan.FromSeconds(0.5f))
                      .Subscribe(l =>
                      {
                          if (_index == 0)
                          {
                              sr.sprite = sprite1;
                          }
                          else if (_index == 1)
                          {
                              sr.sprite = sprite2;
                          }

                          _index++;
                          _index %= 2;
                      })
                      .AddTo(this);

            int index = 0;
            while (true)
            {
                index++;
                if (index >= LevelManager.Instance.CycleIndex)
                {
                    break;
                }

                HP = 2 * HP + 10;
                Attack = 2 * Attack + 10;
                Defend = 2 * Defend + 10;
            }
        }

        public override bool BeDo(TowerPlayer towerPlayer)
        {
            bool tryAttackRole = towerPlayer.TryAttackRole(this);
            if (tryAttackRole)
            {
                if (LevelManager.Instance.SaveInfo.TowerPlayerInfo.HaveSword == false)
                {
                    AudioManager.Instance.PlayAudio("空手攻击");
                }
                else
                {
                    AudioManager.Instance.PlayAudio("剑攻击");
                }

                if(Name.Equals("魔王"))
                {
                    LevelManager.Instance.HaveSuccessBoss = true;
                    Debug.Log("通关");
                    Observable.TimerFrame(1).Subscribe(l =>
                    {
                        DialogManager.Instance.Dia(100).ToObservable().Subscribe(unit =>
                        {
                            LevelManager.Instance.EnterLevel(1);
                            Observable.Timer(TimeSpan.FromSeconds(1.3f)).Subscribe(l1 =>
                            {
                                DialogManager.Instance.Dia(101).ToObservable().Subscribe(unit1 =>
                                {
UIManager.Instance.TrickGamePanel.gameObject.SetActive(false);
LevelManager.Instance.CurLevelRoot.gameObject.SetActive(false);
                                });
                            });
                        });
                    });
                }
                Destroy(gameObject);
            }

            return tryAttackRole;
        }
    }
}