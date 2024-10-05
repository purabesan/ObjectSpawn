using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace PurabeWorks.SpawnObject
{
    /// <summary>
    /// Spawn処理
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SpawnObject : CommonSpawnObject
    {
        [SerializeField, Header("スポーン対象のVRC Object Pool")]
        private VRCObjectPool _vRCObjectPool;
        [SerializeField, Header("ランダムスポーンをするかどうか")]
        private bool _randomSpawn = false;
        [SerializeField, Header("スポーンアイテムを手元に移動するか")]
        private bool _moveItemToHand = false;
        [SerializeField, Header("オブジェクトの出現先"), Tooltip("未指定の場合はPoolの位置に出現")]
        private Transform _spawnPoint;


        private VRCPlayerApi localPlayer;

        private void Start()
        {
            if (_vRCObjectPool == null)
            {
                Debug.Log("[purabe]VRC Object Poolを登録してください。");
            }

            if (_randomSpawn)
            {
                //スポーン順序をシャッフル
                _vRCObjectPool.Shuffle();
            }

            if (Networking.LocalPlayer != null)
            {
                localPlayer = Networking.LocalPlayer;
            }
        }

        /// <summary>
        /// すべてのオブジェクトが出現済みかどうか
        /// </summary>
        /// <returns>true:出現済み false:未</returns>
        private bool AllActive()
        {
            foreach (GameObject item in _vRCObjectPool.Pool)
            {
                if (!item.activeInHierarchy)
                {
                    return false;
                }
            }
            return true;
        }

        public override void Interact()
        {
            // オブジェクトが全てactiveなら操作しない
            if (AllActive())
            {
                Debug.Log("[purabe]スポーンできるオブジェクトがありません。");
                return;
            }

            // このスクリプトを実行しているプレイヤーが「オーナ」でなければ「オーナ」にする
            SetOwner(_vRCObjectPool.gameObject);
            // オブジェクトプールの配列頭のオブジェクトをスポーン
            GameObject spawnedObject = _vRCObjectPool.TryToSpawn();

            if (spawnedObject == null)
            {
                Debug.Log("[purabe]スポーンできるオブジェクトがありません。");
                return;
            }

            // オーナ権限取得
            SetOwner(spawnedObject);

            // 手元に移動させる
            if (_moveItemToHand)
            {
                if (IsNearToRightHand())
                {
                    spawnedObject.transform.position = localPlayer.GetBonePosition(HumanBodyBones.RightHand);
                }
                else
                {
                    spawnedObject.transform.position = localPlayer.GetBonePosition(HumanBodyBones.LeftHand);
                }
            }
            else if (_spawnPoint)
            {
                // 出現ポイントを指定されている場合
                spawnedObject.transform.SetPositionAndRotation(_spawnPoint.position, _spawnPoint.rotation);
            }

            // SE再生
            PlayAudio();
        }



        /// <summary>
        /// 右手の方が距離が近いかどうか
        /// </summary>
        /// <returns>true:近い false:遠い</returns>
        private bool IsNearToRightHand()
        {
            Vector3 rightHandPos = localPlayer.GetBonePosition(HumanBodyBones.RightHand);
            Vector3 leftHandPos = localPlayer.GetBonePosition(HumanBodyBones.LeftHand);

            return Vector3.Distance(transform.position, rightHandPos) <= Vector3.Distance(transform.position, leftHandPos);
        }


    }
}