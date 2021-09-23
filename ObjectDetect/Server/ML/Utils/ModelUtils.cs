using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Image;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace ObjectDetect.Server.ML.Utils
{
    public class InputData
    {
        [ImageType(416, 416)]
        public Bitmap Image { get; set; }
    }
    
    public class Prediction
    {
        [ColumnName("grid")]
        public float[] Labels { get; set; }
    }
}
