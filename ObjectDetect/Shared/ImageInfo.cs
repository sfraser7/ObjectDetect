using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ObjectDetect.Shared
{
    /// <summary>
    /// Class to hold all image data for uploaded image and resulting annotated image.
    /// </summary>
    public class ImageInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("size")]
        public long Size { get; set; }
        [JsonPropertyName("imageData")]
        public string ImageData { get; set; }
    }
}
