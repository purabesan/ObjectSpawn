// Copyright (c) 2026 Purabe Works
// Released under the MIT License. See LICENSE.txt for details.
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
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
        [FormerlySerializedAs("_vRCObjectPool")]
        private VRCObjectPool vRCObjectPool;
        [SerializeField, Header("ランダムスポーンをするかどうか")]
        [FormerlySerializedAs("_randomSpawn")]
        private bool randomSpawn = false;
        [SerializeField, Header("スポーンアイテムを手元に移動するか")]
        [FormerlySerializedAs("_moveItemToHand")]
        private bool moveItemToHand = false;
        [SerializeField, Header("オブジェクトの出現先"), Tooltip("未指定の場合はPoolの位置に出現")]
        private Transform spawnPoint;
        [SerializeField, Header("Spawn Delay"), Tooltip("うまく動かない場合の調整用")]
        private int spawnDelayFrames = 3;

        protected VRCPlayerApi localPlayer;

        /* 拡張パック対応用 */
        protected VRCObjectPool VRCObjectPool => vRCObjectPool;
        protected bool RandomSpawn => randomSpawn;
        protected bool MoveItemToHand => moveItemToHand;
        protected Transform SpawnPoint => spawnPoint;
        protected int SpawnDelayFrames => spawnDelayFrames;

        protected void Start()
        {
            if (VRCObjectPool == null)
            {
                Debug.Log("[purabe]VRC Object Poolを登録してください。");
            }
        }

        protected void OnEnable()
        {
            if (Networking.LocalPlayer != null)
            {
                localPlayer = Networking.LocalPlayer;
            }

            if (RandomSpawn && VRCObjectPool != null)
            {
                //スポーン順序をシャッフル
                VRCObjectPool.Shuffle();
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
            if (VRCObjectPool == null)
            {
                Debug.LogError("[purabe]VRC Object Poolを登録してください。");
                return;
            }

            // スイッチのオーナ権限取得
            GetOwner(this.gameObject);

            // オブジェクトが全てactiveなら操作しない
            if (AllActive())
            {
                Debug.Log("[purabe]スポーンできるオブジェクトがありません。");
                return;
            }

            // Object Pool のスポーン準備
            PreparePoolForSpawn();
            // オブジェクトプールの配列頭のオブジェクトをスポーン
            GameObject spawnedObject = VRCObjectPool.TryToSpawn();

            if (spawnedObject == null)
            {
                Debug.Log("[purabe]スポーンできるオブジェクトがありません。");
                return;
            }

            // Spawn したアイテムのオーナ権限取得
            GetOwner(spawnedObject);

            // 拡張側で専用の移動処理を行わない場合は、通常の移動を実行
            if (!TryMoveSpecialObject(spawnedObject))
            {
                MoveToTarget(spawnedObject);
            }

            // Spawn成功後の拡張処理
            OnObjectSpawned(spawnedObject);

            // SE再生
            PlayAudio();
        }

        /// <summary>
        /// Spawn成功後の拡張処理
        /// </summary>
        /// <param name="target">Spawnに成功したオブジェクト</param>
        protected virtual void OnObjectSpawned(GameObject target)
        {
        }

        /// <summary>
        /// Object Pool からスポーンする前の準備
        /// </summary>
        protected virtual void PreparePoolForSpawn()
        {
            GetOwner(VRCObjectPool.gameObject);
        }

        /// <summary>
        /// 拡張パック固有の移動処理
        /// </summary>
        /// <param name="target">スポーンしたオブジェクト</param>
        /// <returns>専用の移動処理を行った場合はtrue</returns>
        protected virtual bool TryMoveSpecialObject(GameObject target)
        {
            return false;
        }

        /// <summary>
        /// 現在の設定でスポーン後の移動が必要かどうかを取得する
        /// </summary>
        /// <returns>移動が必要な場合はtrue、それ以外はfalse</returns>
        protected virtual bool HasMoveDestination()
        {
            return MoveItemToHand || SpawnPoint != null;
        }

        /// <summary>
        /// スポーン後の移動先Transformを解決する
        /// </summary>
        /// <returns>移動先Transform。移動先がない場合はnull</returns>
        protected virtual Transform ResolveMoveDestination()
        {
            return SpawnPoint;
        }

        /// <summary>
        /// スポーンオブジェクトを指定パラメータに従い移動させる
        /// </summary>
        /// <param name="target">移動するスポーン済みオブジェクト</param>
        protected virtual void MoveToTarget(GameObject target)
        {
            if (target == null)
            {
                moveTargetGo = null;
                return;
            }

            if (!HasMoveDestination())
            {
                moveTargetGo = null;
                return;
            }

            moveTargetGo = target;

            if (MoveItemToHand)
            {
                if (!Utilities.IsValid(localPlayer))
                {
                    Debug.LogError("[purabe]ローカルプレイヤーを取得できないため、手元へ移動できません。");
                    moveTargetGo = null;
                    return;
                }

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
            else
            {
                Transform destination = ResolveMoveDestination();
                if (destination == null)
                {
                    moveTargetGo = null;
                    return;
                }

                // 出現ポイントを指定されている場合
                toPos = destination.position;
                toRot = destination.rotation;
            }

            if (!EnqueueMove(moveTargetGo, toPos, toRot))
            {
                moveTargetGo = null;
                return;
            }

            moveTargetGo = null;

            // 遅延移動呼出
            SendCustomEventDelayedFrames(nameof(MoveToTargetDelayed), SpawnDelayFrames);
        }

        protected GameObject moveTargetGo;
        protected Vector3 toPos;
        protected Quaternion toRot;

        private GameObject[] pendingMoveTargets;
        private Vector3[] pendingMovePositions;
        private Quaternion[] pendingMoveRotations;
        private int pendingMoveHead;
        private int pendingMoveCount;

        /// <summary>
        /// 遅延移動するオブジェクトと移動先をキューに追加する
        /// </summary>
        /// <param name="target">移動するオブジェクト</param>
        /// <param name="position">移動先のワールド座標</param>
        /// <param name="rotation">移動先のワールド回転</param>
        /// <returns>キューに追加できた場合はtrue、それ以外はfalse</returns>
        private bool EnqueueMove(GameObject target, Vector3 position, Quaternion rotation)
        {
            if (target == null) return false;

            EnsureMoveQueue();
            if (pendingMoveCount >= pendingMoveTargets.Length)
            {
                Debug.LogError("[purabe]遅延移動キューに空きがありません。");
                return false;
            }

            int index = (pendingMoveHead + pendingMoveCount) % pendingMoveTargets.Length;
            pendingMoveTargets[index] = target;
            pendingMovePositions[index] = position;
            pendingMoveRotations[index] = rotation;
            pendingMoveCount++;
            return true;
        }

        /// <summary>
        /// Object Poolのサイズに合わせて遅延移動キューを初期化する
        /// </summary>
        private void EnsureMoveQueue()
        {
            if (pendingMoveTargets != null) return;

            int capacity = 1;
            if (VRCObjectPool != null && VRCObjectPool.Pool != null
                && VRCObjectPool.Pool.Length > 0)
            {
                capacity = VRCObjectPool.Pool.Length;
            }

            pendingMoveTargets = new GameObject[capacity];
            pendingMovePositions = new Vector3[capacity];
            pendingMoveRotations = new Quaternion[capacity];
            pendingMoveHead = 0;
            pendingMoveCount = 0;
        }

        /// <summary>
        /// 移動実施
        /// </summary>
        public void MoveToTargetDelayed()
        {
            if (pendingMoveCount > 0)
            {
                GameObject target = pendingMoveTargets[pendingMoveHead];
                Vector3 position = pendingMovePositions[pendingMoveHead];
                Quaternion rotation = pendingMoveRotations[pendingMoveHead];

                pendingMoveTargets[pendingMoveHead] = null;
                pendingMoveHead = (pendingMoveHead + 1) % pendingMoveTargets.Length;
                pendingMoveCount--;

                MoveObject(target, position, rotation);
                return;
            }

            // 従来の派生クラスから直接呼び出された場合の互換処理
            if (moveTargetGo == null) return;
            MoveObject(moveTargetGo, toPos, toRot);
            moveTargetGo = null;
        }

        /// <summary>
        /// 指定したオブジェクトを座標と回転へ移動する
        /// </summary>
        /// <param name="target">移動するオブジェクト</param>
        /// <param name="position">移動先のワールド座標</param>
        /// <param name="rotation">移動先のワールド回転</param>
        private void MoveObject(GameObject target, Vector3 position, Quaternion rotation)
        {
            if (target == null) return;

            Rigidbody rd = target.GetComponent<Rigidbody>();
            VRCObjectSync sync = target.GetComponent<VRCObjectSync>();

            if (rd != null)
            {
                rd.Sleep();
            }

            if (sync != null && !MoveItemToHand
                && SpawnPoint != null)
            {
                // VRCObjectSyncで移動
                sync.FlagDiscontinuity();
                sync.TeleportTo(SpawnPoint);
            }

            // transform 移動 (VRCObjectSyncがあっても実施)
            target.transform.SetPositionAndRotation(position, rotation);
        }

        /// <summary>
        /// すべてのオブジェクトが出現済みかどうか
        /// </summary>
        /// <returns>true:出現済み false:未</returns>
        protected bool AllActive()
        {
            if (VRCObjectPool == null) return true;

            foreach (GameObject item in VRCObjectPool.Pool)
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
            if (!Utilities.IsValid(localPlayer)) return false;

            Vector3 rightHandPos = localPlayer.GetBonePosition(HumanBodyBones.RightHand);
            Vector3 leftHandPos = localPlayer.GetBonePosition(HumanBodyBones.LeftHand);

            return Vector3.Distance(transform.position, rightHandPos) <= Vector3.Distance(transform.position, leftHandPos);
        }
    }
}
