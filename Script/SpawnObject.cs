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
        protected VRCObjectPool vRCObjectPool;
        [SerializeField, Header("ランダムスポーンをするかどうか")]
        protected bool randomSpawn = false;
        [SerializeField, Header("スポーンアイテムを手元に移動するか")]
        protected bool moveItemToHand = false;
        [SerializeField, Header("オブジェクトの出現先"), Tooltip("未指定の場合はPoolの位置に出現")]
        protected Transform spawnPoint;
        [SerializeField, Header("Spawn Delay"), Tooltip("うまく動かない場合の調整用")]
        protected int spawnDelayFrames = 3;

        protected VRCPlayerApi localPlayer;

        protected void Start()
        {
            if (vRCObjectPool == null)
            {
                Debug.Log("[purabe]VRC Object Poolを登録してください。");
            }
        }

        protected void OnEnable()
        {
            if (randomSpawn)
            {
                //スポーン順序をシャッフル
                vRCObjectPool.Shuffle();
            }

            if (Networking.LocalPlayer != null)
            {
                localPlayer = Networking.LocalPlayer;
            }
        }

        public override void Interact()
        {
            Spawn();
        }

        /// <summary>
        /// スポーン処理
        /// </summary>
        protected virtual void Spawn()
        {
            // スイッチのオーナ権限取得
            GetOwner(this.gameObject);

            // オブジェクトが全てactiveなら操作しない
            if (AllActive())
            {
                Debug.Log("[purabe]スポーンできるオブジェクトがありません。");
                return;
            }

            // Object Pool のオーナ権限取得
            GetOwner(vRCObjectPool.gameObject);
            // オブジェクトプールの配列頭のオブジェクトをスポーン
            GameObject spawnedObject = vRCObjectPool.TryToSpawn();

            if (spawnedObject == null)
            {
                Debug.Log("[purabe]スポーンできるオブジェクトがありません。");
                return;
            }

            // Spawn したアイテムのオーナ権限取得
            GetOwner(spawnedObject);

            // 指定パラメータに従い移動
            MoveToTarget(spawnedObject);

            // SE再生
            PlayAudio();
        }

        /// <summary>
        /// スポーンオブジェクトを指定パラメータに従い移動させる
        /// </summary>
        protected void MoveToTarget(GameObject target)
        {
            if (!moveItemToHand && spawnPoint == null)
            {
                moveTargetGo = null;
                return;
            }

            moveTargetGo = target;

            if (moveItemToHand)
            {
                // 手元に移動させる場合
                if (IsNearToRightHand())
                {
                    toPos = localPlayer.GetBonePosition(HumanBodyBones.RightHand);
                    toRot = Quaternion.identity;
                }
                else
                {
                    toPos = localPlayer.GetBonePosition(HumanBodyBones.LeftHand);
                    toRot = Quaternion.identity;
                }
            }
            else if (spawnPoint != null)
            {
                // 出現ポイントを指定されている場合
                toPos = spawnPoint.position;
                toRot = spawnPoint.rotation;
            }

            // 遅延移動呼出
            SendCustomEventDelayedFrames(nameof(MoveToTargetDelayed), spawnDelayFrames);
        }

        protected GameObject moveTargetGo;
        protected Vector3 toPos;
        protected Quaternion toRot;

        /// <summary>
        /// 移動実施
        /// </summary>
        public void MoveToTargetDelayed()
        {
            if (moveTargetGo == null) return;

            Rigidbody rd = moveTargetGo.GetComponent<Rigidbody>();
            VRCObjectSync sync = moveTargetGo.GetComponent<VRCObjectSync>();

            if (rd != null)
            {
                rd.Sleep();
            }

            if (sync != null && !moveItemToHand
                && spawnPoint != null)
            {
                // VRCObjectSyncで移動
                sync.FlagDiscontinuity();
                sync.TeleportTo(spawnPoint);
            }

            // transform 移動 (VRCObjectSyncがあっても実施)
            moveTargetGo.transform.SetPositionAndRotation(toPos, toRot);

            // 参照クリア
            moveTargetGo = null;
        }

        /// <summary>
        /// すべてのオブジェクトが出現済みかどうか
        /// </summary>
        /// <returns>true:出現済み false:未</returns>
        protected bool AllActive()
        {
            foreach (GameObject item in vRCObjectPool.Pool)
            {
                if (item == null) continue;

                if (!item.activeInHierarchy)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 右手の方が距離が近いかどうか
        /// </summary>
        /// <returns>true:近い false:遠い</returns>
        protected bool IsNearToRightHand()
        {
            Vector3 rightHandPos = localPlayer.GetBonePosition(HumanBodyBones.RightHand);
            Vector3 leftHandPos = localPlayer.GetBonePosition(HumanBodyBones.LeftHand);

            return Vector3.Distance(transform.position, rightHandPos) <= Vector3.Distance(transform.position, leftHandPos);
        }
    }
}
