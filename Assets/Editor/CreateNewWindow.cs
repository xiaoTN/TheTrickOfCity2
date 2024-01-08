using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Trick.Tower;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Trick.Editor
{
    public class CreateNewWindow : OdinEditorWindow
    {
        private const float _x = 1f;
        private const float _y = 1.41f;
        private const float _z = 1f;

        private bool _canBindRoad;
        private bool _canBreakRoad;
        private IDisposable _disposable;

        private bool _isStart;

        private LevelRoot _lastLevel;

        [BoxGroup("创建")]
        [DisableIf(nameof(Lock))]
        [DisableInPlayMode]
        [LabelText("画布尺寸")]
        [Range(3, 10)]
        public int CanvasSizeX;


        [BoxGroup("创建")]
        [DisableIf(nameof(Lock))]
        [DisableInPlayMode]
        [LabelText("第几关")]
        public int CurLevelIndex;

        [ReadOnly]
        [ShowIf(nameof(Lock))]
        [LabelText("当前选中的关卡")]
        [SceneObjectsOnly]
        public LevelRoot CurSelectLevel;


        [ReadOnly]
        [ShowIf(nameof(Lock))]
        [LabelText("当前选中的道路")]
        [SceneObjectsOnly]
        public RoadSign CurSelectRoad;

        [HideInPlayMode]
        [ShowIf(nameof(Lock))]
        [InfoBox("把方块拖进来就能删除了")]
        [LabelText("-------------->")]
        [SceneObjectsOnly]
        [PreviewField(ObjectFieldAlignment.Left)]
        public RoadSign DeleteRoadSign;

        [HideInPlayMode]
        [BoxGroup("更新关卡")]
        [ShowIf(nameof(Lock))]
        [PropertyOrder(10)]
        [LabelText("终点")]
        [OnValueChanged(nameof(OnEndRoadChange))]
        public RoadSign EndRoad;

        [HideInInspector]
        public bool Lock;


        [HideInPlayMode]
        [BoxGroup("更新关卡")]
        [ShowIf(nameof(Lock))]
        [PropertyOrder(10)]
        [LabelText("出生点")]
        [OnValueChanged(nameof(OnStartRoadChange))]
        public RoadSign StartRoad;

        [MenuItem("关卡编辑器/打开")]
        public static void Init()
        {
            CreateNewWindow createNewWindow = GetWindow<CreateNewWindow>();
            createNewWindow.titleContent = new GUIContent("关卡编辑器");
            createNewWindow.Show();
        }

        [BoxGroup("创建")]
        [HideInPlayMode]
        [DisableIf(nameof(Lock))]
        [Button("创建新关卡")]
        private void CreateNewLevel()
        {
            GameObject tempPrefab = Resources.Load<GameObject>($"Levels/Level{CurLevelIndex}");
            if (tempPrefab != null)
            {
                // 判断是否要覆盖
                bool displayDialog = EditorUtility.DisplayDialog("此操作无法撤回", $"检测到已经存在第{CurLevelIndex}关了，是否要覆盖第{CurLevelIndex}关？", "是", "否");
                if (displayDialog == false)
                {
                    return;
                }
            }

            GameObject levelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Levels/LevelDefault.prefab");
            GameObject newLevelGo = PrefabUtility.InstantiatePrefab(levelPrefab, GameObject.FindGameObjectWithTag("LevelRoot").transform) as GameObject;

            // GameObject newLevelGo = GameObject.Instantiate(levelPrefab,GameObject.FindGameObjectWithTag("LevelRoot").transform);
            newLevelGo.gameObject.name = $"Level{CurLevelIndex}";
            newLevelGo.transform.SetLocalPositionAndRotation(new Vector3(4.246216f, 2.797546f, -0.5976563f), Quaternion.Euler(0f, 0f, 0f));
            DestroyImmediate(newLevelGo.GetComponent<LevelRoot>());
            LevelRoot levelRoot = newLevelGo.AddComponent<LevelRoot>();

            foreach (UIExpand uiExpand in newLevelGo.GetComponentsInChildren<UIExpand>())
            {
                uiExpand.XCount = CanvasSizeX;
                uiExpand.YCount = CanvasSizeX;
                uiExpand.Init();
            }

            // 设置每关四个角
            RoadSign roadSign1 = levelRoot.Part1.GetComponentInChildren<RoadSign>();

            // PrefabUtility.InstantiatePrefab(roadSign1,)

            levelRoot.Index = CurLevelIndex;
            PrefabUtility.UnpackPrefabInstance(newLevelGo, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            PrefabUtility.SaveAsPrefabAssetAndConnect(newLevelGo, $"Assets/Resources/Levels/Level{CurLevelIndex}.prefab", InteractionMode.AutomatedAction);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            Selection.activeGameObject = newLevelGo;
        }

        [BoxGroup("创建")]
        [HideInPlayMode]
        [Button("加载新关卡")]
        private void LoadNewLevel()
        {
            GameObject tempPrefab = Resources.Load<GameObject>($"Levels/Level{CurLevelIndex}");
            if (tempPrefab == null)
            {
                Debug.LogError($"不存在第{CurLevelIndex}关");
                return;
            }

            GameObject levelRootGo = GameObject.FindWithTag("LevelRoot");
            GameObject levelGo = PrefabUtility.InstantiatePrefab(tempPrefab) as GameObject;
            levelGo.transform.SetParent(levelRootGo.transform);
            levelGo.transform.localPosition = Vector3.zero;
            Selection.activeGameObject = levelGo;
        }


        private void Update()
        {
            if (Application.isPlaying == false)
            {
                UpdateInEditorMode();
            }
            else
            {
                UpdateInPlayMode();
            }
        }

        private void UpdateInPlayMode()
        {
            CurSelectLevel = LevelManager.Instance.CurLevelRoot;
            if (CurSelectLevel == null)
            {
                Lock = false;
                return;
            }

            Lock = true;
            UpdateCanBindRoad();
            UpdateCanBreakRoad();

            if (_lastLevel != CurSelectLevel)
            {
                CurLevelIndex = CurSelectLevel.Index;
                CanvasSizeX = CurSelectLevel.GetComponentInChildren<UIExpand>().XCount;
            }

            _lastLevel = CurSelectLevel;
        }

        private void UpdateCanBreakRoad()
        {
            if (CurSelectLevel == null)
            {
                return;
            }

            int offsetX = UIManager.Instance.IndexX2 - UIManager.Instance.IndexX1;
            int offsetY = UIManager.Instance.IndexY2 - UIManager.Instance.IndexY1;
            MatchInfo matchInfo = CurSelectLevel.MatchInfos.Find(info => info.XOffset == offsetX && info.YOffset == offsetY);
            _canBreakRoad = matchInfo != null;
        }

        private void UpdateCanBindRoad()
        {
            GameObject[] gameObjects = Selection.gameObjects;
            _canBindRoad = false;
            if (gameObjects.Length != 2)
            {
                return;
            }

            foreach (GameObject gameObject in gameObjects)
            {
                RoadSign roadSign = gameObject.GetComponent<RoadSign>();
                if (roadSign.Stair)
                {
                    return;
                }

                if (roadSign.IsBound == false)
                {
                    return;
                }
            }

            _canBindRoad = true;
        }

        private void UpdateInEditorMode()
        {
            _lastLevel = CurSelectLevel;
            GameObject[] gameObjects = Selection.gameObjects;
            if (gameObjects.Length == 0)
            {
                Lock = false;
                CurSelectLevel = null;
                return;
            }

            GameObject go = gameObjects[0];
            CurSelectLevel = go.GetComponent<LevelRoot>() ?? go.GetComponentInParent<LevelRoot>();
            if (CurSelectLevel == null)
            {
                Lock = false;
                StartRoad = null;
                EndRoad = null;
                return;
            }

            Lock = true;


            if (DeleteRoadSign != null)
            {
                bool success;
                Utils.DestroyLevelPrefabObj(CurSelectLevel, DeleteRoadSign.gameObject, out success);
                if (success)
                {
                    Debug.Log("删除成功");
                }
                else
                {
                    Debug.LogError("删除失败", DeleteRoadSign);
                }

                DeleteRoadSign = null;
            }

            if (_lastLevel != CurSelectLevel)
            {
                OnChangeSelectLevel();
            }

            CurSelectRoad = go.GetComponent<RoadSign>() ?? go.GetComponentInParent<RoadSign>();
        }

        [HorizontalGroup("1")]
        [HideInPlayMode]
        [Button("上", ButtonSizes.Large)]
        private void BtnUp()
        {
            CurSelectRoad.transform.position += Vector3.up * 1.41f;
            EditorUtility.SetDirty(CurSelectRoad.transform);
        }

        [HorizontalGroup("1")]
        [HideInPlayMode]
        [Button("上上", ButtonSizes.Large)]
        private void BtnUpUp()
        {
            BtnUp();
            BtnD();
            BtnS();
            EditorUtility.SetDirty(CurSelectRoad.transform);
        }

        [HorizontalGroup("2")]
        [HideInPlayMode]
        [Button("A", ButtonSizes.Large)]
        private void BtnA()
        {
            CurSelectRoad.transform.position += Vector3.left;
            EditorUtility.SetDirty(CurSelectRoad.transform);
        }

        [HorizontalGroup("2")]
        [HideInPlayMode]
        [Button("W", ButtonSizes.Large)]
        private void BtnW()
        {
            CurSelectRoad.transform.position += Vector3.forward;
            EditorUtility.SetDirty(CurSelectRoad.transform);
        }

        [HorizontalGroup("3")]
        [HideInPlayMode]
        [Button("S", ButtonSizes.Large)]
        private void BtnS()
        {
            CurSelectRoad.transform.position += Vector3.back;
            EditorUtility.SetDirty(CurSelectRoad.transform);
        }

        [HorizontalGroup("3")]
        [HideInPlayMode]
        [Button("D", ButtonSizes.Large)]
        private void BtnD()
        {
            CurSelectRoad.transform.position += Vector3.right;
            EditorUtility.SetDirty(CurSelectRoad.transform);
        }

        [HorizontalGroup("4")]
        [HideInPlayMode]
        [Button("下", ButtonSizes.Large)]
        private void BtnDown()
        {
            CurSelectRoad.transform.position += Vector3.down * 1.41f;
            EditorUtility.SetDirty(CurSelectRoad.transform);
        }

        [HorizontalGroup("4")]
        [HideInPlayMode]
        [Button("下下", ButtonSizes.Large)]
        private void BtnDownDown()
        {
            BtnDown();
            BtnA();
            BtnW();
            EditorUtility.SetDirty(CurSelectRoad.transform);
        }

        private void OnChangeSelectLevel()
        {
            CurLevelIndex = CurSelectLevel.Index;
            CanvasSizeX = CurSelectLevel.GetComponentInChildren<UIExpand>().XCount;
            StartRoad = CurSelectLevel.StartRoadSign;
            EndRoad = CurSelectLevel.EndRoadSign;
        }

        private void OnStartRoadChange()
        {
            CurSelectLevel.StartRoadSign = StartRoad;
        }

        private void OnEndRoadChange()
        {
            CurSelectLevel.EndRoadSign = EndRoad;
        }

        [DisableInPlayMode]
        [BoxGroup("更新关卡")]
        [ShowIf(nameof(Lock))]
        [PropertyOrder(10)]
        [GUIColor(0, 1, 0)]
        [Button("更新当前关卡", ButtonSizes.Large)]
        private void UpdateLevel()
        {
            if (CurSelectLevel == null)
            {
                Debug.LogError("请先选中关卡");
                return;
            }

            CurSelectLevel.StartRoadSign = StartRoad;
            CurSelectLevel.EndRoadSign = EndRoad;
            CurSelectLevel.SetAllName();

            RoadSign[] p1Roads = CurSelectLevel.Part1.GetComponentsInChildren<RoadSign>();
            RoadSign[] p2Roads = CurSelectLevel.Part2.GetComponentsInChildren<RoadSign>();

            #region 设置材质

            // Assets/ArtResources/Textures/Trick_box01/Trick_box01.mat
            // Assets/ArtResources/Textures/Trick_box02_ice01/Trick_box02_ice01.mat
            // Assets/ArtResources/Textures/Trick_Stair01/Trick_Stair01.mat
            // Assets/ArtResources/Textures/Trick_Stair01_ice01/Trick_Stair01_ice01.mat

            foreach (RoadSign roadSign in p1Roads)
            {
                if (roadSign.Stair)
                {
                    roadSign.GetComponent<MeshRenderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtResources/Textures/Trick_Stair01/Trick_Stair01.mat");
                }
                else
                {
                    roadSign.GetComponent<MeshRenderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtResources/Textures/Trick_box01/Trick_box01.mat");
                }
            }

            foreach (RoadSign roadSign in p2Roads)
            {
                if (roadSign.Stair)
                {
                    roadSign.GetComponent<MeshRenderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtResources/Textures/Trick_Stair01_ice01/Trick_Stair01_ice01.mat");
                }
                else
                {
                    roadSign.GetComponent<MeshRenderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtResources/Textures/Trick_box02_ice01/Trick_box02_ice01.mat");
                }
            }

            #endregion

            PrefabUtility.ApplyPrefabInstance(CurSelectLevel.gameObject, InteractionMode.AutomatedAction);
        }

        [PropertyOrder(10)]
        [HideInPlayMode]
        [ShowIf(nameof(Lock))]
        [Button("自动绑定路径")]
        private void AutoBindLinkRoad()
        {
            bool displayDialog = EditorUtility.DisplayDialog("自动绑定路径", "此操作会先清空此关卡之前绑定的所有路径，你确定？？", "确定", "取消");
            if (displayDialog == false)
            {
                return;
            }

            RoadSign[] roads = CurSelectLevel.GetComponentsInChildren<RoadSign>();
            EditorUtility.DisplayProgressBar("正在自动绑定路径", "", 0);
            for (int i = 0; i < roads.Length; i++)
            {
                RoadSign roadSign = roads[i];
                EditorUtility.DisplayProgressBar("正在清理旧的路径", roadSign.gameObject.name, (i + 1f) / roads.Length);
                roadSign.Links.Clear();
            }

            void BindPartLinkRoad(Transform part)
            {
                RoadSign[] roadSigns = part.GetComponentsInChildren<RoadSign>().Where(sign => sign.Stair == false).ToArray();
                int index = 0;
                for (int i = 0; i < roadSigns.Length; i++)
                {
                    for (int j = i + 1; j < roadSigns.Length; j++)
                    {
                        index++;
                        EditorUtility.DisplayProgressBar("正在自动绑定路径", $"{roadSigns[i].gameObject.name}->{roadSigns[j].gameObject.name}", index * 1f / (roads.Length * roads.Length));
                        if ((roadSigns[i].transform.position - roadSigns[j].transform.position).magnitude < 1.1f)
                        {
                            //两个道路相连
                            roadSigns[i].LinkRoad(roadSigns[j]);
                            EditorUtility.SetDirty(roadSigns[i]);
                            EditorUtility.SetDirty(roadSigns[j]);
                        }
                    }
                }
            }

            BindPartLinkRoad(CurSelectLevel.Part1);
            BindPartLinkRoad(CurSelectLevel.Part2);

            EditorUtility.ClearProgressBar();
        }

        // [PropertyOrder(10)]
        // [HideInPlayMode]
        // [ShowIf(nameof(Lock))]
        // [Button("自动绑定推箱子路径")]
        private void AutoBindJumpLinkRoad()
        {
            bool displayDialog = EditorUtility.DisplayDialog("自动绑定推箱子路径", "此操作会先清空此关卡之前绑定的所有路径，你确定？？", "确定", "取消");
            if (displayDialog == false)
            {
                return;
            }

            RoadSign[] roads = CurSelectLevel.GetComponentsInChildren<RoadSign>();
            EditorUtility.DisplayProgressBar("正在自动绑定推箱子路径", "", 0);
            for (int i = 0; i < roads.Length; i++)
            {
                RoadSign roadSign = roads[i];
                EditorUtility.DisplayProgressBar("正在清理旧的路径", roadSign.gameObject.name, (i + 1f) / roads.Length);
                roadSign.JumpLinks.Clear();
            }

            void BindPartJumpLinkRoad(Transform part)
            {
                RoadSign[] roadSigns = part.GetComponentsInChildren<RoadSign>().Where(sign => sign.Stair == false).ToArray();
                int index = 0;
                for (int i = 0; i < roadSigns.Length; i++)
                {
                    for (int j = 0; j < roadSigns.Length; j++)
                    {
                        index++;
                        EditorUtility.DisplayProgressBar("自动绑定推箱子路径", $"{roadSigns[i].gameObject.name}->{roadSigns[j].gameObject.name}", index * 1f / (roads.Length * roads.Length));

                        //两个道路相连
                        roadSigns[i].CheckNearVisiableBottomRoad(roadSigns[j]);
                        EditorUtility.SetDirty(roadSigns[i]);
                        EditorUtility.SetDirty(roadSigns[j]);
                    }
                }
            }

            BindPartJumpLinkRoad(CurSelectLevel.Part1);
            BindPartJumpLinkRoad(CurSelectLevel.Part2);

            EditorUtility.ClearProgressBar();
        }

        [EnableIf(nameof(_canBindRoad))]
        [DisableInEditorMode]
        [PropertyOrder(20)]
        [Button("绑定光暗道路")]
        private void BindTwoRoad()
        {
            CurSelectLevel.Save();
            CurSelectLevel.TryLinkAllSameRoad();
        }

        [DisableInEditorMode]
        [PropertyOrder(20)]
        [Button("保存当前画布位置为初始状态")]
        private void SaveStartViewIndex()
        {
            UIDrag[] drags = CurSelectLevel.GetComponentsInChildren<UIDrag>(true);
            foreach (UIDrag uiDrag in drags)
            {
                if (uiDrag.IsFirstTexture)
                {
                    CurSelectLevel.StartView1IndexX = uiDrag.XIndex.Value;
                    CurSelectLevel.StartView1IndexY = uiDrag.YIndex.Value;
                }
                else
                {
                    CurSelectLevel.StartView2IndexX = uiDrag.XIndex.Value;
                    CurSelectLevel.StartView2IndexY = uiDrag.YIndex.Value;
                }
            }

            CurSelectLevel.SaveStartIndex();
        }

        [EnableIf(nameof(_canBreakRoad))]
        [DisableInEditorMode]
        [PropertyOrder(20)]
        [Button("删除光暗连接")]
        private void BreakTwoRoad()
        {
            int offsetX = UIManager.Instance.IndexX2 - UIManager.Instance.IndexX1;
            int offsetY = UIManager.Instance.IndexY2 - UIManager.Instance.IndexY1;
            CurSelectLevel.DeleteMatchInfo(offsetX, offsetY);
            CurSelectLevel.TryLinkAllSameRoad();
        }

        [PropertyOrder(30)]
        [DisableInEditorMode]
        [Button("进入第几关")]
        private void EnterLevel(int index)
        {
            LevelManager.Instance.EnterLevel(index);
        }

        [DisableIf("_isStart")]
        [DisableInEditorMode]
        [Button("自动测试当前关卡")]
        private void AutoTest([LabelText("操作间隔")]float interval=0.25f)
        {
            _disposable = Test(CurLevelIndex,interval).ToObservable().DoOnCompleted(() =>
            {
                _isStart = false;
            }).Subscribe();
            _isStart = true;
        }

        [EnableIf("_isStart")]
        [DisableInEditorMode]
        [Button("停止测试")]
        private void StopTest()
        {
            _isStart = false;
            _disposable?.Dispose();
        }

        private IEnumerator Test(int levelIndex,float interval)
        {
            UIDrag[] drags = CurSelectLevel.GetComponentsInChildren<UIDrag>();
            string[] readAllLines = File.ReadAllLines(Path.Combine(Application.dataPath, "Tests", $"{levelIndex}.txt"));
            int index = 0;
            foreach (string line in readAllLines)
            {
                index++;
                Debug.Log($"【自动测试{index}】: {line}");
                if (Regex.IsMatch(line, @"[WSAD]"))
                {
                    MessageBroker.Default.Publish(line);
                }
                else
                {
                    Match match = Regex.Match(line, @"(\d+),(\d+)\|(\d+),(\d+)");
                    if (match.Success)
                    {
                        while (UIManager.Instance.CanDrag==false)
                        {
                            Debug.Log("Can't Drag!");
                            yield return null;
                        }
                        UIDrag uiDrag1 = drags.First(drag => drag.gameObject.name.Equals("View1"));
                        UIDrag uiDrag2 = drags.First(drag => drag.gameObject.name.Equals("View2"));
                        uiDrag1.AdjustUIToBound(int.Parse(match.Groups[1].ToString()), int.Parse(match.Groups[2].ToString()));
                        uiDrag1.OnEndDrag(null);
                        uiDrag2.AdjustUIToBound(int.Parse(match.Groups[3].ToString()), int.Parse(match.Groups[4].ToString()));
                        uiDrag2.OnEndDrag(null);
                    }
                    else
                    {
                        if (line.Equals("delay"))
                        {
                            yield return new WaitForSeconds(3);
                            continue;
                        }
                    }
                }

                yield return new WaitForSeconds(interval);
            }
        }
        
        [BoxGroup("魔塔")]
        [Button("创建怪物")]
        private void CreateTowerMonster(string name,int hp,int attack,int defend,[PreviewField]Sprite sprite1,[PreviewField]Sprite sprite2)
        {
            GameObject monsterGo = new GameObject(name);
            SpriteRenderer sr = monsterGo.AddComponent<SpriteRenderer>();
            sr.sprite = sprite1;
            sr.sortingOrder = 1;
            Monster monster = monsterGo.AddComponent<Monster>();
            monster.Name = name;
            monster.HP = hp;
            monster.Attack = attack;
            monster.Defend = defend;
            monster.sprite1 = sprite1;
            monster.sprite2 = sprite2;
            
            string path = $"Assets/Resources/Tower/Monster/{name}.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(monsterGo, path, InteractionMode.AutomatedAction, out bool isSuccess);
            if (isSuccess)
            {
                Debug.Log($"保存成功:{path}");
            }
            DestroyImmediate(monsterGo);
        }
        [BoxGroup("魔塔")]
        [Button("创建道具")]
        private void CreateTowerMonster(string name, int hp, int attack, int defend, [PreviewField] Sprite sprite)
        {
            GameObject monsterGo = new GameObject(name);
            SpriteRenderer sr = monsterGo.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 1;
            Item item = monsterGo.AddComponent<Item>();
            item.Name = name;
            item.HP = hp;
            item.Attack = attack;
            item.Defend = defend;
            
            string path = $"Assets/Resources/Tower/Item/{name}.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(monsterGo, path, InteractionMode.AutomatedAction, out bool isSuccess);
            if (isSuccess)
            {
                Debug.Log($"保存成功:{path}");
            }
            DestroyImmediate(monsterGo);
        }

        [DisableInEditorMode]
        [Button("清空数据，重新开始")]
        private void ClearData()
        {
            LevelManager.Instance.ClearData();
            Debug.Log("数据已清空，请重新运行");
        }
    }
}