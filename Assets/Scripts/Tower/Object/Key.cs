namespace Trick.Tower
{
    public class Key: BasePickObject
    {
        public int ID;
        public override void Init()
        {
            
        }

        public override bool BeDo(TowerPlayer towerPlayer)
        {
            BePick(towerPlayer);
            return true;
        }

        public override void BePick(TowerPlayer towerPlayer)
        {
            base.BePick(towerPlayer);
            if (ID == 0)
            {
                LevelManager.Instance.SaveInfo.TowerPlayerInfo.Key0++;
            }
            else if(ID==1)
            {
                LevelManager.Instance.SaveInfo.TowerPlayerInfo.Key1++;
            }
        }
    }
}