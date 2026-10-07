using UnityEngine;

namespace Game.Systems
{
    public class EventHub : MonoBehaviour
    {
        public EventBus EventBus
        {
            get
            {
                if (eventBus == null)
                {
                    eventBus = new EventBus();
                }

                return eventBus;
            }
        }
        protected EventBus eventBus;

    }
}
