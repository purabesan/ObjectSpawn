// Copyright (c) 2026 Purabe Works
// Released under the MIT License. See LICENSE.txt for details.
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
            if (allReseter == null)
            {
                Debug.LogError("[purabe]ResetSwitch: allReseterを登録してください。");
                return;
            }

            // オーナ権限獲得
            if (!SetOwner(this.gameObject) || !SetOwner(allReseter.gameObject)) return;

            // リセット実行
            allReseter.ResetAll();
        }

        /// <summary>
        /// オーナー権限獲得
        /// </summary>
        /// <param name="obj">対象オブジェクト</param>
        /// <returns>所有権の取得処理を実行できた場合はtrue、それ以外はfalse</returns>
        private bool SetOwner(GameObject obj)
        {
            if (obj == null) return false;

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (!Utilities.IsValid(localPlayer)) return false;

            if (!Networking.IsOwner(obj))
            {
                Networking.SetOwner(localPlayer, obj);
            }
            return true;
        }
    }
}
