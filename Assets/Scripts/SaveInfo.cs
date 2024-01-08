using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Trick
{
    [Serializable]
    public class SaveInfo
    {
        public TowerPlayerInfo TowerPlayerInfo;
        public List<TowerRoadInfo> TowerRoadInfos=new List<TowerRoadInfo>(16);
        public List<RoadInfo> LevelInfos=new List<RoadInfo>(16);
    }
    
    [Serializable]
    public class TowerPlayerInfo
    {
        public bool HaveSword;
        public int HP;
        public int Attack;
        public int Defend;
        public int Key0;
        public int Key1;

    }
    
    [Serializable]
    public class LevelInfo
    {
        public int Index;
        public int Count;
        public int Timer;
    }

    [Serializable]
    public class RoadInfo
    {
        public int Index;
        public int Count;
        public int Timer;
        
        public int X1;
        public int Y1;
        public int X2;
        public int Y2;
        
        [TableList]
        public List<Tuple<int, int>> BoxInfos=new List<Tuple<int, int>>();
    }

    [Serializable]
    public class TowerRoadInfo
    {
        public List<int> HaveDos=new List<int>();
    }

    [Serializable]
    public class Config
    {
        public int Volume;
        public bool Music;
        public bool Acoustics;
        public float GameSpeed;
        public string KeyboardUp;
        public string KeyboardDown;
        public string KeyboardLeft;
        public string KeyboardRight;

[ShowInInspector]        public bool? DragGeoplane;
[ShowInInspector]        public bool? Restart;
[ShowInInspector]        public bool? Retract;
[ShowInInspector]        public bool? BorderBlink;
[ShowInInspector]        public int? EnterLevel;

[ShowInInspector]        public bool? MonsterAttributes;
[ShowInInspector]        public bool? ReadArchive;
[ShowInInspector]        public bool? CombatPrediction;
[ShowInInspector]        public int? PlayThrough;

        private const string Url = "游戏主页：http://139.198.17.147:8288/";
        [Button]
        public void SaveGame()
        {
            Debug.Log("写入配置文件");
            string configPath = Path.Combine(Application.streamingAssetsPath,"Data", "Config.txt");

            StringBuilder sb=new StringBuilder();
            sb.AppendLine($"{nameof(Volume)}={Volume}");
            sb.AppendLine($"{nameof(Music)}={Music}");
            sb.AppendLine($"{nameof(Acoustics)}={Acoustics}");
            sb.AppendLine($"{nameof(GameSpeed)}={GameSpeed}");
            sb.AppendLine(EnterLevel==null? string.Empty:$"{nameof(EnterLevel)}={EnterLevel}");
            sb.AppendLine(PlayThrough==null? string.Empty:$"{nameof(PlayThrough)}={PlayThrough}");
            sb.AppendLine($"{nameof(KeyboardUp)}={KeyboardUp}");
            sb.AppendLine($"{nameof(KeyboardDown)}={KeyboardDown}");
            sb.AppendLine($"{nameof(KeyboardLeft)}={KeyboardLeft}");
            sb.AppendLine($"{nameof(KeyboardRight)}={KeyboardRight}");
            sb.AppendLine();
            sb.AppendLine(DragGeoplane==null? string.Empty:$"{nameof(DragGeoplane)}={DragGeoplane}");
            sb.AppendLine(Restart==null? string.Empty:$"{nameof(Restart)}={Restart}");
            sb.AppendLine(Retract==null? string.Empty:$"{nameof(Retract)}={Retract}");
            sb.AppendLine(BorderBlink==null? string.Empty:$"{nameof(BorderBlink)}={BorderBlink}");
            sb.AppendLine();
            sb.AppendLine(MonsterAttributes==null? string.Empty:$"{nameof(MonsterAttributes)}={MonsterAttributes}");
            sb.AppendLine(ReadArchive==null? string.Empty:$"{nameof(ReadArchive)}={ReadArchive}");
            sb.AppendLine(CombatPrediction==null? string.Empty:$"{nameof(CombatPrediction)}={CombatPrediction}");
            sb.AppendLine(Url);
            
            File.WriteAllText(configPath,sb.ToString());
        }

        [Button]
        public void ReadGame()
        {
            Debug.Log("读配置文件");
            try
            {
                string configPath = Path.Combine(Application.streamingAssetsPath,"Data", "Config.txt");

                string[] lines = File.ReadAllLines(configPath);
                foreach (string line in lines)
                {
                    if (line.IsNullOrWhitespace())
                        continue;
                    {
                        Match match = Regex.Match(line, $@"^{nameof(Volume)}\s*=\s*(.+)\s*?#?");
                        if (match.Success)
                            Volume = int.Parse(match.Groups[1].Value);
                    }
                    {
                        Match match = Regex.Match(line, $@"^{nameof(Music)}\s*=\s*(.+)\s*?#?");
                        if (match.Success)
                            Music = bool.Parse(match.Groups[1].Value);
                    }
                    {
                        Match match = Regex.Match(line, $@"^{nameof(Acoustics)}\s*=\s*(.+)\s*?#?");
                        if (match.Success)
                            Acoustics = bool.Parse(match.Groups[1].Value);
                    }
                    {
                        Match match = Regex.Match(line, $@"^{nameof(GameSpeed)}\s*=\s*(.+)\s*?#?");
                        if (match.Success)
                        {
                            GameSpeed = float.Parse(match.Groups[1].Value);
                            GameSpeed = Mathf.Clamp(GameSpeed, 0.5f, 1.5f);
                        }
                    }
                    {
                        Match match = Regex.Match(line, $@"^{nameof(KeyboardUp)}\s*=\s*(.+)\s*?#?");
                        if (match.Success)
                            KeyboardUp = (match.Groups[1].Value);
                    }
                    {
                        Match match = Regex.Match(line, $@"^{nameof(KeyboardDown)}\s*=\s*(.+)\s*?#?");
                        if (match.Success)
                            KeyboardDown = (match.Groups[1].Value);
                    }
                    {
                        Match match = Regex.Match(line, $@"^{nameof(KeyboardLeft)}\s*=\s*(.+)\s*?#?");
                        if (match.Success)
                            KeyboardLeft = (match.Groups[1].Value);
                    }
                    {
                        Match match = Regex.Match(line, $@"^{nameof(KeyboardRight)}\s*=\s*(.+)\s*?#?");
                        if (match.Success)
                            KeyboardRight = (match.Groups[1].Value);
                    }

                    {
                        Match match = Regex.Match(line, $@"^{nameof(DragGeoplane)}\s*=\s*(.+)\s*?#?");
                        if (match.Success)
                            DragGeoplane = bool.Parse(match.Groups[1].Value);
                    }
                    {
                        Match match = Regex.Match(line, $@"^{nameof(Restart)}\s*=\s*(.+)\s*?#?");
                        if (match.Success)
                            Restart = bool.Parse(match.Groups[1].Value);
                    }
                    {
                        Match match = Regex.Match(line, $@"^{nameof(Retract)}\s*=\s*(.+)\s*?#?");
                        if (match.Success)
                            Retract = bool.Parse(match.Groups[1].Value);
                    }
                    {
                        Match match = Regex.Match(line, $@"^{nameof(BorderBlink)}\s*=\s*(.+)\s*?#?");
                        if (match.Success)
                            BorderBlink = bool.Parse(match.Groups[1].Value);
                    }
                    {
                        Match match = Regex.Match(line, $@"^{nameof(EnterLevel)}\s*=\s*(.+)\s*?#?");
                        if (match.Success)
                            EnterLevel = int.Parse(match.Groups[1].Value);
                    }

                    {
                        Match match = Regex.Match(line, $@"^{nameof(MonsterAttributes)}\s*=\s*(.+)\s*?#?");
                        if (match.Success)
                            MonsterAttributes = bool.Parse(match.Groups[1].Value);
                    }
                    {
                        Match match = Regex.Match(line, $@"^{nameof(ReadArchive)}\s*=\s*(.+)\s*?#?");
                        if (match.Success)
                            ReadArchive = bool.Parse(match.Groups[1].Value);
                    }
                    {
                        Match match = Regex.Match(line, $@"^{nameof(CombatPrediction)}\s*=\s*(.+)\s*?#?");
                        if (match.Success)
                            CombatPrediction = bool.Parse(match.Groups[1].Value);
                    }
                    {
                        Match match = Regex.Match(line, $@"^{nameof(PlayThrough)}\s*=\s*(.+)\s*?#?");
                        if (match.Success)
                            PlayThrough = int.Parse(match.Groups[1].Value);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"配置文件错误:{e}");
                return;
            }

            AudioManager.Instance.EnableMusicBg(Music);
            AudioManager.Instance.EnableAcoustics(Acoustics);
            AudioManager.Instance.SetMusicVolumn(Volume);
            Time.timeScale = GameSpeed;
            
            GameRoot.Instance.ShowReback(Retract == null ? false : (bool) Retract);
            GameRoot.Instance.ShowRestart(Restart == null ? false : (bool) Restart);
            UIManager.Instance.UltimateCanDrag = DragGeoplane == null ? false : (bool)DragGeoplane;
            if (EnterLevel != null)
            {
                int levelIndex = (int) EnterLevel;
                if (LevelManager.Instance.CurLevelRoot != null
                    && LevelManager.Instance.CurLevelRoot.Index != levelIndex)
                {
                    LevelManager.Instance.EnterLevel(levelIndex);
                }
            }

            if (PlayThrough != null)
            {
                int playThrough = (int) PlayThrough;
                if(LevelManager.Instance.CurLevelRoot!=null
                   &&LevelManager.Instance.CurLevelRoot.InTower.Value)
                {
                    UIManager.Instance.TowerPanelInstance.UpdateUI();
                }
            }
            else
            {
                PlayThrough = 0;
            }
        }
    }
}