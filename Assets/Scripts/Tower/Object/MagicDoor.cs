namespace Trick.Tower
{
    public class MagicDoor: BaseObject
    {
        public override void Init()
        {
            
        }

        public override bool BeDo(TowerPlayer towerPlayer)
        {
            LevelManager.Instance.CurLevelRoot.ExitTower();
            return true;
        }
    }
}