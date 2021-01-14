using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hyperboliq
{
    class AverageRGB
    {
        private Double _avgR,
            _avgG, 
            _avgB;

        public Double AverageRed { get { return _avgR; } }
        public Double AverageGreen { get { return _avgG; } }
        public Double AverageBlue { get { return _avgB; } }

        public AverageRGB(Image image) 
        {
            Double R = 0, G = 0, B = 0, total = 0;

            using (var bitMap = new Bitmap(image))
            {
                unsafe 
                {
                    var bitMapData = bitMap.LockBits(new Rectangle(0, 0, bitMap.Width, bitMap.Height), ImageLockMode.ReadWrite, bitMap.PixelFormat);

                    Int32 pixelHeight = bitMapData.Height,
                        //3 bytes per pixel
                        pixelWidht = bitMapData.Width * 3;

                    //first pixel
                    byte* pointer = (byte*)bitMapData.Scan0;

                    Parallel.For(0, pixelHeight, x => {
                        var scanLine = pointer + (x * bitMapData.Stride);

                        for (var i = 0; x < pixelWidht; x = x + 3) 
                        {
                            B = scanLine[i];
                            G = scanLine[i + 1];
                            R = scanLine[i + 2];
                            total++;
                        }
                    });
                    bitMap.UnlockBits(bitMapData);
                }
            }

            _avgR = R / total;
            _avgG = G / total;
            _avgB = B / total;
        }
    }

    class XYZ
    {
        #region reference
        /*  
        http://www.easyrgb.com/en/math.php - Standard-RGB → XYZ
        //sR, sG and sB (Standard RGB) input range = 0 ÷ 255
        //X, Y and Z output refer to a D65/2° standard illuminant.

        var_R = ( sR / 255 )
        var_G = ( sG / 255 )
        var_B = ( sB / 255 )

        if ( var_R > 0.04045 ) var_R = ( ( var_R + 0.055 ) / 1.055 ) ^ 2.4
        else                   var_R = var_R / 12.92
        if ( var_G > 0.04045 ) var_G = ( ( var_G + 0.055 ) / 1.055 ) ^ 2.4
        else                   var_G = var_G / 12.92
        if ( var_B > 0.04045 ) var_B = ( ( var_B + 0.055 ) / 1.055 ) ^ 2.4
        else                   var_B = var_B / 12.92

        var_R = var_R * 100
        var_G = var_G * 100
        var_B = var_B * 100

        X = var_R * 0.4124 + var_G * 0.3576 + var_B * 0.1805
        Y = var_R * 0.2126 + var_G * 0.7152 + var_B * 0.0722
        Z = var_R * 0.0193 + var_G * 0.1192 + var_B * 0.9505
        */
        #endregion

        private Double _x, _y, _z;

        public Double X { get { return _x; } }
        public Double Y { get { return _y; } }
        public Double Z { get { return _z; } }
        public XYZ(AverageRGB averageRGB)
        {
            var rgb = new Dictionary<String, Double>() { { "Red", averageRGB.AverageRed}, { "Green", averageRGB.AverageGreen }, { "Blue", averageRGB.AverageBlue } };

            foreach (var color in rgb)
            {
                var colorValue = Convert.ToDouble(color.Value / 225);
                colorValue = colorValue > 0.04045 ? Math.Pow((colorValue + 0.055) / 1.055, 2.4) : colorValue / 12.92;
                colorValue = colorValue * 100;
            }

            _x = rgb["Red"] * 0.4124 + rgb["Green"] * 0.3576 + rgb["Blue"] * 0.1805;
            _y = rgb["Red"] * 0.2126 + rgb["Green"] * 0.7152 + rgb["Blue"] * 0.0722;
            _z = rgb["Red"] * 0.0193 + rgb["Green"] * 0.1192 + rgb["Blue"] * 0.9505;
        }
    }

    class CieLAB
    {
        #region reference
        /* 
        http://www.easyrgb.com/en/math.php - XYZ → CIE-L*ab
        var_X = X / Reference - X
        var_Y = Y / Reference - Y
        var_Z = Z / Reference - Z

        if (var_X > 0.008856) var_X = var_X ^ (1 / 3)
        else var_X = (7.787 * var_X) + (16 / 116)
        if (var_Y > 0.008856) var_Y = var_Y ^ (1 / 3)
        else var_Y = (7.787 * var_Y) + (16 / 116)
        if (var_Z > 0.008856) var_Z = var_Z ^ (1 / 3)
        else var_Z = (7.787 * var_Z) + (16 / 116)

        CIE - L * = (116 * var_Y) - 16
        CIE - a * = 500 * (var_X - var_Y)
        CIE - b * = 200 * (var_Y - var_Z)
        */
        #endregion

        private Double _cieL, _cieA, _cieB;
        public Double CieL { get { return _cieL; } }
        public Double CieA { get { return _cieA; } }
        public Double CieB { get { return _cieB; } }

        public CieLAB(XYZ xYZ, IlluminantItem illuminant) 
        {
            var x = illuminant.RefX - xYZ.X;
            var y = illuminant.RefY - xYZ.Y;
            var z = illuminant.RefZ - xYZ.Z;

            var xyz = new Dictionary<String, Double>() { { "X", x }, { "Y", y }, { "Z", z } };

            foreach (var pos in xyz)
            {
                var posValue = (pos.Value > 0.008856) ? Math.Pow(pos.Value, (1 / 3)) : (7.787 * pos.Value) + (16 / 116);
            }

            _cieL = (116 * xyz["Y"]) - 16;
            _cieA = 500 * (xyz["X"] - xyz["Y"]);
            _cieB = 200 * (xyz["Y"] - xyz["Z"]);
        }
    }

    class Calc {
        public static Double CompareCieLAB(Image img1, Image img2, IlluminantItem _userIlluminant) 
        {
            CieLAB CieLABItem1 = TranFormImageToCieLAB(img1, _userIlluminant),
                CieLABItem2 = TranFormImageToCieLAB(img2, _userIlluminant);

            return CompareCieLAB(CieLABItem1, CieLABItem2);
        }

        private static CieLAB TranFormImageToCieLAB(Image image, IlluminantItem _userIlluminant)
        {
            var averageRGB = new AverageRGB(image);
            var XYZ = new XYZ(averageRGB);
            var cieLAB = new CieLAB(XYZ, _userIlluminant);
            return cieLAB;
        }

        private static Double CompareCieLAB(CieLAB cieLAB1, CieLAB cieLAB2)
        {
            #region Reference
            /*
            http://www.easyrgb.com/en/math.php - Delta E* CIE
            Delta E* = sqrt( ( ( CIE-L*1 - CIE-L*2 ) ^ 2 )   + ( ( CIE-a*1 - CIE-a*2 ) ^ 2 ) + ( ( CIE-b*1 - CIE-b*2 ) ^ 2 ) )
            */
            #endregion

            return Math.Sqrt(Math.Pow((cieLAB1.CieL - cieLAB2.CieL), 2) + Math.Pow((cieLAB1.CieA - cieLAB2.CieA), 2) + Math.Pow((cieLAB1.CieB - cieLAB2.CieB), 2));
        }
    }
}
