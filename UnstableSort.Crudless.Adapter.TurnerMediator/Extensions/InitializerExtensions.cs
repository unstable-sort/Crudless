using UnstableSort.Crudless.Adapter.TurnerMediator;

namespace UnstableSort.Crudless
{
    public static class IncludeTurnerMediatorAdapterInitializer
    {
        public static CrudlessInitializer UseTurnerMediatorAdapter(this CrudlessInitializer initializer,
            AdapterDirection direction = AdapterDirection.CrudlessToTurner)
        {
            return direction switch
            {
                AdapterDirection.TurnerToCrudless => initializer.AddInitializer(new TurnerToCrudlessMediatorAdapterInitializer()),
                _ => initializer.AddInitializer(new CrudlessToTurnerMediatorAdapterInitializer())
            };
        }
    }
}