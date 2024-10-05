
using UdonSharp;
using UnityEngine;

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
            allReseter.SendCustomNetworkEvent(
                VRC.Udon.Common.Interfaces.NetworkEventTarget.All,
                nameof(allReseter.ResetAll));
        }
    }
}