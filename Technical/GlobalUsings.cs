#if CROSS_PLATFORM

global using CrossColor = System.Drawing.Color;
global using CrossKey = Avalonia.Input.Key;
global using CrossColors = System.Drawing.Color;
global using CrossKeyEventArgs = Avalonia.Input.KeyEventArgs;
global using CrossSolidBrush = Utils.Common.UniversalSolidBrush;
global using CrossPen = Utils.Common.UniversalPen;
global using CrossPens = Utils.Common.UniversalPens;

#else

global using CrossColor = System.Windows.Media.Color;
global using CrossKey = System.Windows.Input.Key;
global using CrossColors = System.Windows.Media.Colors;
global using CrossKeyEventArgs = System.Windows.Input.KeyEventArgs;
global using CrossSolidBrush = System.Drawing.SolidBrush;
global using CrossPen = System.Drawing.Pen;
global using CrossPens = System.Drawing.Pens;

#endif