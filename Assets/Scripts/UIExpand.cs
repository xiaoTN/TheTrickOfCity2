using Sirenix.OdinInspector;
using UnityEngine;

namespace Trick
{
    public class UIExpand : MonoBehaviour
    {
        [LabelText("横着有几格？")]
        public int XCount=3;
        [LabelText("竖着有几格？")]
        public int YCount=3;

        private float _uiWeight;
        private float _uiHeight;
        
        [Button]
        public void Init()
        {
            _uiWeight = UIManager.XAdsord;
            _uiHeight = UIManager.YAdsord;
            
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta=new Vector2(XCount*_uiWeight,YCount*_uiHeight);
        }
        
        
    }
}