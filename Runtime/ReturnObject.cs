// Copyright (c) 2026 Purabe Works
// Released under the MIT License. See LICENSE.txt for details.
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
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
        [FormerlySerializedAs("_reference")]
        private ReturnObject reference;
        [Header("リターン対象レイヤー"), Tooltip("13: Pickup")]
        public int layer = 13;

        protected GameObject[] poolsRef;

        protected ReturnObject Reference => reference;

        /// <summary>
        /// Return対象のPool参照を初期化する。
        /// </summary>
        protected void Start()
        {
            if ((pools == null || pools.Length <= 0) && Reference == null)
            {
                Debug.LogError("[purabe]poolsを定義しない場合はreferenceを登録してください");
            }

            if (Reference != null)
            {
                poolsRef = Reference.pools;
            }
        }

        /// <summary>
        /// トリガーに入ったオブジェクトのReturn処理を実行する。
        /// </summary>
        /// <param name="other">トリガーに入ったCollider</param>
        public void OnTriggerEnter(Collider other)
        {
            if (other == null) return;
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
                if (pg == null) continue;

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
            PreparePoolForReturn(pool);

            // Pool 内の全オブジェクトに対して Return 処理
            foreach (GameObject target in pool.Pool)
            {
                if (target == null || !target.activeInHierarchy)
                {
                    // null or 非表示ならば何もしない
                    continue;
                }

                // オーナ権限取得
                PrepareObjectForReturn(target);
                // Drop処理
                DropObject(target);
                // Return直前処理
                BeforeObjectReturn(target);
                // Return実行
                PreparePoolForReturn(pool);
                pool.Return(target);
                // Return後処理
                OnObjectReturned(target);
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
                PrepareObjectForReturn(target);
                // Drop処理
                DropObject(target);

                // 拡張側の構造を考慮し、実際にPoolへ返すオブジェクトを解決
                GameObject returnTarget = ResolveReturnTarget(target);
                if (returnTarget == null) return;
                if (returnTarget != target)
                {
                    PrepareObjectForReturn(returnTarget);
                }

                // すべてのVRC Object Poolに対してアイテムReturnを実行
                if (ReturnToConfiguredPools(returnTarget))
                {
                    OnObjectReturned(returnTarget);
                }
            }
        }

        /// <summary>
        /// 実際にPoolへ返すオブジェクトを解決する
        /// </summary>
        /// <param name="target">Return処理の起点となるオブジェクト。</param>
        /// <returns>実際にPoolへ返すオブジェクト。</returns>
        protected virtual GameObject ResolveReturnTarget(GameObject target)
        {
            return target;
        }

        /// <summary>
        /// Return前の対象オブジェクト準備
        /// </summary>
        /// <param name="target">準備するReturn対象オブジェクト。</param>
        protected virtual void PrepareObjectForReturn(GameObject target)
        {
            if (target == null) return;
            GetOwner(target);
        }

        /// <summary>
        /// Return前のObject Pool準備
        /// </summary>
        /// <param name="pool">準備するObject Pool。</param>
        protected virtual void PreparePoolForReturn(VRCObjectPool pool)
        {
            if (pool == null) return;
            GetOwner(pool.gameObject);
        }

        /// <summary>
        /// Return直前の拡張処理
        /// </summary>
        /// <param name="target">これからPoolへReturnするオブジェクト</param>
        protected virtual void BeforeObjectReturn(GameObject target)
        {
        }

        /// <summary>
        /// Return成功後の拡張処理
        /// </summary>
        /// <param name="target">PoolへのReturnに成功したオブジェクト</param>
        protected virtual void OnObjectReturned(GameObject target)
        {
        }

        /// <summary>
        /// 設定済みのPoolへ対象を返す
        /// </summary>
        /// <param name="target">Poolへ返すオブジェクト</param>
        /// <returns>いずれかのPoolへReturnできた場合はtrue、それ以外はfalse</returns>
        private bool ReturnToConfiguredPools(GameObject target)
        {
            if (ProcessPools(target, pools)) return true;
            if (poolsRef == null) return false;
            return ProcessPoolsWithRef(target, pools, poolsRef);
        }

        /// <summary>
        /// 指定されたPool配列を順に検索し、対象のReturnを試みる。
        /// </summary>
        /// <param name="obj">Poolへ返すオブジェクト</param>
        /// <param name="poolArray">検索対象のPoolまたはPoolの親オブジェクト配列</param>
        /// <returns>対象をReturnできた場合はtrue、それ以外はfalse</returns>
        protected bool ProcessPools(GameObject obj, GameObject[] poolArray)
        {
            if (obj == null || poolArray == null) return false;

            foreach (GameObject p in poolArray)
            {
                if (p == null) continue;
                ReturnProcessSub(obj, p);
                if (!obj.activeInHierarchy)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 重複するPool参照を除外しながら、参照先のPool配列から対象のReturnを試みる。
        /// </summary>
        /// <param name="obj">Poolへ返すオブジェクト</param>
        /// <param name="pools">重複判定の基準となるPoolまたはPoolの親オブジェクト配列</param>
        /// <param name="poolsRef">検索対象の参照先Pool配列</param>
        /// <returns>対象をReturnできた場合はtrue、それ以外はfalse</returns>
        protected bool ProcessPoolsWithRef(GameObject obj, GameObject[] pools, GameObject[] poolsRef)
        {
            if (obj == null || poolsRef == null) return false;

            foreach (GameObject p in poolsRef)
            {
                if (p == null) continue;
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
            if (target == null || g == null) return;

            VRCObjectPool[] poolsLocal = g.GetComponentsInChildren<VRCObjectPool>(true);

            foreach (VRCObjectPool p in poolsLocal)
            {
                if (!ContainsPoolObject(p, target)) continue;

                // Poolのオーナ権限取得
                PreparePoolForReturn(p);
                // Return直前処理
                BeforeObjectReturn(target);
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
        /// 指定したObject Poolに対象オブジェクトが登録されているか確認する
        /// </summary>
        private bool ContainsPoolObject(VRCObjectPool pool, GameObject target)
        {
            if (pool == null || pool.Pool == null || target == null) return false;

            // Enumerable.Contains は Udon not exposed
            foreach (GameObject item in pool.Pool)
            {
                if (item == target) return true;
            }
            return false;
        }

        /// <summary>
        /// Drop処理
        /// </summary>
        /// <param name="target">対象オブジェクト</param>
        protected void DropObject(GameObject target)
        {
            if (target == null) return;

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
