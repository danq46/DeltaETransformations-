using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using System.Xml;

namespace Hyperboliq
{

    class Program
    {
        private static IlluminantItem _userIlluminant;
        private static List<Image> _tileImages = new List<Image>();
        private static List<String> _fileNames = new List<String>();

        private readonly static String _outImagePath = $"{Path.GetTempPath()}HyperBoliq\\ImageOutput.jpeg";

        static void Main(string[] args)
        {
            Image image = null;
            image = null;
            Console.Clear();
            _fileNames.Clear();
            _tileImages.Clear();

            image = GetImage();
            GetTileFolder();

            Console.WriteLine($"Large Image: {_fileNames.First()}");

            var fileNames = String.Empty;

            _fileNames.ForEach(fileName =>
            {
                if (!fileName.Equals(_fileNames.First()))
                    fileNames += fileName;
            });
            Console.WriteLine($"tileImages : {fileNames}");

            IntialiseIlluminantItem();

            CalculateLargeImage(image);
        }

        private static void GetTileFolder()
        {
            var folderInput = String.Empty;

            while (String.IsNullOrEmpty(folderInput))
            {
                try
                {
                    Console.WriteLine("Please add a valid folder path with mutiple tile images, this can be dragged and dropped");
                    folderInput = Console.ReadLine();
                    folderInput = File.GetAttributes(folderInput).HasFlag(FileAttributes.Directory) ? folderInput : String.Empty;
                }
                catch {
                    folderInput = String.Empty;
                }
            }
            
            Console.WriteLine($"Processing file directory: {folderInput}");

            foreach (var filePath in Directory.GetFiles(folderInput)) 
            { 
                var fileExtension = Path.GetExtension(filePath);

                if (!IsValidImageExtension(fileExtension))
                    continue;

                _tileImages.Add(Image.FromFile(filePath));
                _fileNames.Add(filePath);
            }
        }

        private static bool IsValidImageExtension(string fileExtension)
        {
            fileExtension = fileExtension.Replace(".", String.Empty);

            return fileExtension.ToLower().Equals(ImageFormat.Png.ToString().ToLower()) ||
                fileExtension.ToLower().Equals(ImageFormat.Jpeg.ToString().ToLower()) ||
                fileExtension.ToLower().Equals(ImageFormat.Bmp.ToString().ToLower());
        }

        private static Image GetImage()
        {
            Image image = null;

            String imageInput = String.Empty,
                folderInput = String.Empty;

            while (image == null)
            {
                Console.WriteLine("Please add a valid file path to a large image, this can be dragged and dropped");

                imageInput = Console.ReadLine();
                try
                {
                    //drag and drop - just a quick fix
                    if (imageInput.StartsWith("\"") && imageInput.EndsWith("\""))
                    {
                        imageInput = imageInput.Remove(0, 1);
                        imageInput = imageInput.Remove(imageInput.Length - 1, 1);
                    }
                    
                    image = Image.FromFile(imageInput);
                    _fileNames.Add(imageInput);
                }
                catch
                {
                    Console.Clear();
                    imageInput = String.Empty;
                }
            }
            return image;
        }

        private static void IntialiseIlluminantItem()
        {
            String observer = String.Empty,
                illuminantInput = String.Empty;

            while (String.IsNullOrWhiteSpace(observer) || !(observer.Equals("0") || observer.Equals("1")))
            {
                Console.WriteLine($"Please select of Observer by typing 0 - (2° (CIE 1931)) or 1 (10° (CIE 1964))");
                observer = Console.ReadLine();
            }

            var count = 1;
            foreach (var illuminant in IlluminantReference.IlluminantCollection)
            {
                var xVal = observer.Equals("0") ? illuminant.x2 : illuminant.x10;
                var yVal = observer.Equals("0") ? illuminant.y2 : illuminant.y10;
                var zVal = observer.Equals("0") ? illuminant.z2 : illuminant.z10;

                Console.WriteLine($"number  :{count} - {illuminant.illuminant} - X :{xVal} - Y :{xVal} - Z :{xVal}");

                count++;
            }

            while (String.IsNullOrWhiteSpace(illuminantInput))
            {
                Console.WriteLine($"Please select of option by typing the number of the item");
                illuminantInput = Console.ReadLine();
                var isValid = false;
                for (var i = 1; i < count; i++)
                {
                    if (illuminantInput.Equals(i.ToString()))
                        isValid = true;
                }
                illuminantInput = isValid ? illuminantInput : String.Empty;
            }

            _userIlluminant = new IlluminantItem(IlluminantReference.IlluminantCollection[Convert.ToInt32(illuminantInput)]);
        }

        private static void CalculateLargeImage(Image image)
        {
            //setting an array to the amount of peices desired
            Image[] imgarray = new Image[400];

            Console.WriteLine($"Seprating {_fileNames.First()} in 400 parts");

            //looping through height and width to achieve the desired amount of pieces 20x20
            for (var x = 0; x <= 20; x++)
            {
                for (var y = 0; y <= 20; y++)
                {
                    //assigning each index for the desired 20x20 image
                    var index = x * 20 + y;

                    if (index >= imgarray.Length)
                        continue;

                    imgarray[index] = new Bitmap(104, 104);
                    var graphics = Graphics.FromImage(imgarray[index]);
                    graphics.DrawImage(image, new Rectangle(0, 0, 104, 104), new Rectangle(x * 104, y * 104, 104, 104), GraphicsUnit.Pixel);
                    graphics.Dispose();
                }
            }

            _tileImages.ForEach(img =>
            {
                var minIndex = 0;
                var maxVale = Double.MaxValue;

                for (var i = 0; i < imgarray.Length; i++)
                {
                    if(Calc.CompareCieLAB(imgarray[i], img, _userIlluminant) < maxVale) 
                        minIndex = i;
                }
                imgarray[minIndex] = img;
            });

            Int32 width = 0, height = 0;
            foreach (var img in imgarray)
            {
                width += img.Width;
                height += img.Height;
            }

            var count = 0;
            var bitMap = new Bitmap(width, height);
            var graphics1 = Graphics.FromImage(bitMap);
            foreach (var img in imgarray)
            {
                if (count == 0)
                {
                    graphics1.DrawImage(img, new Point(0, 0));
                    width = img.Width;
                }
                else
                {
                    graphics1.DrawImage(img, new Point(width, 0));
                    width += img.Width;
                }
                count++;
            }
            bitMap.Save(_outImagePath, System.Drawing.Imaging.ImageFormat.Jpeg);
        }
    }
}
