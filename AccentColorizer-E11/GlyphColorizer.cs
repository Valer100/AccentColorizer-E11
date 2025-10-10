using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Forms;

namespace AccentColorizer_E11
{
    class GlyphColorizer
    {
        private const string DEFAULT_COLOR_LIGHT = "#0078D4";
        private const string DEFAULT_COLOR_DARK  = "#4CC2FF";

        private readonly string[] paths;
        private readonly RegistryKey key;

        public GlyphColorizer(string[] paths, RegistryKey key)
        {
            this.paths = paths;
            this.key = key;
        }

        public void ApplyColorization()
        {
            string ImmersiveSystemAccent = Utility.ColorToHex(AccentColors.GetColorByTypeName("ImmersiveSystemAccent"));
            string ImmersiveSystemAccentLight2 = Utility.ColorToHex(AccentColors.GetColorByTypeName("ImmersiveSystemAccentLight2"));

            ColorizeGlyphs("light", ImmersiveSystemAccent, DEFAULT_COLOR_LIGHT);
            ColorizeGlyphs("dark", ImmersiveSystemAccentLight2, DEFAULT_COLOR_DARK);
        }

        public void RevertColorization()
        {
            string ImmersiveSystemAccent = Utility.ColorToHex(AccentColors.GetColorByTypeName("ImmersiveSystemAccent"));
            string ImmersiveSystemAccentLight2 = Utility.ColorToHex(AccentColors.GetColorByTypeName("ImmersiveSystemAccentLight2"));

            ColorizeGlyphs("light", DEFAULT_COLOR_LIGHT, ImmersiveSystemAccent);
            ColorizeGlyphs("dark", DEFAULT_COLOR_DARK, ImmersiveSystemAccentLight2);
        }

        public void ColorizeGlyphs(string theme, string replacementColor, string defaultColor)
        {
            var currentColor = (string)key.GetValue(theme, defaultColor);
            key.SetValue(theme, replacementColor);

            var currentColorSpotlight = Utility.BlendColors("#251840", currentColor, 82);
            var replacementColorSpotlight = Utility.BlendColors("#251840", replacementColor, 82);

            string defaultGradientStartColor, defaultGradientEndColor;
            string currentGradientStartColor, currentGradientEndColor;
            string replacementGradientStartColor, replacementGradientEndColor;

            if ("light".Equals(theme))
            {
                defaultGradientStartColor = defaultColor;
                defaultGradientEndColor = Utility.BlendColors("#C600D2", defaultColor, 117);

                currentGradientStartColor = currentColor;
                currentGradientEndColor = Utility.BlendColors("#C600D2", currentColor, 117);

                replacementGradientStartColor = replacementColor;
                replacementGradientEndColor = Utility.BlendColors("#C600D2", replacementColor, 117);
            }
            else
            {
                defaultGradientStartColor = Utility.BlendColors("#B5FCFF", defaultColor, 49);
                defaultGradientEndColor = Utility.BlendColors("#D2FEE0", defaultColor, 90);

                currentGradientStartColor = Utility.BlendColors("#B5FCFF", currentColor, 49);
                currentGradientEndColor = Utility.BlendColors("#D2FEE0", currentColor, 90);

                replacementGradientStartColor = Utility.BlendColors("#B5FCFF", replacementColor, 49);
                replacementGradientEndColor = Utility.BlendColors("#D2FEE0", replacementColor, 90);
            }

            foreach (var basePath in paths)
            {
                var dir = new DirectoryInfo(basePath + theme);

                foreach (var file in dir.GetFiles("*.svg"))
                {
                    var path = file.FullName;

                    var raw = Utility.ReadFile(path);

                    raw = raw.Replace(currentColor, replacementColor)
                             .Replace(defaultColor, replacementColor)
                             .Replace(currentGradientStartColor, replacementGradientStartColor)
                             .Replace(currentGradientEndColor, replacementGradientEndColor)
                             .Replace(defaultGradientStartColor, replacementGradientStartColor)
                             .Replace(defaultGradientEndColor, replacementGradientEndColor);

                    if ("light".Equals(theme))
                    {
                        // Windows Spotlight has a different color used (WHY?)
                        raw = raw.Replace(currentColorSpotlight, replacementColorSpotlight).Replace("#0C59A4", replacementColorSpotlight);
                    }

                    try
                    {
                        Utility.WriteFile(path, raw);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Utility.RunElevated(Program.ARGUMENT_TAKEOWN);
                        break;
                    }
                }
            }
        }
    }
}
