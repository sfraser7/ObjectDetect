using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using ObjectDetect.Server.ML.Utils;
using ObjectDetect.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ObjectDetect.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ObjectDetectionController : ControllerBase
    {
        private readonly PredictionEnginePool<InputData, Prediction> predictionEngine;
        private readonly YoloOutputParser outputParser = new YoloOutputParser();
        private const int IMAGE_INPUT_SIZE = 416;

        public ObjectDetectionController(PredictionEnginePool<InputData, Prediction> predictionEngine)
        {
            this.predictionEngine = predictionEngine;
        }

        [HttpPost]
        public async Task<IActionResult> DetectObject(ImageInfo image)
        {
            // Remove characters added by frontend to display image.
            string base64im = image.ImageData.Split(',')[1];
            
            // Convert base64 string to byte array
            byte[] buffer = Convert.FromBase64String(base64im);

            MemoryStream ms = new MemoryStream(buffer);
           
            // Now convert to Bitmap for inference.
            Bitmap im = (Bitmap)Bitmap.FromStream(ms);

            InputData inputData = new InputData { Image = im };

            // Predict probabilities.
            var probs = predictionEngine.Predict(inputData).Labels;

            var bboxes = outputParser.ParseOutputs(probs);

            // Uncomment if you want to limit the amount of bounding boxes produced.
            //bboxes = outputParser.FilterBoundingBoxes(bboxes, 5, .5F);

            // Annotate the image with labels and detections.
            var annotatedIm = DrawBoundingBox(im, bboxes);

            // Convert the image back to base64.
            using MemoryStream m = new MemoryStream();
            annotatedIm.Save(m, annotatedIm.RawFormat);
            byte[] img = m.ToArray();

            string newBase64im = Convert.ToBase64String(img);
            var result = new ImageInfo { ImageData = $"data:image/jpg;base64,{newBase64im}" };

            return Ok(result);
        }

        public static Image DrawBoundingBox(Bitmap image, IList<YoloBoundingBox> yoloBoundingBoxes)
        {
            var originalHeight = image.Height;
            var originalWidth = image.Width;
            foreach (var box in yoloBoundingBoxes)
            {
                // bounding box dimensions
                var x = (uint)Math.Max(box.Dimensions.X, 0);
                var y = (uint)Math.Max(box.Dimensions.Y, 0);
                var width = (uint)Math.Min(originalWidth - x, box.Dimensions.Width);
                var height = (uint)Math.Min(originalHeight - y, box.Dimensions.Height);

                // fit to current image size
                x = (uint)originalWidth * x / IMAGE_INPUT_SIZE;
                y = (uint)originalHeight * y / IMAGE_INPUT_SIZE;
                width = (uint)originalWidth * width / IMAGE_INPUT_SIZE;
                height = (uint)originalHeight * height / IMAGE_INPUT_SIZE;

                using Graphics thumbnailGraphic = Graphics.FromImage(image);
                thumbnailGraphic.CompositingQuality = CompositingQuality.HighQuality;
                thumbnailGraphic.SmoothingMode = SmoothingMode.HighQuality;
                thumbnailGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // define Text Options
                Font drawFont = new Font("Arial", 12, FontStyle.Bold);
                SizeF size = thumbnailGraphic.MeasureString(box.Label, drawFont);
                SolidBrush fontBrush = new SolidBrush(Color.Black);
                Point atPoint = new Point((int)x, (int)y - (int)size.Height - 1);

                // define bbox options
                Pen pen = new Pen(box.BoxColor, 3.2f);
                SolidBrush colorBrush = new SolidBrush(box.BoxColor);

                // draw object label. 
                thumbnailGraphic.FillRectangle(colorBrush, (int)x, (int)(y - size.Height - 1), (int)size.Width, (int)size.Height);
                thumbnailGraphic.DrawString(box.Label, drawFont, fontBrush, atPoint);

                // draw bounding box
                thumbnailGraphic.DrawRectangle(pen, x, y, width, height);
            }
            return image;
        }
    }

}
