using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Trick.Tower.UI
{
    public class TowerPanel: MonoBehaviour
    {
        public MonsterPanel MonsterPanelPrefab;
        public Text YongHu;
        public Image PlayerHead;
        public Text CengText;
        public Text HPText;
        public Text AttackText;
        public Text DefendText;
        public Text Key0Text;
        public Text Key1Text;

        [HideInEditorMode]
        [Button]
        public void UpdateUI()
        {
            UpdateUI(LevelManager.Instance.CurLevelRoot);
        }

        public Sprite DaySprite;
        public Sprite NightSprite;
        private List<MonsterPanel> _monsterPanelInstances=new List<MonsterPanel>();
        [HideInEditorMode]
        [Button]
        private void UpdateUI(LevelRoot levelRoot)
        {
            YongHu.text = $"用户{LevelManager.Instance.Config.PlayThrough:00000}";
            PlayerHead.sprite = LevelManager.Instance.PlayerIsDayInTrick ? DaySprite : NightSprite;
            TowerPlayerInfo towerPlayerInfo = LevelManager.Instance.SaveInfo.TowerPlayerInfo;
            CengText.text =(17- LevelManager.Instance.CurIndex).ToString();
            HPText.text = towerPlayerInfo.HP.ToString();
            AttackText.text = towerPlayerInfo.Attack.ToString();
            DefendText.text = towerPlayerInfo.Defend.ToString();
            Key0Text.text = towerPlayerInfo.Key0.ToString();
            Key1Text.text = towerPlayerInfo.Key1.ToString();

            MonsterPanelPrefab.gameObject.SetActive(false);
            foreach (MonsterPanel monsterPanelInstance in _monsterPanelInstances)
            {
                Destroy(monsterPanelInstance.gameObject);
            }
            _monsterPanelInstances.Clear();
            List<Monster> allMonstersInLevel = levelRoot.GetAllMonstersInLevel();
            foreach (Monster monster in allMonstersInLevel)
            {
                MonsterPanel monsterPanel = Instantiate(MonsterPanelPrefab, MonsterPanelPrefab.transform.parent);
                _monsterPanelInstances.Add(monsterPanel);
                monsterPanel.gameObject.SetActive(true);
                monsterPanel.UpdatePanel(monster);
            }
         
        }
    }
}