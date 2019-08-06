using Nanoka.Core;
using Nanoka.Core.Models;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Web.Database
{
    [ElasticsearchType(RelationName = nameof(Doujinshi) + "Snapshot", IdProperty = nameof(Id))]
    public class DbDoujinshiSnapshot : DbSnapshotBase<DbDoujinshiSnapshot, Doujinshi>
    {
        protected override string Serialize(JsonSerializer serializer, Doujinshi value)
            => serializer.Serialize(new DbDoujinshi().Apply(value));

        protected override Doujinshi Deserialize(JsonSerializer serializer, string value)
            => serializer.Deserialize<DbDoujinshi>(value).ApplyTo(new Doujinshi());
    }
}