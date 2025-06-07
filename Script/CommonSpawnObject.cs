using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace PurabeWorks.SpawnObject
{
    public class CommonSpawnObject : UdonSharpBehaviour
    {
        [SerializeField, Header("SEの音源")]
        private AudioSource _audioSource = null;
        [SerializeField, Header("SEのクリップ")]
        private AudioClip _audioClip = null;

        /// <summary>
        /// オーディオ再生(全体再生用)
        /// </summary>
        public void PlayAudioSub()
        {
            if (_audioSource != null)
            {
                if (_audioClip != null)
                {
                    // audioClip 設定を優先
                    _audioSource.PlayOneShot(_audioClip);
                }
                else if (_audioSource.clip != null)
                {
                    _audioSource.PlayOneShot(_audioSource.clip);
                }
            }
        }

        /// <summary>
        /// オーディオ再生(外部呼出用)
        /// </summary>
        protected void PlayAudio()
        {
            SendCustomNetworkEvent(
                VRC.Udon.Common.Interfaces.NetworkEventTarget.All,
                nameof(PlayAudioSub));
        }

        /// <summary>
        /// オーナー権限獲得
        /// </summary>
        /// <param name="obj">対象オブジェクト</param>
        protected void SetOwner(GameObject obj)
        {
            if (!Networking.IsOwner(obj))
            {
                Networking.SetOwner(Networking.LocalPlayer, obj);
            }
        }
    }
}