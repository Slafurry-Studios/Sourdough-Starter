using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Slafurry.Core.Interface;
using Slafurry.Core.Abstract;

namespace Slafurry.System
{
    /// <summary>
    /// Pure logic loading sequence. Fires events for progress/status,
    /// does not know about any UI implementation.
    /// </summary>
    public class LoadingSystem : GameSystem<LoadingSystem>
    {
        [SerializeField] private float perObjectTimeoutSeconds = 10f;

        private readonly List<IInitializable> _registered = new();

        public event Action<float> OnProgressChanged;
        public event Action<string> OnStatusChanged;
        public event Action OnLoadingComplete;

        public void Register(IInitializable obj) => _registered.Add(obj);

        void Start() => StartCoroutine(LoadSequence());

        private IEnumerator LoadSequence()
        {
            var ordered = _registered.OrderBy(o => o.Priority).ToList();
            int total = Mathf.Max(ordered.Count, 1);

            // ---- PHASE 1: Initialize each object ----
            for (int i = 0; i < ordered.Count; i++)
            {
                var obj = ordered[i];
                OnStatusChanged?.Invoke($"Initializing {obj.GetType().Name}...");

                yield return StartCoroutine(SafeInit(obj));

                OnProgressChanged?.Invoke((float)(i + 1) / total * 0.8f); // 80% budget for phase 1
                yield return null;
            }

            // ---- PHASE 2: PostInitialize, everything is ready ----
            OnStatusChanged?.Invoke("Finalizing...");
            foreach (var obj in ordered)
            {
                try { obj.PostInitialize(); }
                catch (Exception e) { Debug.LogError($"PostInitialize failed on {obj.GetType().Name}: {e}"); }
                yield return null;
            }

            OnProgressChanged?.Invoke(1f);
            OnStatusChanged?.Invoke("Ready!");
            OnLoadingComplete?.Invoke();
        }

        private IEnumerator SafeInit(IInitializable obj)
        {
            var enumerator = obj.Initialize();
            float elapsed = 0f;

            while (true)
            {
                bool moved;
                try { moved = enumerator.MoveNext(); }
                catch (Exception e)
                {
                    Debug.LogError($"Init failed on {obj.GetType().Name}: {e}");
                    yield break;
                }

                if (!moved) break;

                elapsed += Time.deltaTime;
                if (elapsed > perObjectTimeoutSeconds)
                {
                    Debug.LogWarning($"{obj.GetType().Name} timed out during Initialize() after {perObjectTimeoutSeconds}s");
                    yield break;
                }

                yield return enumerator.Current;
            }
        }

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake(); // still DontDestroyOnLoad
        }

        public override IEnumerator Initialize() { yield return null; }
        public override void PostInitialize() { }
    }
}