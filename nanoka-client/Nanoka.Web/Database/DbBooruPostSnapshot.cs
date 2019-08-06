using Nanoka.Core;
using Nanoka.Core.Models;
using Newtonsoft.Json;

namespace Nanoka.Web.Database
{
    public class DbBooruPostSnapshot : DbSnapshotBase<DbBooruPostSnapshot, BooruPostSnapshot, BooruPost>
    {
        protected override string Serialize(JsonSerializer serializer, BooruPost value)
            => serializer.Serialize(new DbBooruPost().Apply(value));

        protected override BooruPost Deserialize(JsonSerializer serializer, string value)
            => serializer.Deserialize<DbBooruPost>(value).ApplyTo(new BooruPost());
    }
}