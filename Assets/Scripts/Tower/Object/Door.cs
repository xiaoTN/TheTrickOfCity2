using UnityEngine;

namespace Trick.Tower
{
    public class Door: BaseObject
    {
        public int ID;

        public override void Init()
        {
            
        }

        public override bool BeDo(TowerPlayer towerPlayer)
        {
            if (ID == 0)
            {
                if(LevelManager.Instance.SaveInfo.TowerPlayerInfo.Key0>0)
                {
                    AudioManager.Instance.PlayAudio("024-Door01");
                    Debug.Log("魔塔门打开了");
                    Destroy(gameObject);
                    LevelManager.Instance.SaveInfo.TowerPlayerInfo.Key0--;
                    return true;
                }
            }

            if(ID==1)
            {
                if(LevelManager.Instance.SaveInfo.TowerPlayerInfo.Key1>0)
                {
                    AudioManager.Instance.PlayAudio("024-Door01");
                    Debug.Log("魔塔门打开了");
                    Destroy(gameObject);
                    LevelManager.Instance.SaveInfo.TowerPlayerInfo.Key1--;
                    return true;
                }
            }

            return false;
        }
    }
}