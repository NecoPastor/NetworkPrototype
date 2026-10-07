
using Game.Systems.GameStateMachine;

public class GameStateChangedEvent : IGameEvent
{
    public GameStateChangedEvent(GameState previous, GameState current) { }

}
