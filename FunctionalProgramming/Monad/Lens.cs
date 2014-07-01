using System;
using FunctionalProgramming.Basics;

namespace FunctionalProgramming.Monad
{
    public class Lens<TEntity, TProperty>
    {
        private readonly Func<TEntity, TProperty, TEntity> _mutator;
        private readonly Func<TEntity, TProperty> _accessor;
 
        public Lens(Func<TEntity, TProperty, TEntity> mutator, Func<TEntity, TProperty> accessor)
        {
            _mutator = mutator;
            _accessor = accessor;
        }

        public TProperty Get(TEntity e)
        {
            return _accessor(e);
        }

        public State<TEntity, TProperty> GetS()
        {
            return _accessor.Get();
        }

        public TEntity Set(TEntity e, TProperty value)
        {
            return _mutator(e, value);
        }

        public TEntity Mod(TEntity e, Func<TProperty, TProperty> updater)
        {
            return _mutator(e, updater(_accessor(e)));
        }

        public State<TEntity, TProperty> ModS(Func<TProperty, TProperty> updater)
        {
            return new State<TEntity, TProperty>(e => Tuple.Create(_mutator(e, updater(_accessor(e))), updater(_accessor(e))));
        } 

        public Lens<TEntity, TChildProperty> AndThen<TChildProperty>(Lens<TProperty, TChildProperty> otherLens)
        {
            return new Lens<TEntity, TChildProperty>((e, cpv) => Set(e, otherLens.Set(_accessor(e), cpv)), e => otherLens.Get(_accessor(e)));
        } 
    }
}
