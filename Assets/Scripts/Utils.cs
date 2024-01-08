using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DG.Tweening;
using Trick;
using UniRx;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


public static class Utils
{
    public static void SetLocalPositionAndRotation(this Transform root, Vector3 localPos, Quaternion localQua)
    {
        root.localPosition = localPos;
        root.localRotation = localQua;
    }

#if UNITY_EDITOR
    public static void DestroyLevelPrefabObj(RoadSign roadSign, GameObject obj, out bool isSuccess)
    {
        LevelRoot levelRoot = roadSign.GetComponentInParent<LevelRoot>();
        DestroyLevelPrefabObj(levelRoot, obj, out isSuccess);
    }

    public static void DestroyLevelPrefabObj(LevelRoot levelRoot, GameObject obj, out bool isSuccess)
    {
        try
        {
            GameObject.DestroyImmediate(obj);
            isSuccess = true;
        }
        catch (Exception e)
        {
            // 无法销毁,尝试通过预制体销毁
            bool displayDialog = EditorUtility.DisplayDialog("此操作会触发保存关卡", "你确定吗？", "是", "否");
            if (displayDialog == false)
            {
                isSuccess = false;
                return;
            }

            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(levelRoot.gameObject);
            PrefabUtility.UnpackPrefabInstance(levelRoot.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            GameObject.DestroyImmediate(obj);
            PrefabUtility.SaveAsPrefabAssetAndConnect(levelRoot.gameObject, prefabPath, InteractionMode.AutomatedAction, out bool success);
            if (success)
            {
                isSuccess = true;
                Debug.Log("保存成功");
            }
            else
            {
                isSuccess = false;
            }
        }
    }
#endif

    public static Vector3 GetDirByKeyOp(this KeyOp keyOp)
    {
        switch (keyOp)
        {
            case KeyOp.W:
                return Vector3.forward;
                break;
            case KeyOp.A:
                return Vector3.left;
                break;
            case KeyOp.S:
                return Vector3.back;
                break;
            case KeyOp.D:
                return Vector3.right;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(keyOp), keyOp, null);
        }
    }

    public static KeyOp Revert(this KeyOp keyOp)
    {
        switch (keyOp)
        {
            case KeyOp.W:
                return KeyOp.S;
                break;
            case KeyOp.A:
                return KeyOp.D;
                break;
            case KeyOp.S:
                return KeyOp.W;
                break;
            case KeyOp.D:
                return KeyOp.A;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(keyOp), keyOp, null);
        }
    }

    public static KeyOp GetKeyOpByVector3(Vector3 dir)
    {
        dir.y = 0;
        dir.Normalize();
        if (dir.Equals(Vector3.zero) == false)
        {
            if (dir.z > 0.5f)
            {
                return KeyOp.W;
            }

            if (dir.z < -0.5f)
            {
                return KeyOp.S;
            }

            if (dir.x > 0.5f)
            {
                return KeyOp.D;
            }

            if (dir.x < -0.5f)
            {
                return KeyOp.A;
            }
        }

        throw new Exception("方向不能是zero");
    }

    public static Tweener DoJump(this Transform trans, Vector3 target, float jumpHeight, float duration)
    {
        Vector3 p1 = trans.position;
        Vector3 p3 = target;
        Vector3 p2 = (p1 + p3) / 2;
        p2.y = Mathf.Max(p1.y, p3.y) + jumpHeight;

        return trans.DOPath(new[]
        {
            p1,
            p2,
            p3
        }, duration, PathType.CatmullRom);
    }

    public static void SetLayers(this GameObject go,string layerName)
    {
        Transform[] children = go.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            child.gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }
    public static void SetLayers(this GameObject go,int layerIndex)
    {
        // Transform[] children = go.GetComponentsInChildren<Transform>(true);
        // foreach (Transform child in children)
        // {
        //     child.gameObject.layer = layerIndex;
        // }
    }
    
    public static void OpenFile(string fileName,string content)
    {
#if UNITY_EDITOR
        string filePath = $"Assets\\StreamingAssets\\Data\\{fileName}.txt";
#elif PLATFORM_STANDALONE
        string filePath = $"TheCityOfTrick_Data\\StreamingAssets\\Data\\{fileName}.txt";
#endif
        string directoryName = Path.GetDirectoryName(filePath);
        Directory.CreateDirectory(directoryName);
        if (File.Exists(filePath) == false)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.CreateNew))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(content);
                }
            }
        }

        Observable.ReturnUnit()
                  .SubscribeOn(Scheduler.ThreadPool)
                  .Subscribe(unit =>
                  {
                      DllUtils.MessageBox(IntPtr.Zero,  $"检测到未知数据记录，已存放在：{filePath}","用户65535：", 0);
                      System.Diagnostics.Process.Start("Explorer.exe", $"/Select, {filePath}");
                  });
    }
}