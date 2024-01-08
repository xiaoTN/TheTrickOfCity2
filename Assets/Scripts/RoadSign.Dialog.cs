using Sirenix.OdinInspector;
using UniRx;
using UniRx.Triggers;

namespace Trick
{
    public partial class RoadSign
    {
        [LabelText("对话框")]
        public int DialogIndex = -1;
        [LabelText("对话框(魔塔)")]
        public int DialogIndexTower = -1;
        private void InitDialog()
        {
            // if (DialogIndex == -1)
            // {
            //     return;
            // }
            // HavePlayer.FirstOrDefault(b => b).Subscribe(b =>
            // {
            //     gameObject.UpdateAsObservable()
            //               .First(unit => Player.Instance.IsStop)
            //               .Subscribe(unit =>
            //               {
            //                   StartCoroutine(DialogManager.Instance.Dia(DialogIndex));
            //               });
            // }).AddTo(this);
        }
    }
}