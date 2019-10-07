using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum BookCategory
    {
        [EnumMember(Value = "doujinshi")] Doujinshi = 0,
        [EnumMember(Value = "manga")] Manga = 1,
        [EnumMember(Value = "artistCg")] ArtistCg = 2,
        [EnumMember(Value = "gameCg")] GameCg = 3,
        [EnumMember(Value = "imageSet")] ImageSet = 4,
        [EnumMember(Value = "novel")] LightNovel = 5
    }
}