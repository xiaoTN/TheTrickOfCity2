using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Common.Json;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Trick
{
    public class DialogManager: MonoSingleton<DialogManager>
    {
        public List<DialogInfo> DialogInfos;
        
        public override void Init()
        {
            base.Init();
            DialogInfos = JsonUtils.ReadJson<List<DialogInfo>>("Dialog");
        }

        [Button]
        private void DiaTest(int id)
        {
            StartCoroutine(Dia(id));
        }

        public bool Dialoging;
        public IEnumerator Dia(int id)
        {
            Dialoging = true;
            DialogInfo dialogInfo = DialogInfos.Find(info => int.Parse(info.ID).Equals(id));
            foreach (MsgInfo info in dialogInfo.Msgs)
            {
                string msg = info.Content;
                GameObject textGo = UIManager.Instance.CreateDialog(info.IsDay,LevelManager.Instance.CurLevelRoot.InTower.Value, msg, info.Duration);
                float timer = 0;
                while (true)
                {
                    if (Input.anyKeyDown)
                    {
                        // Destroy(textGo);
                        Debug.Log("跳过");
                        break;
                    }
                    yield return null;
                    timer += Time.deltaTime;
                    if (timer > info.Duration)
                    {
                        break;
                    }
                }
                Destroy(textGo);
                yield return null;
            }

            Dialoging = false;
            UIManager.Instance.CloseDia();
        }

    }

    [Serializable]
    public class DialogInfo
    {
        public string ID;
        public bool AutoPlayer;
        public List<MsgInfo> Msgs;
    }

    [Serializable]
    public class MsgInfo
    {
        public int IsDay;
        public string Content;
        public float Duration;
        public MsgInfo(string content)
        {
            Content = content;
        }
    }
}