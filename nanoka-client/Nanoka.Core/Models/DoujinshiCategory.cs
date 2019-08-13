using System.Runtime.Serialization;

namespace Nanoka.Core.Models
{
    public enum DoujinshiCategory
    {
        [EnumMember(Value = "doujinshi")] Doujinshi = 0,
        [EnumMember(Value = "manga")] Manga,
        [EnumMember(Value = "artist_cg")] ArtistCg,
        [EnumMember(Value = "game_cg")] GameCg,
        [EnumMember(Value = "image_set")] ImageSet
    }
}