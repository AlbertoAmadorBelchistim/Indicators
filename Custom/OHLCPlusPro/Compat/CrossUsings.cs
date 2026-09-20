// Platform types that differ between ATAS X (Avalonia, System.Drawing) and the Windows flavors
// (WPF). CrossColor itself comes from the shared global usings of Custom/.
#if CROSS_PLATFORM
global using CrossKey = Avalonia.Input.Key;
global using CrossKeyEventArgs = Avalonia.Input.KeyEventArgs;
global using CrossColors = System.Drawing.Color;
#else
global using CrossKey = System.Windows.Input.Key;
global using CrossKeyEventArgs = System.Windows.Input.KeyEventArgs;
global using CrossColors = System.Windows.Media.Colors;
#endif
