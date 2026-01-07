using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace XExplorer.Core.Dictionaries;

internal class DicColors : Dictionary<int, Color>
{
    public static DicColors Instance = new();
    public DicColors()
    {
        this.Add(0, Colors.White);
        this.Add(1, Colors.Red);
        this.Add(2, Colors.Orange);
        this.Add(3, Colors.Yellow);
        this.Add(4, Colors.Green);
        this.Add(5, Colors.Lime);
        this.Add(6, Colors.LightGreen);
        this.Add(7, Colors.Teal);
        this.Add(8, Colors.Cyan);
        this.Add(9, Colors.Blue);
        this.Add(10, Colors.LightBlue);
        this.Add(11, Colors.SkyBlue);
        this.Add(12, Colors.LightSkyBlue);
        this.Add(13, Colors.Purple);
        this.Add(14, Colors.MediumPurple);
        this.Add(15, Colors.Violet);
        this.Add(16, Colors.Pink);
        this.Add(17, Colors.HotPink);
        this.Add(18, Colors.Magenta);
        this.Add(19, Colors.DeepPink);
        this.Add(20, Colors.Gold);
        this.Add(21, Colors.Khaki);
        this.Add(22, Colors.LightCoral);
        this.Add(23, Colors.Salmon);
        this.Add(24, Colors.LightSalmon);
        this.Add(25, Colors.Tomato);
        this.Add(26, Colors.IndianRed);
        this.Add(27, Colors.Plum);
        this.Add(28, Colors.Orchid);
        this.Add(29, Colors.Aquamarine);
        this.Add(30, Colors.Turquoise);
        this.Add(31, Colors.Chartreuse);
        this.Add(32, Colors.SpringGreen);
        this.Add(33, Colors.MediumSpringGreen);
        this.Add(34, Colors.MediumTurquoise);
        this.Add(35, Colors.LightSeaGreen);
        this.Add(36, Colors.MediumAquamarine);
        this.Add(37, Colors.Coral);
        this.Add(38, Colors.DarkOrange);
        this.Add(39, Colors.MediumOrchid);
        this.Add(40, Colors.Thistle);
        this.Add(41, Colors.PaleTurquoise);
        this.Add(42, Colors.PaleGreen);
        this.Add(43, Colors.PaleGoldenrod);
        this.Add(44, Colors.Moccasin);
        this.Add(45, Colors.PapayaWhip);
        this.Add(46, Colors.PeachPuff);
        this.Add(47, Colors.PowderBlue);
        this.Add(48, Colors.LightSteelBlue);
        this.Add(49, Colors.LightCyan);
        this.Add(50, Colors.LightYellow);
        this.Add(51, Colors.Lavender);
        this.Add(52, Colors.LavenderBlush);
        this.Add(53, Colors.MistyRose);
        this.Add(54, Colors.SeaShell);
        this.Add(55, Colors.Honeydew);
        this.Add(56, Colors.AliceBlue);
        this.Add(57, Colors.FloralWhite);
        this.Add(58, Colors.Linen);
        this.Add(59, Colors.OldLace);
        this.Add(60, Colors.Wheat);
    }
}
