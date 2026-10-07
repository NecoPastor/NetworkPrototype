using Game.Systems.GameStateMachine;
using Game.Systems.Service;
using UnityEngine;

namespace Game.Systems
{
    public class GameStateMachineProvider : GameSystemProvider, IServiceProvider<StateMachine>
    {
        [SerializeField] private StateMachine gameStateMachine;

        public StateMachine Get() => gameStateMachine;

    }
}

