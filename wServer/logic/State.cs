using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wServer.logic
{
    public class State
    {
        public State(params object[] children) : this("", children) { }
        public State(string name, params object[] children)
        {
            this.Name = name;
            States = new List< State>();
            Behaviors = new List<Behavior>();
            Transitions = new List<Transition>();
            foreach (var i in children)
            {
                if (i is State)
                {
                    State state = i as State;
                    state.Parent = this;
                    States.Add(state);
                }
                else if (i is Behavior)
                    Behaviors.Add(i as Behavior);
                else if (i is Transition)
                    Transitions.Add(i as Transition);
                else
                    throw new NotSupportedException("Unknown children type.");
            }
        }

        public string Name { get; private set; }
        public State Parent { get; private set; }
        public IList<State> States { get; private set; }
        public IList<Behavior> Behaviors { get; private set; }
        public IList<Transition> Transitions { get; private set; }

        internal void Resolve(Dictionary<string, State> states)
        {
            states[Name] = this;
            foreach (var i in States)
                i.Resolve(states);
            foreach (var i in States)
                foreach (var j in i.Transitions)
                    j.Resolve(states);
        }

        void ResolveTransition(Dictionary<string, State> states)
        {
            foreach (var i in Transitions)
                i.Resolve(states);
        }

        public override string ToString()
        {
            return Name;
        }

        public static readonly State NullState = new State();
    }
}
