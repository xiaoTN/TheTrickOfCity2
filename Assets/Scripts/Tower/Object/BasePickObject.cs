namespace Trick.Tower
{
    public abstract class BasePickObject: BaseObject
    {
        public virtual void BePick(TowerPlayer towerPlayer)
        {
            Destroy(gameObject);
        }
    }
}