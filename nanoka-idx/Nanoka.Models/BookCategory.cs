using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum BookCategory
    {
        /// <summary>
        /// Book is a doujinshi.
        /// </summary>
        [EnumMember(Value = "doujinshi")] Doujinshi = 0,

        /// <summary>
        /// Book is a manga.
        /// </summary>
        [EnumMember(Value = "manga")] Manga = 1,

        /// <summary>
        /// Book contains artist CG.
        /// </summary>
        [EnumMember(Value = "artistCg")] ArtistCg = 2,

        /// <summary>
        /// Book contains game CG.
        /// </summary>
        [EnumMember(Value = "gameCg")] GameCg = 3,

        /// <summary>
        /// Book is a set of images.
        /// </summary>
        [EnumMember(Value = "imageSet")] ImageSet = 4,

        /// <summary>
        /// Book is a light novel.
        /// </summary>
        [EnumMember(Value = "novel")] LightNovel = 5
    }
}