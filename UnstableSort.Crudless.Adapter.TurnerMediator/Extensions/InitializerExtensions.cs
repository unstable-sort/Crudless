using UnstableSort.Crudless.Adapter.TurnerMediator;

namespace UnstableSort.Crudless
{
    public static class IncludeTurnerMediatorAdapterInitializer
    {
        public static CrudlessInitializer UseTurnerMediatorAdapter(this CrudlessInitializer initializer)
        {
            return initializer.AddInitializer(new TurnerMediatorAdapterInitializer());
        }
    }
}