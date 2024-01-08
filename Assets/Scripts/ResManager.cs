using UnityEngine;

namespace Trick
{
    public class ResManager: MonoSingleton<ResManager>
    {
        public GameObject LoadLevel(int index)
        {
            return Resources.Load<GameObject>($"Levels/Level{index}");
        }

        public GameObject LoadStartPoint()
        {
            return Resources.Load<GameObject>("StartPoint");
        }

        public GameObject LoadRole(string roleName)
        {
            return Resources.Load<GameObject>($"TowerRoles/{roleName}");
        }
    }
}