using System;
using UnityEngine;

public abstract class MonoSingleton<T>: MonoBehaviour where T: MonoSingleton<T>
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if(_instance==null)
                {
                    GameObject go = new GameObject("MonoSingle " + typeof(T).FullName);
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<T>();
                }
                _instance.SetUp();
            }
            return _instance;
        }
    }

    /// <summary>
    /// 初始化接口
    /// </summary>
    protected virtual void SetUp()
    {
    }

    public virtual void Init()
    {
        //nothing
    }

    protected virtual void OnDestroy()
    {
        
    }
}