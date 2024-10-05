
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
        private ReturnObject _reference;
        [Header("リターン対象レイヤー"), Tooltip("13: Pickup")]
        public int layer = 13;

        private GameObject[] poolsRef;
        private void Start()
        {
            if (pools.Length <= 0 && _reference == null)
            {
                Debug.Log("[purabe]poolsを定義しない場合はreferenceを登録してください");
            }

            if (_reference != null)
            {
                poolsRef = _reference.pools;
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
                // poolオブジェクトそのものの場合
                VRCObjectPool pool = (VRCObjectPool)pg.GetComponent(typeof(VRCObjectPool));
                if (pool != null)
                {
                    ResetAllPerPool(pool);
                }

                // poolオブジェクトの親
                Component[] poolcs = pg.GetComponentsInChildren(typeof(VRCObjectPool));
                if (poolcs.Length > 0)
                {
                    foreach (Component p in poolcs)
                    {
                        ResetAllPerPool((VRCObjectPool)p);
                    }
                }
            }
        }

        /// <summary>
        /// Poolごとに全リセットを実行
        /// </summary>
        /// <param name="pool">Pool</param>
        private void ResetAllPerPool(VRCObjectPool pool)
        {
            /* VRC Object Poolに登録された
             * 全オブジェクトにReturn実行 */

            foreach (GameObject target in pool.Pool)
            {
                if (!target.activeInHierarchy)
                {
                    // 非表示ならば何もしない
                    continue;
                }

                // オーナ権限取得
                SetOwner(target);
                // Drop処理
                DropObject(target);
                // Return実行
                SetOwner(pool.gameObject);
                pool.Return(target);
            }
        }

        /// <summary>
        /// 親子関係チェック
        /// </summary>
        /// <param name="parent">親オブジェクト</param>
        /// <param name="child">子オブジェクト</param>
        /// <returns>親子 true/親子ではない false</returns>
        private bool HasGameObject(GameObject[] parent, GameObject child)
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
        private void ReturnProcess(GameObject target)
        {
            if (target != null && target.activeInHierarchy
                && target.layer == layer)
            {
                // オーナ権限取得
                SetOwner(target);
                // Drop処理
                DropObject(target);
                // すべてのVRC Object Poolに対してアイテムReturnを実行
                foreach (GameObject p in pools)
                {
                    ReturnProcessSub(target, p);
                    if (!target.activeInHierarchy)
                    {
                        return;
                    }
                }
                if (poolsRef == null)
                {
                    return;
                }
                foreach (GameObject p in poolsRef)
                {
                    // 直下のpoolと重複ならスキップ
                    if (HasGameObject(pools, p))
                    {
                        continue;
                    }
                    // リターン処理
                    ReturnProcessSub(target, p);
                    if (!target.activeInHierarchy)
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Return処理のサブ関数
        /// </summary>
        /// <param name="target">Return対象</param>
        /// <param name="g">PoolまたはPoolの親オブジェクト</param>
        private void ReturnProcessSub(GameObject target, GameObject g)
        {
            VRCObjectPool pool = (VRCObjectPool)g.GetComponent(typeof(VRCObjectPool));
            if (pool != null)
            {
                SetOwner(pool.gameObject);
                pool.Return(target);
                if (!target.activeInHierarchy)
                {
                    // SE再生
                    PlayAudio();
                    return;
                }
            }

            Component[] pools = g.GetComponentsInChildren(typeof(VRCObjectPool));
            foreach (Component x in pools)
            {
                VRCObjectPool p2 = (VRCObjectPool)x;
                SetOwner(p2.gameObject);
                p2.Return(target);
                if (!target.activeInHierarchy)
                {
                    // SE再生
                    PlayAudio();
                    return;
                }
            }
        }

        /// <summary>
        /// Drop処理
        /// </summary>
        /// <param name="target">対象オブジェクト</param>
        private void DropObject(GameObject target)
        {
            VRCPickup pickup = (VRCPickup)target.GetComponent(typeof(VRCPickup));
            if (pickup != null)
            {
                pickup.Drop();
            }
        }
    }
}