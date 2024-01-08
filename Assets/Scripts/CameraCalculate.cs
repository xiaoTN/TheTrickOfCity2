using Sirenix.OdinInspector;
using UnityEngine;

namespace Trick
{
    public class CameraCalculate : MonoBehaviour
    {
        public Transform Point1;
        public Transform Point2;

        [Button]
        private float Calculate()
        {
            Camera cam = GetComponent<Camera>();
            Vector3 screenPoint1 = cam.WorldToScreenPoint(Point1.position);
            Vector3 screenPoint2 = cam.WorldToScreenPoint(Point2.position);
            Debug.Log(screenPoint1);
            Debug.Log(screenPoint2);
            return Vector3.Distance(screenPoint1, screenPoint2);
        }

        [Button]
        private Vector3 WorldPos2UIPos(Camera cam,Transform worldPos)
        {
            return cam.WorldToScreenPoint(worldPos.position);
        }
    }
}