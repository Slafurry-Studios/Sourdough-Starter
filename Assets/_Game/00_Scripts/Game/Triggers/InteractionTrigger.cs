using Slafurry.Player;
using UnityEngine;
using UnityEngine.Events;

namespace Slafurry.Interaction
{
    public class InteractionTrigger : MonoBehaviour, IInteractable
    {
        [Header("Interaction")]
        [SerializeField] private string prompt = "Interact";
        [SerializeField] private UnityEvent onInteract;

        [Header("Limit")]
        [Tooltip("How many times this interaction may be triggered. Set 0 or negative = no limit (can be used repeatedly).")]
        [SerializeField] private int maxInteractions = 1;

        [Tooltip("When the limit is reached, whether this object is automatically deactivated (SetActive(false))? If not, the object stays but Interact() does nothing.")]
        [SerializeField] private bool disableWhenLimitReached = false;

        [Header("Additional Events (optional)")]
        [Tooltip("Fires only once, right when the interaction limit has just been reached.")]
        [SerializeField] private UnityEvent onLimitReached;

        public string Prompt => prompt;
        public bool CanInteract => !IsLimitReached;

        private int _interactionCount = 0;

        public bool IsLimitReached => maxInteractions > 0 && _interactionCount >= maxInteractions;

        public void Interact()
        {
            if (IsLimitReached) return;

            _interactionCount++;
            onInteract?.Invoke();

            if (IsLimitReached)
            {
                onLimitReached?.Invoke();

                if (disableWhenLimitReached)
                    gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Decrease the interaction count by 1 (for quit/losing a mini-game).
        /// Will not go below 0.
        /// </summary>
        public void DecrementCount()
        {
            _interactionCount = Mathf.Max(0, _interactionCount - 1);
        }

        /// <summary>
        /// Reset the interaction count, use when this object needs to be reusable
        /// (e.g. after level reset / checkpoint).
        /// </summary>
        public void ResetInteractionCount()
        {
            _interactionCount = 0;
        }
    }
}