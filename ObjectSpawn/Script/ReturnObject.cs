
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Components;
using VRC.Udon;

namespace PurabeWorks
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ReturnObject : UdonSharpBehaviour
    {
        [Header("VRC Object Poolオブジェクトまたは親")]
        public GameObject[] pools;
        [SerializeField, Header("VRC Object Poolオブジェクトまたは親の参照先")]
        private ReturnObject _reference;
        [Header("リターン対象レイヤー")]
        public int layer = 13;
        [SerializeField, Header("リターン時に再生するオーディオ")]
        private AudioSource _audioSource = null;
        [SerializeField, Header("再生SE")]
        private AudioClip _audioClip = null;
        [SerializeField, Header("カスタムメソッドを実行するか")]
        private bool _executeCustomEvent = false;
        [SerializeField, Header("リターン対象以外のカスタムメソッド実行先")]
        private GameObject[] _externalObjects;
        [SerializeField, Header("カスタムメソッド(コピペ用)")]
        [TextArea]
        public string CustomEventNames = "public void Pura_OnReturn(){}";

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


        public void ResetAll()
        {
            // すべて返却する
            ResetAllPerArray(pools);
            ResetAllPerArray(poolsRef);
        }

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

        private void ResetAllPerPool(VRCObjectPool pool)
        {
            /* VRC Object Poolに登録された
             * 全オブジェクトにReturn実行 */

            foreach(GameObject target in pool.Pool)
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
                pool.Return(target);
            }
        }

        private bool HasGameObject(GameObject[] parent, GameObject child)
        {
            if (parent == null)
            {
                return false;
            }
            foreach(GameObject c in parent)
            {
                if (child == c)
                {
                    return true;
                }
            }
            return false;
        }

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

        private void ReturnProcessSub(GameObject target, GameObject g)
        {
            VRCObjectPool pool = (VRCObjectPool)g.GetComponent(typeof(VRCObjectPool));
            if (pool != null)
            {
                pool.Return(target);
                if (!target.activeInHierarchy)
                {
                    DoWhenReturned(target);
                    return;
                }
            }

            Component[] pools = g.GetComponentsInChildren(typeof(VRCObjectPool));
            foreach (Component x in pools)
            {
                VRCObjectPool p2 = (VRCObjectPool)x;
                p2.Return(target);
                if (!target.activeInHierarchy)
                {
                    DoWhenReturned(target);
                    return;
                }
            }
        }

        private void DoWhenReturned(GameObject obj)
        {
            // カスタムメソッド対応
            if (_executeCustomEvent)
            {
                //カスタムメソッドを全Udon Behaviourで発火する
                Component[] udons = obj.GetComponents(typeof(UdonBehaviour));
                foreach (Component c in udons)
                {
                    DoWhenReturnedSub((UdonBehaviour)c);
                }
                udons = obj.GetComponentsInChildren(typeof(UdonBehaviour));
                foreach (Component c in udons)
                {
                    DoWhenReturnedSub((UdonBehaviour)c);
                }

                //外部オブジェクトでの実行
                if (_externalObjects.Length > 0)
                {
                    foreach (GameObject e in _externalObjects)
                    {
                        if (e == null)
                        {
                            continue;
                        }
                        
                        udons = e.GetComponents(typeof(UdonBehaviour));
                        foreach(Component c in udons)
                        {
                            DoWhenReturnedSub((UdonBehaviour)c);
                        }
                        udons = e.GetComponentsInChildren(typeof(UdonBehaviour));
                        foreach (Component c in udons)
                        {
                            DoWhenReturnedSub((UdonBehaviour)c);
                        }
                    }
                }
            }

            //リターンSE再生
            if (_audioSource != null && (_audioClip != null || _audioSource.clip != null))
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(PlayAudio));
            }
        }

        private void DoWhenReturnedSub(UdonBehaviour udon)
        {
            udon.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Pura_OnReturn");
        }

        private void SetOwner(GameObject obj)
        {
            if (!Networking.IsOwner(obj))
            {
                Networking.SetOwner(Networking.LocalPlayer, obj);
            }
        }

        public void PlayAudio()
        {
            if(_audioClip == null && _audioSource.clip != null)
            {
                _audioSource.PlayOneShot(_audioSource.clip);
            } else if(_audioSource != null) {
                _audioSource.PlayOneShot(_audioClip);
            }
        }

        private void DropObject(GameObject target)
        {
            // Drop処理
            VRCPickup pickup = (VRCPickup)target.GetComponent(typeof(VRCPickup));
            if (pickup != null)
            {
                pickup.Drop();
            }
        }
    }
}