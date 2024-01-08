using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace Trick.Editor
{
    public class BindHelper
    {
        [MenuItem("Tools/LinkSelects %1")]
        public static void LinkRoad()
        {
            GameObject[] gos = Selection.gameObjects;
            foreach (GameObject go1 in gos)
            {
                RoadSign roadSign = go1.GetComponent<RoadSign>();
                foreach (GameObject go2 in gos)
                {
                    if (go1.Equals(go2))
                    {
                        continue;
                    }

                    RoadSign sign = go2.GetComponent<RoadSign>();
                    if(roadSign.Links.Contains(sign)==false)
                    {
                        roadSign.Links.Add(sign);
                    }
                    EditorUtility.SetDirty(sign);
                }
                EditorUtility.SetDirty(roadSign);
            }
        }
        [MenuItem("Tools/JumpLinkSelects %2")]
        public static void JumpLinkRoad()
        {
            GameObject[] gos = Selection.gameObjects;

            GameObject topGo = gos[0];
            GameObject downGo = gos[1];
            if (topGo.transform.position.y < gos[1].transform.position.y)
            {
                topGo = gos[1];
                downGo = gos[0];
            }

            RoadSign component = topGo.GetComponent<RoadSign>();
            List<RoadSign> jumpLinks = component.JumpLinks;
            RoadSign roadSign = downGo.GetComponent<RoadSign>();
            if(jumpLinks.Contains(roadSign)==false)
            {
                jumpLinks.Add(roadSign);
            }
            EditorUtility.SetDirty(component);
 
        }

        [MenuItem("CONTEXT/Transform/复制Transform")]
        public static void CopyTransform()
        {
            GameObject[] gos = Selection.gameObjects;
            Transform root = gos[0].transform;
            Vector3 localPos = root.localPosition;
            Vector3 localEulerAngles = root.localEulerAngles;
            GUIUtility.systemCopyBuffer = $"new Vector3({localPos.x}f,{localPos.y}f,{localPos.z}f), Quaternion.Euler({localEulerAngles.x}f,{localEulerAngles.y}f,{localEulerAngles.z}f)";
        }
    }
}