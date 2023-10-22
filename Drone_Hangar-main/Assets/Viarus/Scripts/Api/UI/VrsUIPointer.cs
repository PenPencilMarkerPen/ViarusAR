using UnityEngine;
using UnityEngine.EventSystems;

namespace Vrs.Internal
{
    public class VrsUIPointer : MonoBehaviour
    {
        [HideInInspector]
        public PointerEventData pointerEventData;
        protected EventSystem cachedEventSystem;
        protected GazeInputModule cachedVRInputModule;

        protected virtual void OnEnable()
        {
            ConfigureEventSystem();
        }

        protected virtual void OnDisable()
        {
            GazeInputModule.RemovePoint(this);
        }

        public bool IsShow()
        {
            VrsReticle mVrsReticle = gameObject.GetComponent<VrsReticle>();
            if(mVrsReticle != null)
            {
                return mVrsReticle.IsShowing();
            }
            return true;
        }

        protected virtual void ConfigureEventSystem()
        {
            if (!cachedEventSystem)
            {
                cachedEventSystem = FindObjectOfType<EventSystem>();
            }

            if (!cachedVRInputModule)
            {
                cachedVRInputModule = cachedEventSystem.GetComponent<GazeInputModule>();
            }

            if (cachedEventSystem && cachedVRInputModule)
            {
                if (pointerEventData == null)
                {
                    pointerEventData = new PointerEventData(cachedEventSystem);
                }
            }
            GazeInputModule.AddPoint(this);
        }
    }
}