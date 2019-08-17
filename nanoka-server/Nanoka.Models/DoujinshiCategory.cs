using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum DoujinshiCategory
    {
        [EnumMember(Value = "doujinshi")] Doujinshi = 0,
        [EnumMember(Value = "manga")] Manga = 1,
        [EnumMember(Value = "artist_cg")] ArtistCg = 2,
        [EnumMember(Value = "game_cg")] GameCg = 3,
        [EnumMember(Value = "image_set")] ImageSet = 4
    }
}