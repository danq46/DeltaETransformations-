using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Hyperboliq
{
    struct IlluminantReference
    {
        #region
        /*
        http://www.easyrgb.com/en/math.php#text3 - YZ (Tristimulus) Reference values of a perfect reflecting diffuser

        Illuminant	X2	Y2	Z2	X10	Y10	Z10	 
        A	109.850	100.000	35.585	111.144	100.000	35.200	Incandescent/tungsten
        B	99.0927	100.000	85.313	99.178;	100.000	84.3493	Old direct sunlight at noon
        C	98.074	100.000	118.232	97.285	100.000	116.145	Old daylight
        D50	96.422	100.000	82.521	96.720	100.000	81.427	ICC profile PCS
        D55	95.682	100.000	92.149	95.799	100.000	90.926	Mid-morning daylight
        D65	95.047	100.000	108.883	94.811	100.000	107.304	Daylight, sRGB, Adobe-RGB
        D75	94.972	100.000	122.638	94.416	100.000	120.641	North sky daylight
        E	100.000	100.000	100.000	100.000	100.000	100.000	Equal energy
        F1	92.834	100.000	103.665	94.791	100.000	103.191	Daylight Fluorescent
        F2	99.187	100.000	67.395	103.280	100.000	69.026	Cool fluorescent
        F3	103.754	100.000	49.861	108.968	100.000	51.965	White Fluorescent
        F4	109.147	100.000	38.813	114.961	100.000	40.963	Warm White Fluorescent
        F5	90.872	100.000	98.723	93.369	100.000	98.636	Daylight Fluorescent
        F6	97.309	100.000	60.191	102.148	100.000	62.074	Lite White Fluorescent
        F7	95.044	100.000	108.755	95.792	100.000	107.687	Daylight fluorescent, D65 simulator
        F8	96.413	100.000	82.333	97.115	100.000	81.135	Sylvania F40, D50 simulator
        F9	100.365	100.000	67.868	102.116	100.000	67.826	Cool White Fluorescent
        F10	96.174	100.000	81.712	99.001	100.000	83.134	Ultralume 50, Philips TL85
        F11	100.966	100.000	64.370	103.866	100.000	65.627	Ultralume 40, Philips TL84
        F12	108.046	100.000	39.228	111.428	100.000	40.353	Ultralume 30, Philips TL83
        */
        #endregion

        public static ReadOnlyCollection<Illuminant> IlluminantCollection = new ReadOnlyCollection<Illuminant>(new List<Illuminant>() {
            new Illuminant("A",109.850,100.000,35.585,111.144,100.000,35.200,"Incandescent/tungsten"),
            new Illuminant("B",99.0927,100.000,85.313,99.178,100.000,84.3493,"Olddirectsunlightatnoon"),
            new Illuminant("C",98.074,100.000,118.232,97.285,100.000,116.145,"Olddaylight"),
            new Illuminant("D50",96.422,100.000,82.521,96.720,100.000,81.427,"ICCprofilePCS"),
            new Illuminant("D55",95.682,100.000,92.149,95.799,100.000,90.926,"Mid-morningdaylight"),
            new Illuminant("D65",95.047,100.000,108.883,94.811,100.000,107.304,"Daylight,sRGB,Adobe-RGB"),
            new Illuminant("D75",94.972,100.000,122.638,94.416,100.000,120.641,"Northskydaylight"),
            new Illuminant("E",100.000,100.000,100.000,100.000,100.000,100.000,"Equalenergy"),
            new Illuminant("F1",92.834,100.000,103.665,94.791,100.000,103.191,"DaylightFluorescent"),
            new Illuminant("F2",99.187,100.000,67.395,103.280,100.000,69.026,"Coolfluorescent"),
            new Illuminant("F3",103.754,100.000,49.861,108.968,100.000,51.965,"WhiteFluorescent"),
            new Illuminant("F4",109.147,100.000,38.813,114.961,100.000,40.963,"WarmWhiteFluorescent"),
            new Illuminant("F5",90.872,100.000,98.723,93.369,100.000,98.636,"DaylightFluorescent"),
            new Illuminant("F6",97.309,100.000,60.191,102.148,100.000,62.074,"LiteWhiteFluorescent"),
            new Illuminant("F7",95.044,100.000,108.755,95.792,100.000,107.687,"Daylightfluorescent,D65simulator"),
            new Illuminant("F8",96.413,100.000,82.333,97.115,100.000,81.135,"SylvaniaF40,D50simulator"),
            new Illuminant("F9",100.365,100.000,67.868,102.116,100.000,67.826,"CoolWhiteFluorescent"),
            new Illuminant("F10",96.174,100.000,81.712,99.001,100.000,83.134,"Ultralume50,PhilipsTL85"),
            new Illuminant("F11",100.966,100.000,64.370,103.866,100.000,65.627,"Ultralume40,PhilipsTL84"),
            new Illuminant("F12",108.046,100.000,39.228,111.428,100.000,40.353,"Ultralume30,PhilipsTL83")
        });
    }
    class Illuminant : IIlluminant
    {
        public String illuminant { get; set; }
        public Double x2 { get; }
        public Double y2 { get; }
        public Double z2 { get; }
        public Double x10 { get; }
        public Double y10 { get; }
        public Double z10 { get; }
        public String note { get; }
        public Illuminant(String Illuminant , Double X2 ,Double Y2 ,Double Z2 ,Double X10 ,Double Y10 ,Double Z10, String Note) 
        { 
                illuminant = Illuminant;
                x2 = X2;
                y2 = Y2;
                z2 = Z2;
                x10 = X10;
                y10 = Y10;
                z10 = Z10;
                note = Note;
        }
    }
    interface IIlluminant
    {
        String illuminant { get;}
        Double x2 { get; }
        Double y2 { get; }
        Double z2 { get; }
        Double x10 { get; }
        Double y10 { get; }
        Double z10 { get; }
        String note { get; }
        }

    class IlluminantItem
    {
        private double _refX,
            _refY,
            _refZ;

        public Double RefX { get { return _refX; } }
        public Double RefY { get { return _refY; } }
        public Double RefZ { get { return _refZ; } }

        public IlluminantItem(Illuminant illuminant)
        {

        }
    }
}
