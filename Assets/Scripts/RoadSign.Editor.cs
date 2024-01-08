
using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Trick
{
    public partial class RoadSign
    {
#if UNITY_EDITOR
        [BoxGroup("可修改的")]
        [ShowIf(nameof(Stair))]
        [Button("旋转")]
        private void RotateStair()
        {
            EditorUtility.SetDirty(this);
            Vector3 localEulerAngles = transform.localEulerAngles;
            localEulerAngles.y += 90;
            transform.localEulerAngles = localEulerAngles;
        }
        private void ChangeToOtherRoad()
        {
            Stair = !Stair;
            BreakAllRoad();
            GameObject newGo;
            if (Stair)
            {
                GameObject stairPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ArtResources/Prefabs/Trick_Stair01_p.prefab");
                newGo = PrefabUtility.InstantiatePrefab(stairPrefab) as GameObject;
            }
            else
            {
                GameObject cubePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ArtResources/Prefabs/Trick_box01_p.prefab");
                newGo = PrefabUtility.InstantiatePrefab(cubePrefab) as GameObject;
            }

            newGo.GetComponent<RoadSign>().IsDay = IsDay;
            newGo.transform.SetParent(transform.parent);
            newGo.transform.SetSiblingIndex(transform.GetSiblingIndex());
            newGo.transform.localPosition = transform.localPosition;
            if (Stair)
                newGo.transform.localRotation = transform.localRotation;
            bool success;
            Utils.DestroyLevelPrefabObj(this, gameObject, out success);
            if (success == false)
                return;

            // DestroyImmediate(gameObject);
            Selection.activeGameObject = newGo;
            EditorUtility.SetDirty(newGo);
        }
        private void OnIsDayBoxChange()
        {
            Box.IsDay = _isDayBox;
            Box.SetMaterialEditor(_isDayBox);
            EditorUtility.SetDirty(Box);
        }
        
        private void CreateBlock()
        {
            EditorUtility.SetDirty(this);
            HaveBlock = true;
            if (BlockGo != null)
            {
                return;
            }

            GameObject blockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ArtResources/Prefabs/block_prefab.prefab");
            BlockGo = PrefabUtility.InstantiatePrefab(blockPrefab, TopPoint) as GameObject;
            BlockGo.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(0,90,0));
            BreakAllRoad();
        }
        
        private void DestroyBlock()
        {
            EditorUtility.SetDirty(this);
            try
            {
                if (BlockGo != null)
                {
                    Utils.DestroyLevelPrefabObj(this, BlockGo.gameObject, out bool isSuccess);
                    if (isSuccess == false)
                        return;
                    BlockGo = null;
                }

                HaveBlock = false;
            }
            catch (Exception e)
            {
                Debug.LogError($"销毁障碍物失败:{e}");
            }
        }
        
        [HideInInspector]
        [SerializeField]
        private bool _haveEndDecorate;
        [HideInInspector]
        [SerializeField]
        private Transform _decorate;
        [HideIf(nameof(IsBound))]
        [HideIf(nameof(HaveBox))]
        [HideIf(nameof(HaveBlock))]
        [HideIf(nameof(HaveDoor))]
        [HideIf(nameof(HaveKey))]
        [HideIf(nameof(Stair))]
        [HideInPlayMode]
        [Button("创建终点装饰")]
        private void CreateEndDecorate()
        {
            try
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ArtResources/Prefabs/Trick_TheGate01_p.prefab");
                GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                go.transform.SetParent(TopPoint);
                go.transform.localPosition=Vector3.zero;
                go.transform.localRotation=Quaternion.identity;
                _decorate = go.transform;
                _haveEndDecorate = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"创建失败:{e}");
                _decorate = null;
                _haveEndDecorate = false;
            }
        }

        [HideIf(nameof(IsBound))]
        [HideIf(nameof(HaveBox))]
        [HideIf(nameof(HaveBlock))]
        [HideIf(nameof(HaveDoor))]
        [HideIf(nameof(HaveKey))]
        [HideIf(nameof(Stair))]
        [ShowIf(nameof(_haveEndDecorate))]
        [HideInPlayMode]
        [Button("旋转终点装饰")]
        private void RotateEndDecorate()
        {
            if (_decorate == null)
            {
                Debug.LogError("旋转失败");
                _haveEndDecorate = false;
                return;
            }
            Vector3 localAngle = _decorate.localEulerAngles;
            localAngle.y += 90;
            _decorate.localEulerAngles = localAngle;
        }

        private void CreateBoxEditor()
        {
            EditorUtility.SetDirty(this);
            HaveBox = true;
            if (Box != null)
            {
                return;
            }

            GameObject boxPrefab = Resources.Load<GameObject>("box");
            GameObject go = PrefabUtility.InstantiatePrefab(boxPrefab) as GameObject;
            go.transform.SetParent(TopPoint);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            Box = go.GetComponent<Box>();
            _isDayBox = IsDay;
            Box.IsDay = _isDayBox;
            Box.SetMaterialEditor(_isDayBox);
            SetStandardName();
        }
        
        private void DestroyBox()
        {
            EditorUtility.SetDirty(this);
            try
            {
                if (Box != null)
                {
                    Utils.DestroyLevelPrefabObj(this, Box.gameObject, out bool isSuccess);
                    if (isSuccess == false)
                        return;
                    Box = null;
                }

                HaveBox = false;
                SetStandardName();
            }
            catch (Exception e)
            {
                Debug.LogError($"销毁箱子失败:{e}");
            }
        }
        
        private void CreateKey()
        {
            EditorUtility.SetDirty(this);
            HaveKey = true;
            if (Key != null)
            {
                return;
            }

            GameObject key = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ArtResources/Prefabs/Trick_Button02_P.prefab");
            GameObject go = PrefabUtility.InstantiatePrefab(key) as GameObject;
            Key = go.GetComponent<Key>();
            go.transform.SetParent(TopPoint);
            go.transform.localPosition = Vector3.zero;
        }

        private void DestroyKey()
        {
            EditorUtility.SetDirty(this);
            try
            {
                if (Key != null)
                {
                    bool success;
                    Utils.DestroyLevelPrefabObj(this, Key.gameObject, out success);
                    if (success == false)
                        return;
                    Key = null;
                }

                HaveKey = false;
            }
            catch (Exception e)
            {
                Debug.LogError($"销毁机关失败:{e}");
            }
        }
 private void CreateDoor()
        {
            EditorUtility.SetDirty(this);
            if (HaveDoor)
                return;
            if (Links.Count != 2)
            {
                Debug.LogError("这个位置不能放门，请检查路径连接");
                return;
            }

            Vector3 linkRoadDir = (Links[0].transform.position - Links[1].transform.position).normalized;
            if (Mathf.Abs(linkRoadDir.x) < 0.9f && Mathf.Abs(linkRoadDir.x) > 0.1f)
            {
                Debug.LogError("这个位置不能放门，请检查路径连接");
                return;
            }

            // todo 判断当前part是否存在其他门

            GameObject doorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ArtResources/Prefabs/door_prefab.prefab");
            GameObject doorGo = PrefabUtility.InstantiatePrefab(doorPrefab) as GameObject;
            HaveDoor = true;
            Door = doorGo.GetComponent<Door>();
            doorGo.transform.SetParent(TopPoint);
            doorGo.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            //设置门的角度
            if (Mathf.Abs(linkRoadDir.x) > 0.9f)
            {
                //阻断x轴方向的路，需要旋转门
                doorGo.transform.localEulerAngles = new Vector3(0, -90, 0);
            }

            // 设置门连接其他key
            Door.CurRoad = this;
            Door.CanLinks = Links;
            Door.Break();
            LevelRoot levelRoot = GetComponentInParent<LevelRoot>();
            Door.KeyRoads = (IsDay ? levelRoot.Part1 : levelRoot.Part2).GetComponentsInChildren<RoadSign>().Where(sign => sign.HaveKey).ToList();

        }

        private void DestoryDoor()
        {
            EditorUtility.SetDirty(this);
            try
            {
                if (HaveDoor == false)
                    return;
                bool success;
                Utils.DestroyLevelPrefabObj(this, Door.gameObject, out success);
                if (success == false)
                    return;
                HaveDoor = false;
                Door.Repair();
            }
            catch (Exception e)
            {
                Debug.LogError($"销毁门失败:{e}");
            }
        }
        [DisableIf(nameof(HaveBlock))]
        [DisableIf(nameof(Stair))]
        [Button("尝试连接周围的路(除了楼梯)")]
        private void TryRepairRoad()
        {
            LevelRoot levelRoot = GetComponentInParent<LevelRoot>();
            RoadSign[] roadSigns = (IsDay ? levelRoot.Part1 : levelRoot.Part2).GetComponentsInChildren<RoadSign>().Where(sign => sign.Stair == false).ToArray();
            int index = 0;
            int count = roadSigns.Length;
            for (var j = 0; j < count; j++)
            {
                index++;
                if (roadSigns[j] == this)
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar("正在自动绑定路径", $"{gameObject.name}->{roadSigns[j].gameObject.name}", index * 1f / count);
                if ((transform.position - roadSigns[j].transform.position).magnitude < 1.1f)
                {
                    //两个道路相连
                    LinkRoad(roadSigns[j]);
                    EditorUtility.SetDirty(this);
                    EditorUtility.SetDirty(roadSigns[j]);
                }
            }

            EditorUtility.ClearProgressBar();
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            for (int index = Links.Count - 1; index >= 0; index--)
            {
                RoadSign sign = Links[index];
                if (sign == null)
                {
                    Links.RemoveAt(index);
                    continue;
                }

                Gizmos.DrawLine(TopPoint.position, (sign.TopPoint.position + TopPoint.position) / 2);
            }

            Gizmos.color = Color.green;
            for (int index = JumpLinks.Count - 1; index >= 0; index--)
            {
                RoadSign sign = JumpLinks[index];
                if (sign == null)
                {
                    JumpLinks.RemoveAt(index);
                    continue;
                }

                Vector3 topPosition = (sign.TopPoint.position + TopPoint.position) / 2;
                topPosition.y = Mathf.Max(TopPoint.position.y, sign.transform.position.y);
                Gizmos.DrawLine(TopPoint.position, topPosition);
                Gizmos.DrawLine(sign.TopPoint.position, topPosition);
            }

            Gizmos.color = Color.red;
            for (int index = JumpFailedLinks.Count - 1; index >= 0; index--)
            {
                RoadSign sign = JumpFailedLinks[index];
                if (sign == null)
                {
                    JumpFailedLinks.RemoveAt(index);
                    continue;
                }

                Vector3 topPosition = (sign.TopPoint.position + TopPoint.position) / 2;
                topPosition.y = Mathf.Max(TopPoint.position.y, sign.transform.position.y);
                Gizmos.DrawLine(TopPoint.position, topPosition);
                Gizmos.DrawLine(sign.TopPoint.position, topPosition);
            }

            // Gizmos.color = Color.magenta;
            // if (SameRoadSign != null)
            // {
            //     Gizmos.DrawSphere(TopPoint.position, 0.4f);
            // }

            Handles.SetCamera(Camera.current);
            Gizmos.color = Color.white;

            if (IsBound)
            {
                Handles.Label(TopPoint.position, "边界", new GUIStyle()
                {
                    alignment = TextAnchor.MiddleCenter,
                    contentOffset = new Vector2(0, 15),
                });
            }

            Handles.Label(TopPoint.position, _index.ToString(), new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20
            });
        }

#endif
    }
}