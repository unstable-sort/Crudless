using UnstableSort.Crudless.Configuration;

namespace UnstableSort.Crudless.Requests
{
    public class InlineConfigurableRequest
        : IInlineConfiguredRequest
    {
        internal object Profile { get; set; }

        public object BuildProfile() => Profile;
    }
}
