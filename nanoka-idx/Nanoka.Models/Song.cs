using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    /// <summary>
    /// Represents a song.
    /// </summary>
    public class Song : SongBase, IHasId
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }

        [JsonProperty("length_ms")]
        public double Length { get; set; }

        [JsonProperty("bitrate")]
        public int Bitrate { get; set; }
    }

    public class SongBase : IHasEntityType
    {
        /// <summary>
        /// First element should be the fully localized primary name.
        /// </summary>
        [JsonProperty("names"), Required, MinLength(1)]
        public string[] Name { get; set; }

        [JsonProperty("preview_ms"), Range(0, double.MaxValue)]
        public double PreviewTime { get; set; }

        [JsonProperty("lyrics")]
        public Dictionary<LanguageType, SongLyrics> Lyrics { get; set; }

#region Meta

        [JsonIgnore]
        NanokaEntity IHasEntityType.Type => NanokaEntity.Song;

#endregion
    }
}