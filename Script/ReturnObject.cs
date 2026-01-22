
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;

namespace PurabeWorks.SpawnObject
{
    /// <summary>
    /// Return処理
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ReturnObject : CommonSpawnObject
    {
        [Header("VRC Object Poolオブジェクトまたは親")]
        public GameObject[] pools;
        [SerializeField, Header("VRC Object Poolオブジェクトまたは親の参照先")]
        protected ReturnObject reference;
        [Header("リターン対象レイヤー"), Tooltip("13: Pickup")]
        public int layer = 13;

        protected GameObject[] poolsRef;
        protected void Start()
        {
            if (pools.Length <= 0 && reference == null)
            {
                Debug.Log("[purabe]poolsを定義しない場合はreferenceを登録してください");
            }

            if (reference != null)
            {
                poolsRef = reference.pools;
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            ReturnProcess(other.gameObject);
        }

        /// <summary>
        /// 全リセット実行(外部呼出用)
        /// </summary>
        public void ResetAll()
        {
            // すべて返却する
            ResetAllPerArray(pools);
            ResetAllPerArray(poolsRef);
        }

        /// <summary>
        /// 配列ごとに全リセットを実行
        /// </summary>
        /// <param name="targetPoolgs">Pool配列</param>
        private void ResetAllPerArray(GameObject[] targetPoolgs)
        {
            // 参照先のプールオブジェクト配列ごとの処理
            if (targetPoolgs == null || targetPoolgs.Length <= 0)
            {
                return;
            }

            foreach (GameObject pg in targetPoolgs)
            {
                // 子も含めて Pool を取り出して処理
                VRCObjectPool[] poolsLocal = pg.GetComponentsInChildren<VRCObjectPool>(true);
                if (poolsLocal.Length > 0)
                {
                    foreach (VRCObjectPool p in poolsLocal)
                    {
                        ResetAllPerPool(p);
                    }
                }
            }
        }

        /// <summary>
        /// Poolごとに全リセットを実行
        /// </summary>
        /// <param name="pool">Pool</param>
        protected virtual void ResetAllPerPool(VRCObjectPool pool)
        {
            if (pool == null) return;

            // オーナ権限取得
            GetOwner(pool.gameObject);

            // Pool 内の全オブジェクトに対して Return 処理
            foreach (GameObject target in pool.Pool)
            {
                if (target == null || !target.activeInHierarchy)
                {
                    // null of 非表示ならば何もしない
                    continue;
                }

                // オーナ権限取得
                GetOwner(target);
                // Drop処理
                DropObject(target);
                // Return実行
                GetOwner(pool.gameObject);
                pool.Return(target);
            }
        }

        /// <summary>
        /// 親子関係チェック
        /// </summary>
        /// <param name="parent">親オブジェクト</param>
        /// <param name="child">子オブジェクト</param>
        /// <returns>親子 true/親子ではない false</returns>
        protected bool HasGameObject(GameObject[] parent, GameObject child)
        {
            if (parent == null || child == null)
            {
                return false;
            }
            foreach (GameObject c in parent)
            {
                if (child == c)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Return処理
        /// </summary>
        /// <param name="target">対象オブジェクト</param>
        protected virtual void ReturnProcess(GameObject target)
        {
            if (target != null && target.activeInHierarchy
                && target.layer == layer)
            {
                // 対象オブジェクトのオーナ権限取得
                GetOwner(target);
                // Drop処理
                DropObject(target);

                // すべてのVRC Object Poolに対してアイテムReturnを実行
                if (ProcessPools(target, pools)) return;
                if (poolsRef == null) return;
                if (ProcessPoolsWithRef(target, pools, poolsRef)) return;
            }
        }

        // poolsのReturnProcessSub処理
        protected bool ProcessPools(GameObject obj, GameObject[] poolArray)
        {
            foreach (GameObject p in poolArray)
            {
                ReturnProcessSub(obj, p);
                if (!obj.activeInHierarchy)
                    return true;
            }
            return false;
        }

        // poolsRefのReturnProcessSub処理（重複除外）
        protected bool ProcessPoolsWithRef(GameObject obj, GameObject[] pools, GameObject[] poolsRef)
        {
            foreach (GameObject p in poolsRef)
            {
                if (HasGameObject(pools, p))
                    continue;
                ReturnProcessSub(obj, p);
                if (!obj.activeInHierarchy)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Return処理のサブ関数
        /// </summary>
        /// <param name="target">Return対象</param>
        /// <param name="g">PoolまたはPoolの親オブジェクト</param>
        private void ReturnProcessSub(GameObject target, GameObject g)
        {
            VRCObjectPool[] poolsLocal = g.GetComponentsInChildren<VRCObjectPool>(true);

            foreach (VRCObjectPool p in poolsLocal)
            {
                // Poolのオーナ権限取得
                GetOwner(p.gameObject);
                // リターン実行
                p.Return(target);
                if (!target.activeInHierarchy)
                {
                    // SE再生
                    PlayAudio();
                    // 終了
                    return;
                }
            }
        }

        /// <summary>
        /// Drop処理
        /// </summary>
        /// <param name="target">対象オブジェクト</param>
        protected void DropObject(GameObject target)
        {
            VRCPickup[] pickups = target.GetComponentsInChildren<VRCPickup>(true);

            if (pickups.Length <= 0)
            {
                return;
            }

            foreach (VRCPickup pickup in pickups)
            {
                if (pickup == null)
                {
                    continue;
                }
                pickup.Drop();
            }
        }
    }
}
