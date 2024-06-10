# Face Detection and Recognation Program

This program processes images to detect faces, generate embeddings, and find the nearest images to a given image based on facial embeddings. It uses the `FaceAiSharp` library for face detection and embedding generation, and `ImageSharp` for image processing.

## Prerequisites

Ensure you have the following libraries installed:
- `System.Text.Json`
- `FaceAiSharp`
- `SixLabors.ImageSharp`

## How to Use

### Command Line Arguments

The program expects two command line arguments:
1. `imgPath`: The path to the image file for which you want to find the nearest images.
2. `folderPath`: The path to the folder containing images to be processed.

### Example Usage

```bash
dotnet run <imgPath> <folderPath>
```

### Workflow

1. **Initialize Face Detector and Embeddings Generator**: 
   ```csharp
   var det = FaceAiSharpBundleFactory.CreateFaceDetector();
   var rec = FaceAiSharpBundleFactory.CreateFaceEmbeddingsGenerator();
   ```

2. **Load No Face Images and Embeddings Data**:
   - `no_face_images.json`: Contains paths to images where no face was detected.
   - `images_embeddings.json`: Contains embeddings for processed images.

3. **Process Images**:
   - Detect faces in images from the specified folder.
   - If no face is detected, log the image path to `no_face_images.json`.
   - Generate and store embeddings for images with detected faces.
   - Display progress in the console.

4. **Save Data**:
   - Update and save `no_face_images.json` and `images_embeddings.json` with the latest data.

5. **Find Nearest Images**:
   - Detect and generate embeddings for the specified image.
   - Find and display images from the folder that have the closest embeddings.

### Output

The program outputs the paths of the nearest images to the specified image along with their similarity scores.


## Acknowledgments

- [FaceAiSharp](https://github.com/FaceAiSharp)
- [ImageSharp](https://github.com/SixLabors/ImageSharp)