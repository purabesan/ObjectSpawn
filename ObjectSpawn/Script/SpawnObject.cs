using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace PurabeWorks
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SpawnObject : UdonSharpBehaviour
    {
        [SerializeField, Header("スポーン対象のVRC Object Pool")]
        private VRCObjectPool _vRCObjectPool;
        [SerializeField, Header("ランダムスポーンをするかどうか")]
        private bool _randomSpawn = false;
        [SerializeField, Header("スポーンアイテムを手元に移動するか")]
        private bool _moveItemToHand = true;
        [SerializeField, Header("スポーン時に再生するオーディオの音源")]
        private AudioSource _audioSource = null;
        [SerializeField, Header("スポーン時に再生するオーディオクリップ")]
        private AudioClip _audioClip = null;
        [SerializeField, Header("カスタムメソッドを実行するか")]
        private bool _executeCustomEvent = false;
        [SerializeField, Header("スポーン対象以外のカスタムメソッド実行先")]
        private GameObject[] _externalObjects;
        [SerializeField, Header("カスタムメソッド(コピペ用)")]
        [TextArea]
        public string CustomEventNames = "public void Pura_OnSpawn(){}";

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

        private bool AllActive()
        {
            foreach(GameObject item in _vRCObjectPool.Pool)
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
                } else
                {
                    spawnedObject.transform.position = localPlayer.GetBonePosition(HumanBodyBones.LeftHand);
                }
            }

            // カスタムメソッド実行
            if (_executeCustomEvent)
            {
                ExecuteCustomEvent(spawnedObject);
            }

            // SE再生
            if (_audioSource != null && _audioSource.clip != null)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(PlayAudio));
            }
        }

        private void SetOwner(GameObject obj)
        {
            if (!Networking.IsOwner(obj))
            {
                Networking.SetOwner(Networking.LocalPlayer, obj);
            }
        }

        private bool IsNearToRightHand()
        {
            Vector3 rightHandPos = localPlayer.GetBonePosition(HumanBodyBones.RightHand);
            Vector3 leftHandPos = localPlayer.GetBonePosition(HumanBodyBones.LeftHand);

            return Vector3.Distance(transform.position, rightHandPos) <= Vector3.Distance(transform.position, leftHandPos);
        }

        private void ExecuteCustomEvent(GameObject spawnedObject)
        {
            //スポーンされたオブジェクトでの実行
            Component[] udons = spawnedObject.GetComponents(typeof(UdonBehaviour));
            foreach (Component c in udons)
            {
                ExecuteCustomEventSub((UdonBehaviour)c);
            }
            udons = spawnedObject.GetComponentsInChildren(typeof(UdonBehaviour));
            foreach (Component c in udons)
            {
                ExecuteCustomEventSub((UdonBehaviour)c);
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
                    foreach (Component c in udons)
                    {
                        ExecuteCustomEventSub((UdonBehaviour)c);
                    }
                    udons = e.GetComponentsInChildren(typeof(UdonBehaviour));
                    foreach (Component c in udons)
                    {
                        ExecuteCustomEventSub((UdonBehaviour)c);
                    }
                }
            }
        }

        private void ExecuteCustomEventSub(UdonBehaviour udon)
        {
            udon.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Pura_OnSpawn");
        }

        public void PlayAudio()
        {
            if (_audioClip == null && _audioSource.clip != null)
            {
                _audioSource.PlayOneShot(_audioSource.clip);
            }
            else if (_audioSource != null)
            {
                _audioSource.PlayOneShot(_audioClip);
            }
        }
    }
}