// Color type used by indicator properties: WPF color on the Windows flavors,
// System.Drawing.Color on ATAS X. ATAS.Indicators provides Convert() to and from
// System.Drawing.Color on every flavor.
#if CROSS_PLATFORM
global using CrossColor = System.Drawing.Color;
#else
global using CrossColor = System.Windows.Media.Color;
#endif
