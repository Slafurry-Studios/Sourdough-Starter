using System.Collections;
using Slafurry.Core.Interface;
using Slafurry.System;
using UnityEngine;

namespace Slafurry.Core.Abstract
{
    public abstract class Manager : MonoBehaviour, IInitializable
    {
        public virtual int Priority => 0;

        // Sealed - override subclass from OnManagerAwake(), so
        // RegisterToGameManager() & LoadingManager.Register() never forget to be called.
        private void Awake()
        {
            LoadingSystem.Instance.Register(this);
            RegisterToGameManager();
            OnManagerAwake();
        }

        private void OnDestroy()
        {
            UnregisterFromGameManager();
            OnManagerDestroyed();
        }

        /// <summary>Awake Hook for subclass.</summary>
        protected virtual void OnManagerAwake() { }

        /// <summary>OnDestroy Hook for subclass.</summary>
        protected virtual void OnManagerDestroyed() { }

        /// <summary>Mandatory, example: GameManager.Instance.RegisterEnemyManager(this)</summary>
        protected abstract void RegisterToGameManager();

        /// <summary>Mandatory, example: GameManager.Instance.UnregisterEnemyManager(this)</summary>
        protected abstract void UnregisterFromGameManager();

        public abstract IEnumerator Initialize();
        public abstract void PostInitialize();
    }
}
