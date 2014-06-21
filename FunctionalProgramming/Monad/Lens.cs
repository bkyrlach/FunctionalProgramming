using System;

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

        public TEntity Set(TEntity e, TProperty value)
        {
            return _mutator(e, value);
        }

        public Lens<TEntity, TChildProperty> Combine<TChildProperty>(Lens<TProperty, TChildProperty> otherLens)
        {
            return new Lens<TEntity, TChildProperty>((e, cpv) => Set(e, otherLens.Set(_accessor(e), cpv)), e => otherLens.Get(_accessor(e)));
        } 
    }
}
