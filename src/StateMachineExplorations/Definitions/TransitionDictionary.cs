namespace Morgados.StateMachines.Definitions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class TransitionDictionary : IEnumerable<KeyValuePair<string, GuardedTransitionBase>>
    {
        private readonly Dictionary<string, LinkedList<GuardedTransitionBase>> transitionsDictionary = new Dictionary<string, LinkedList<GuardedTransitionBase>>();

        public TransitionDictionary()
        {
        }

        public IEnumerator<KeyValuePair<string, GuardedTransitionBase>> GetEnumerator()
        {
            foreach (var key in this.transitionsDictionary.Keys)
            {
                if (this.transitionsDictionary.TryGetValue(key, out var transitionsCollection))
                {
                    foreach (var transition in transitionsCollection)
                    {
                        yield return new KeyValuePair<string, GuardedTransitionBase>(key, transition);
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public ICollection<GuardedTransitionBase> this[string key] => this.GetTransitionsCollection(key);

        public void Add(string key, GuardedTransitionBase transition) => this.GetTransitionsCollection(key).AddLast(transition);

        private LinkedList<GuardedTransitionBase> GetTransitionsCollection(string key)
        {
            if (!this.transitionsDictionary.TryGetValue(key, out var transitionsCollection))
            {
                transitionsCollection = new LinkedList<GuardedTransitionBase>();
                this.transitionsDictionary[key] = transitionsCollection;
            }

            return transitionsCollection;
        }
    }
}
