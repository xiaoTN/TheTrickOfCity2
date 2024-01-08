using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Trick
{
    public partial class LevelRoot
    {
#if UNITY_EDITOR
        [Button("设置所有障碍物的材质")]
        private void SetBlockMaterial()
        {
            //Assets/ArtResources/Textures/Trick_statue_bat03/Trick_statue_bat03a.mat
            //Assets/ArtResources/Textures/Trick_statue_bat03/Trick_statue_bat03.mat
            List<RoadSign> roadSigns = GetComponentsInChildren<RoadSign>().Where(sign =>
            {
                if (sign.HaveBlock && sign.BlockGo == null)
                {
                    Debug.LogError("block=null", sign);
                    return false;
                }

                bool signHaveBlock = sign.HaveBlock && sign.BlockGo.name.StartsWith("Trick_statue_bat03");
                if (signHaveBlock)
                {
                    Debug.Log($"block is {sign.IsDay}", sign);
                }

                return signHaveBlock;
            }).ToList();
            foreach (RoadSign roadSign in roadSigns)
            {
                roadSign.BlockGo.GetComponent<MeshRenderer>().material = AssetDatabase.LoadAssetAtPath<Material>(roadSign.IsDay ?
                    "Assets/ArtResources/Textures/Trick_statue_bat03/Trick_statue_bat03a.mat" :
                    "Assets/ArtResources/Textures/Trick_statue_bat03/Trick_statue_bat03.mat");
                EditorUtility.SetDirty(roadSign);
            }
        }

        private void OnEnable()
        {
            if (Part1 == null)
                Part1 = transform.Find("Part1");
            if (Part2 == null)
                Part2 = transform.Find("Part2");

            GameObject[] bothGos = new[]
            {
                transform.Find("Canvas").gameObject,
                Part1.Find("Cam1").gameObject,
                Part2.Find("Cam2").gameObject,
            };
            foreach (GameObject go in bothGos)
            {
                SceneVisibilityManager.instance.Hide(go, true);
                SceneVisibilityManager.instance.DisablePicking(go, true);
            }

            GameObject[] disablePickGos = new[]
            {
                Part1.Find("sky").gameObject,
                Part2.Find("sky").gameObject,
            };
            foreach (GameObject go in disablePickGos)
            {
                SceneVisibilityManager.instance.DisablePicking(go, true);
            }
        }

        [Button("设置所有物体的名字")]
        public void SetAllName()
        {
            _allRoadSigns.Clear();
            Part1 = transform.Find("Part1");
            Part2 = transform.Find("Part2");
            _allRoadSigns.AddRange(GetComponentsInChildren<RoadSign>());
            for (int index = 0; index < _allRoadSigns.Count; index++)
            {
                RoadSign road = _allRoadSigns[index];
                road.SetStandardName(index + 1);
            }

            if (StartRoadSign != null)
            {
                StartRoadSign.SetName("==出生点==");
            }

            if (EndRoadSign != null)
            {
                EndRoadSign.SetName("==终点==");
            }

            EditorUtility.SetDirty(gameObject);
            Debug.Log("设置完成");
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            int matchInfosCount = MatchInfos.Count - 1;
            for (int index = matchInfosCount; index >= 0; index--)
            {
                MatchInfo matchInfo = MatchInfos[index];
                if (matchInfo.LinkRoad1 != null)
                {
                    if (matchInfo.LinkRoad2 != null)
                    {
                        Gizmos.DrawLine(matchInfo.LinkRoad1.TopPoint.transform.position, matchInfo.LinkRoad2.TopPoint.transform.position);
                        continue;
                    }
                }

                MatchInfos.RemoveAt(index);
                EditorUtility.SetDirty(this);
            }

            Gizmos.color = Color.white;
        }

        [Button("保存文件")]
        public void Save()
        {
            GameObject[] gos = Selection.gameObjects;

            MatchInfo matchInfo = MatchInfos.Find(info => (info.XOffset == UIManager.Instance.IndexX2 - UIManager.Instance.IndexX1) && (info.YOffset == UIManager.Instance.IndexY2 - UIManager.Instance.IndexY1));
            if (matchInfo == null)
            {
                matchInfo = new MatchInfo()
                {
                    XOffset = UIManager.Instance.IndexX2 - UIManager.Instance.IndexX1,
                    YOffset = UIManager.Instance.IndexY2 - UIManager.Instance.IndexY1,
                    LinkRoad1 = gos[0].GetComponent<RoadSign>(),
                    LinkRoad2 = gos[1].GetComponent<RoadSign>(),
                };
                MatchInfos.Add(matchInfo);
            }
            else
            {
                matchInfo.LinkRoad1 = gos[0].GetComponent<RoadSign>();
                matchInfo.LinkRoad2 = gos[1].GetComponent<RoadSign>();
            }

            Debug.Log($"save :{matchInfo.XOffset},{matchInfo.YOffset}");
            _askWethearSaveMatchInfo = true;

            // JsonUtils.WriteJson(MatchInfos, _jsonPath);
        }

        private void OnApplicationQuit()
        {
            if (_askWethearSaveMatchInfo)
            {
                bool displayDialog = EditorUtility.DisplayDialog("检测到你修改了光暗连接点", "是否保存？", "是", "否");
                if (displayDialog)
                {
                    GameObject levelPrefab = Resources.Load<GameObject>($"Levels/Level{Index}");
                    GameObject prefabGo = PrefabUtility.InstantiatePrefab(levelPrefab) as GameObject;
                    LevelRoot levelRoot = prefabGo.GetComponent<LevelRoot>();
                    levelRoot.MatchInfos.Clear();
                    foreach (MatchInfo matchInfo in MatchInfos)
                    {
                        bool linkRoad1IsDay = matchInfo.LinkRoad1.IsDay;
                        int siblingIndex1 = matchInfo.LinkRoad1.transform.GetSiblingIndex();
                        bool linkRoad2IsDay = matchInfo.LinkRoad2.IsDay;
                        int siblingIndex2 = matchInfo.LinkRoad2.transform.GetSiblingIndex();

                        RoadSign linkRoad1 = (linkRoad1IsDay ? levelRoot.Part1 : levelRoot.Part2).GetChild(siblingIndex1).GetComponent<RoadSign>();
                        RoadSign linkRoad2 = (linkRoad2IsDay ? levelRoot.Part1 : levelRoot.Part2).GetChild(siblingIndex2).GetComponent<RoadSign>();
                        MatchInfo item = new MatchInfo()
                        {
                            XOffset = matchInfo.XOffset,
                            YOffset = matchInfo.YOffset,
                            LinkRoad1 = linkRoad1,
                            LinkRoad2 = linkRoad2,
                        };
                        levelRoot.MatchInfos.Add(item);
                    }

                    PrefabUtility.SaveAsPrefabAsset(prefabGo, $"Assets/Resources/Levels/Level{Index}.prefab", out bool isSuccess);
                    if (isSuccess)
                    {
                        Debug.Log("保存成功");
                    }
                    else
                    {
                        Debug.LogError("保存失败");
                    }

                    DestroyImmediate(prefabGo);
                }
            }

            if (_askWethearSaveStartIndex)
            {
                bool displayDialog = EditorUtility.DisplayDialog("检测到你修改了关卡画布初始位置", "是否保存？", "是", "否");
                if (displayDialog)
                {
                    GameObject levelPrefab = Resources.Load<GameObject>($"Levels/Level{Index}");
                    GameObject prefabGo = PrefabUtility.InstantiatePrefab(levelPrefab) as GameObject;
                    LevelRoot levelRoot = prefabGo.GetComponent<LevelRoot>();
                    levelRoot.StartView1IndexX = StartView1IndexX;
                    levelRoot.StartView1IndexY = StartView1IndexY;
                    levelRoot.StartView2IndexX = StartView2IndexX;
                    levelRoot.StartView2IndexY = StartView2IndexY;
                    PrefabUtility.SaveAsPrefabAsset(prefabGo, $"Assets/Resources/Levels/Level{Index}.prefab", out bool isSuccess);
                    if (isSuccess)
                    {
                        Debug.Log("保存成功");
                    }
                    else
                    {
                        Debug.LogError("保存失败");
                    }

                    DestroyImmediate(prefabGo);
                }
            }
        }

        [Button]
        private void CreateEmpty()
        {
            GameObject dayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/DayEfx.prefab");
            GameObject nightPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/NightEfx.prefab");

            GameObject dayGo = PrefabUtility.InstantiatePrefab(dayPrefab) as GameObject;
            GameObject nightGo = PrefabUtility.InstantiatePrefab(nightPrefab) as GameObject;
            
            dayGo.transform.SetParent(Part1);
            dayGo.transform.SetAsFirstSibling();
            dayGo.transform.localPosition=Vector3.zero;
            dayGo.transform.localRotation=Quaternion.identity;
            
            nightGo.transform.SetParent(Part2);
            nightGo.transform.SetAsFirstSibling();
            nightGo.transform.localPosition=Vector3.zero;
            nightGo.transform.localRotation=Quaternion.identity;
            
            PrefabUtility.ApplyPrefabInstance(gameObject, InteractionMode.AutomatedAction);
        }
#endif
    }
}