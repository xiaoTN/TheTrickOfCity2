using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Trick
{
    public partial class Box
    {
        [HideInEditorMode]
        [Button("模拟推箱子（跳）")]
        public void PushJumpBox(RoadSign targetRoad)
        {
            Vector3 p3 = targetRoad.TopPoint.position;
            transform.DoJump(p3,1f,0.3f).OnComplete(() =>
            {
                AudioManager.Instance.PlayAudio("箱子落地");

            });

            SetToRoad(targetRoad);
        }
        
        
        /// <summary>
        /// 推到虚空下
        /// </summary>
        [HideInEditorMode]
        [Button("推到虚空下（跳）")]
        public void PushJumpToEmpty(KeyOp keyOp)
        {
            Sequence seq = DOTween.Sequence();
            Vector3 targetLocalPos = keyOp.GetDirByKeyOp() + transform.position;
            Vector3 p3 = targetLocalPos;
            SetToRoad(null);
            Tweener t1=transform.DoJump(p3,1f,0.3f);
            var t2 = DOTween.To(() => p3, x => transform.position = x, p3 + Vector3.down * 15, 0.7f).SetEase(Ease.Linear).OnComplete(() =>
            {
                // todo 掉入虚空声音
                // AudioManager.Instance.PlayAudio("箱子落地");
                // DestroyImmediate(tempBox.gameObject);
            });
            seq.Append(t1).Append(t2).SetTarget(this);
        }
    }
}