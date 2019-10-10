using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    /// <summary>
    /// Represents an image in a booru.
    /// </summary>
    public class Image : ImageBase, IHasId, IHasScore
    {
        /// <summary>
        /// Image ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Score value.
        /// </summary>
        [JsonProperty("score")]
        public double Score { get; set; }

        /// <summary>
        /// Image width in pixels.
        /// </summary>
        [JsonProperty("width")]
        public int Width { get; set; }

        /// <summary>
        /// Image height in pixels.
        /// </summary>
        [JsonProperty("height")]
        public int Height { get; set; }

        /// <summary>
        /// Image format.
        /// </summary>
        [JsonProperty("type")]
        public ImageMediaType MediaType { get; set; }
    }

    public class ImageBase : IHasEntityType, IValidatableObject
    {
        public const int MinimumTagCount = 5;

        /// <summary>
        /// Tags added to this image.
        /// </summary>
        [JsonProperty("tags"), Required]
        public Dictionary<ImageTag, string[]> Tags { get; set; }

        /// <summary>
        /// Sources from where this image was downloaded from.
        /// </summary>
        [JsonProperty("source")]
        public ExternalSource[] Sources { get; set; }

        /// <summary>
        /// Content rating.
        /// </summary>
        [JsonProperty("rating")]
        public MaterialRating Rating { get; set; }

        /// <summary>
        /// Notes added to this image.
        /// </summary>
        [JsonProperty("notes")]
        public ImageNote[] Notes { get; set; }

#region Meta

        [JsonIgnore]
        NanokaEntity IHasEntityType.Type => NanokaEntity.Image;

#endregion

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Tags.Values.SelectMany(x => x).Count() < MinimumTagCount)
                yield return new ValidationResult($"Must provide at least {MinimumTagCount} tags.");
        }
    }
}