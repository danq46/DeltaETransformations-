using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hyperboliq
{

    class Program
    {
        private static IlluminantItem _userIlluminant;
        private static List<Image> _tileImages = new List<Image>();
        private static List<String> _fileNames = new List<String>();

        private readonly static String _outImageFolderPath = $"{Path.GetTempPath()}HyperBoliq",
            _outImagePath = $"{Path.GetTempPath()}HyperBoliq\\ImageOutput.jpeg";

        static void Main(string[] args)
        {
            Image image = null;
            image = null;
            Console.Clear();
            _fileNames.Clear();
            _tileImages.Clear();

            image = GetImage();
            GetTileFolder();

            var fileNames = String.Empty;

            _fileNames.ForEach(fileName =>
            {
                if (!fileName.Equals(_fileNames.First()))
                    fileNames += fileName;
            });

            IntialiseIlluminantItem();

            CalculateLargeImage(image);
        }

        private static void GetTileFolder()
        {
            var folderInput = String.Empty;

            while (_tileImages.Count == 0)
            {
                while (String.IsNullOrEmpty(folderInput))
                {
                    try
                    {
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.WriteLine($"Please add a valid file path to a folder of tile images, this can be dragged and dropped");

                        folderInput = Console.ReadLine();

                        //drag and drop - just a quick fix
                        if (folderInput.StartsWith("\"") && folderInput.EndsWith("\""))
                        {
                            folderInput = folderInput.Remove(0, 1);
                            folderInput = folderInput.Remove(folderInput.Length - 1, 1);
                        }

                        folderInput = File.GetAttributes(folderInput).HasFlag(FileAttributes.Directory) ? folderInput : String.Empty;
                    }
                    catch
                    {
                        folderInput = String.Empty;
                    }
                }
                Int32 folderCount = Directory.GetFiles(folderInput).Count();

                Console.WriteLine($"Processing ({folderCount} Files)");

                Parallel.ForEach(Directory.GetFiles(folderInput), filePath =>
                {
                    var fileExtension = Path.GetExtension(filePath);

                    if (IsValidImageExtension(fileExtension))
                    {
                        _tileImages.Add(Image.FromFile(filePath));
                        _fileNames.Add(Path.GetFileName(filePath));
                    }
                });

                var tileImageCount = _tileImages.Count;
                if (tileImageCount <= 0)
                {
                    folderInput = String.Empty;
                }
                else
                    Console.WriteLine($"Tile Images found ({tileImageCount})");
            }
        }

        private static Boolean IsValidImageExtension(string fileExtension)
        {
            fileExtension = fileExtension.Replace(".", String.Empty);

            return fileExtension.ToLower().Equals("jpeg") ||
                fileExtension.ToLower().Equals("jpg") ||
                fileExtension.ToLower().Equals("png") ||
                fileExtension.ToLower().Equals("bmp");
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
                    _fileNames.Add(Path.GetFileName(imageInput));

                    Console.WriteLine($"Success... File({_fileNames.First()})");
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
                Console.WriteLine($"\nPlease select of Observer by typing 0 - (2° (CIE 1931)) or 1 (10° (CIE 1964))");
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

            _userIlluminant = new IlluminantItem(IlluminantReference.IlluminantCollection[Convert.ToInt32(illuminantInput)], observer);
        }

        private static void CalculateLargeImage(Image image)
        {
            //setting an array to the amount of peices desired
            var imgPieces = new List<Image>();

            Int32 imageWidth = Convert.ToInt32((double)image.Width / 400),
                imageHeight = Convert.ToInt32((double)image.Height/ 400);

            //looping through height and width to achieve the desired amount of pieces 20x20
            for (var x = 0; x < 20; x++)
            {
                for (var y = 0; y < 20; y++)
                {
                    //assigning each index for the desired 20x20 image
                    var index = x * 20 + y;

                    imgPieces.Add(new Bitmap(imageWidth, imageHeight));
                    using (var graphics = Graphics.FromImage(imgPieces.Last()))
                    {
                        graphics.DrawImage(image, new Rectangle(0, 0, imageWidth, imageHeight), new Rectangle(x * imageWidth, y * imageHeight, imageWidth, imageHeight), GraphicsUnit.Pixel);
                    }
                }
            }
            
            var imgCount = 0;
            var imgList = new List<Image>();
            var proccessEnd = (double)_tileImages.Count * imgPieces.Count;
            double smallValue = Double.MaxValue;

            imgPieces.ForEach(img => {
                Image currentImage = img;

                _tileImages.ForEach(tileImage => {
                    var proccessPrecentage = Math.Round(proccessEnd / (double)imgCount);

                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write($"processing tile images... {proccessPrecentage}%");

                    var calculateddiff = Calc.CompareCieLAB(img, tileImage, _userIlluminant);
                    if (calculateddiff <= smallValue) 
                    {
                        currentImage = new Bitmap(tileImage, currentImage.Size);
                    }
                    imgCount++;
                });
                imgList.Add(currentImage);
                imgCount++;
            });

            using (var bitMap = new Bitmap(image.Width, image.Height))
            {
                var width = 0;
                using (var graphics1 = Graphics.FromImage(bitMap))
                {
                    Int32 counter = 1;
                    imgList.ForEach(img => {
                        graphics1.DrawImage(img, width, 0, img.Width, img.Height);
                        width += (Int32)((double)image.Width / 400);
                    });
                    counter++;
                }

                if (!Directory.Exists(_outImageFolderPath))
                    Directory.CreateDirectory(_outImageFolderPath);

                bitMap.Save(_outImagePath, System.Drawing.Imaging.ImageFormat.Jpeg);


                Console.WriteLine($"\nFile located in : {_outImagePath}");
            }

            try
            {
                // opens the folder in explorer
                Process.Start("explorer.exe", _outImagePath);
            }
            catch
            {

            }
        }
    }
}
