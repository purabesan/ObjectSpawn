
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace PurabeWorks.SpawnObject
{
    /// <summary>
    /// リセットボタン
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ResetSwitch : UdonSharpBehaviour
    {
        [SerializeField] private ReturnObject allReseter;

        public override void Interact()
        {
            // オーナ権限獲得
            SetOwner(this.gameObject);
            SetOwner(allReseter.gameObject);

            // リセット実行
            allReseter.ResetAll();
        }

        /// <summary>
        /// オーナー権限獲得
        /// </summary>
        /// <param name="obj">対象オブジェクト</param>
        private void SetOwner(GameObject obj)
        {
            if (!Networking.IsOwner(obj))
            {
                Networking.SetOwner(Networking.LocalPlayer, obj);
            }
        }
    }
}
