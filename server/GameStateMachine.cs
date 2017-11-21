using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    abstract class GameState
    {
        public abstract GameState ApplyAction(PlayerAction action);
    }

    class PacmanGameState : GameState
    {
        public override GameState ApplyAction(PlayerAction action)
        {
            return this;
        }
    }

    class GameStateMachine : MarshalByRefObject
    {
        private GameState currentState;

        public GameState CurrentState { get { return currentState; } }

        public GameStateMachine(GameState initialState)
        {
            this.currentState = initialState;
        }

        public GameState TransitionFunction(PlayerAction action)
        {
            return this.CurrentState.ApplyAction(action);
        }

    }
}
