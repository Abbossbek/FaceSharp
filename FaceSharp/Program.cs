using System.Text.Json;

using FaceAiSharp;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

if (args.Length == 2 && args[0] is string imgPath && File.Exists(imgPath) && args[1] is string folderPath && Directory.Exists(folderPath))
{
    var det = FaceAiSharpBundleFactory.CreateFaceDetector();
    var rec = FaceAiSharpBundleFactory.CreateFaceEmbeddingsGenerator();

    string noFaceImagesFilePath = Path.Combine(folderPath, "no_face_images.json");
    var noFaceImages = new List<string>();
    if (File.Exists(noFaceImagesFilePath))
        noFaceImages = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(noFaceImagesFilePath));

    string imagesEmbeddingsFilePath = Path.Combine(folderPath, "images_embeddings.json");
    var images = Directory.GetFiles(folderPath, "*.jpg", SearchOption.AllDirectories).ToList();

    if (noFaceImages == null)
        noFaceImages = new List<string>();
    noFaceImages.RemoveAll(x => !images.Contains(x));

    images.RemoveAll(x => noFaceImages.Contains(x));

    var imagesEmbeddingsDict = new Dictionary<string, float[]>();
    if (File.Exists(imagesEmbeddingsFilePath))
    {
        imagesEmbeddingsDict = JsonSerializer.Deserialize<Dictionary<string, float[]>>(File.ReadAllText(imagesEmbeddingsFilePath));
    }
    if (imagesEmbeddingsDict != null)
    {
        var notProcessedImages = images.Except(imagesEmbeddingsDict.Keys);
        foreach (var img in notProcessedImages)
        {
            using var image = SixLabors.ImageSharp.Image.Load<Rgb24>(img);
            var face = det.DetectFaces(image).FirstOrDefault();
            if (face == default)
            {
                Console.Write($"\rNo face detected in {img}");
                Console.WriteLine();
                noFaceImages.Add(img);
                continue;
            }
            rec.AlignFaceUsingLandmarks(image, face.Landmarks!);
            var imgEmbeddings = rec.GenerateEmbedding(image);
            imagesEmbeddingsDict[img] = imgEmbeddings;
            //write progress in percentage to console, if old value has in console then remove it
            Console.Write($"\rImages processing: {imagesEmbeddingsDict.Count}/{images.Count} {(((float)imagesEmbeddingsDict.Count / images.Count) * 100).ToString("#.##")}%");

        }
        Console.WriteLine();
        File.WriteAllText(noFaceImagesFilePath, JsonSerializer.Serialize(noFaceImages));
        var deletedImages = imagesEmbeddingsDict.Keys.Except(images);
        foreach (var img in deletedImages)
        {
            imagesEmbeddingsDict.Remove(img);
        }
        File.WriteAllText(imagesEmbeddingsFilePath, JsonSerializer.Serialize(imagesEmbeddingsDict));
    }
    var sImage = SixLabors.ImageSharp.Image.Load<Rgb24>(imgPath);
    var sFace = det.DetectFaces(sImage).FirstOrDefault();
    rec.AlignFaceUsingLandmarks(sImage, sFace.Landmarks!);
    var embedding = rec.GenerateEmbedding(sImage);
    var nearestImages = imagesEmbeddingsDict.OrderByDescending(x => FaceAiSharp.Extensions.GeometryExtensions.Dot(x.Value, embedding)).TakeWhile(x => FaceAiSharp.Extensions.GeometryExtensions.Dot(x.Value, embedding) > 0.42);
    Console.WriteLine($"Nearest images to {imgPath}:");
    foreach (var img in nearestImages)
    {
        Console.WriteLine($"{img.Key}|{FaceAiSharp.Extensions.GeometryExtensions.Dot(img.Value, embedding)}");
    }
    Console.ReadKey();
}
