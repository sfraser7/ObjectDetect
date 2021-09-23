using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ML;
using Microsoft.ML;
using Microsoft.ML.Transforms.Image;
using ObjectDetect.Server.ML.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ObjectDetect.Server
{
    public class Startup
    {
        private string onnxModelPath;
        private string mlModelPath;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            
            // Set up path to model
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;
            mlModelPath = Path.Combine(assemblyFolderPath, Configuration["MLModelPath"]);

            // This code was run once on first run of app to create model and define
            // the input pipeline.

            //onnxModelPath = Path.Combine(assemblyFolderPath, Configuration["OnnxModelPath"]);
            //var mlContext = new MLContext();

            //var dataView = mlContext.Data.LoadFromEnumerable(new List<InputData>());
            //var pipeline = mlContext.Transforms.ResizeImages(resizing: ImageResizingEstimator.ResizingKind.Fill, outputColumnName: "image", imageWidth: 416, imageHeight: 416, inputColumnName: nameof(InputData.Image))
            //    .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "image"))
            //    .Append(mlContext.Transforms.ApplyOnnxModel(modelFile: onnxModelPath, outputColumnName: "grid", inputColumnName: "image"));

            //var mlModel = pipeline.Fit(dataView);
            //mlContext.Model.Save(mlModel, null, Configuration["MLModelPath"]);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllersWithViews();
            services.AddRazorPages();

            // Add Prediction engine to DI Container for use in api controller.
            services.AddPredictionEnginePool<InputData, Prediction>().FromFile(mlModelPath);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
