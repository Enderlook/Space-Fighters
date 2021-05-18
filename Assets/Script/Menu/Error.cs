using System;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Menu
{
    public sealed class Error : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        private Text errorMessage;

        [SerializeField]
        private GameObject errorContainer;
#pragma warning restore CS0649

        private Action callback;

        public void Show(string error, Action callback)
        {
            errorContainer.SetActive(true);
            errorMessage.text = error;
            this.callback = callback;
        }

        public void Callback() => callback?.Invoke();
    }
}