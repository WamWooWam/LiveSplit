using WinFormsFontDialog;
using Fetze.WinFormsColor;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Xml;
using System.Drawing.Imaging;

namespace LiveSplit.UI
{
    public class SettingsHelper
    {
        // TODO: This creates a hard dependency on WinFormsFontDialog, which is not ideal.
        public static CustomFontDialog GetFontDialog(Font previousFont, int minSize, int maxSize)
        {
            var dialog = new CustomFontDialog();
            dialog.OriginalFont = previousFont;
            dialog.MinSize = minSize;
            dialog.MaxSize = maxSize;
            return dialog;
        }

        public static string FormatFont(Font font)
        {
            return $"{font.FontFamily.Name} {font.Style}";
        }

        public static void ColorButtonClick(Button button, Control control)
        {
            var picker = new ColorPickerDialog();
            picker.SelectedColorChanged += (s, x) => button.BackColor = picker.SelectedColor;
            picker.SelectedColor = picker.OldColor = button.BackColor;
            picker.ShowDialog(control);
            button.BackColor = picker.SelectedColor;
        }

        public static Color ParseColor(XmlElement colorElement, Color defaultColor = default(Color))
        {
            return colorElement != null
                ? Color.FromArgb(int.Parse(colorElement.InnerText, NumberStyles.HexNumber))
                : defaultColor;
        }

        public static Font GetFontFromElement(XmlElement element)
        {
            if (element != null && !element.IsEmpty)
            {
                // try to find the new XML format
                var fontElement = element["Font"];
                if (fontElement != null)
                {
                    var fontName = fontElement.Attributes["Name"].InnerText;
                    var fontSize = float.Parse(fontElement.Attributes["Size"].InnerText);
                    var fontStyle = (FontStyle)Enum.Parse(typeof(FontStyle), fontElement.Attributes["Style"].InnerText);
                    return new Font(fontName, fontSize, fontStyle);
                }
                else if (element.FirstChild is XmlCDataSection cdata) // otherwise we have to use the old (bad) binary format
                {
                    using (var ms = new MemoryStream(Convert.FromBase64String(cdata.InnerText)))
                    {
#pragma warning disable SYSLIB0011
                        var bf = new BinaryFormatter();
                        return (Font)bf.Deserialize(ms);
#pragma warning restore SYSLIB0011
                    }
                }
            }
            return null;
        }

        public static int CreateSetting(XmlDocument document, XmlElement parent, string elementName, Font font)
        {
            if (document != null)
            {
                var element = document.CreateElement(elementName);

                // write the new XML format
                if (font != null)
                {
                    var fontElement = document.CreateElement("Font");
                    var nameAttribute = document.CreateAttribute("Name");
                    nameAttribute.InnerText = font.Name;
                    fontElement.Attributes.Append(nameAttribute);

                    var sizeAttribute = document.CreateAttribute("Size");
                    sizeAttribute.InnerText = font.Size.ToString();
                    fontElement.Attributes.Append(sizeAttribute);

                    var styleAttribute = document.CreateAttribute("Style");
                    styleAttribute.InnerText = font.Style.ToString();
                    fontElement.Attributes.Append(styleAttribute);

                    element.AppendChild(fontElement);
                }

                parent.AppendChild(element);
            }
            return getFontHashCode(font);
        }

        private static int getFontHashCode(Font font)
        {
            int hash = 17;
            unchecked
            {
                hash = hash * 23 + font.Name.GetHashCode();
                hash = hash * 23 + font.FontFamily.GetHashCode();
                hash = hash * 23 + font.Size.GetHashCode();
                hash = hash * 23 + font.Style.GetHashCode();
            }
            return hash;
        }

        [Obsolete("TODO: Storing images in XML is icky.")]
        public static int CreateSetting(XmlDocument document, XmlElement parent, string elementName, Image image)
        {
            if (document != null)
            {
                var element = document.CreateElement(elementName);
                if (image != null)
                {
                    using var stream = new MemoryStream();
                    image.Save(stream, ImageFormat.Jpeg);

                    // god this is awful
                    var base64String = Convert.ToBase64String(stream.ToArray());
                    element.InnerText = base64String;
                }

                parent.AppendChild(element);
            }

            return image != null ? image.GetHashCode() : 0;
        }

        [Obsolete("TODO: Storing images in XML is icky.")]
        public static Image GetImageFromElement(XmlElement element)
        {
            if (element != null && !element.IsEmpty)
            {
                var base64String = element.InnerText;
                var data = Convert.FromBase64String(base64String);
                using var ms = new MemoryStream(data);

                try
                {
                    // try the old way
                    var bf = new BinaryFormatter();
                    return (Image)bf.Deserialize(ms);
                }
                catch (Exception)
                {
                    ms.Position = 0;

                    // try the new way
                    return Image.FromStream(ms);
                }

            }
            return null;
        }

        public static bool ParseBool(XmlElement boolElement, bool defaultBool = false)
        {
            return boolElement != null
                ? bool.Parse(boolElement.InnerText)
                : defaultBool;
        }

        public static bool TryParseBool(XmlElement boolElement, out bool result, bool defaultBool = false)
        {
            if (boolElement != null && bool.TryParse(boolElement.InnerText, out result))
            {
                return true;
            }

            result = defaultBool;
            return false;
        }

        public static int ParseInt(XmlElement intElement, int defaultInt = 0)
        {
            return intElement != null
                ? int.Parse(intElement.InnerText)
                : defaultInt;
        }

        public static bool TryParseInt(XmlElement intElement, out int result, int defaultInt = 0)
        {
            if (intElement != null && int.TryParse(intElement.InnerText, out result))
            {
                return true;
            }

            result = defaultInt;
            return false;
        }

        public static float ParseFloat(XmlElement floatElement, float defaultFloat = 0f)
        {
            return floatElement != null
                ? float.Parse(floatElement.InnerText.Replace(',', '.'), CultureInfo.InvariantCulture)
                : defaultFloat;
        }

        public static bool TryParseFloat(XmlElement floatElement, out float result, float defaultFloat = 0f)
        {
            if (floatElement != null && float.TryParse(floatElement.InnerText, out result))
            {
                return true;
            }

            result = defaultFloat;
            return false;
        }

        public static double ParseDouble(XmlElement doubleElement, double defaultDouble = 0.0)
        {
            return doubleElement != null
                ? double.Parse(doubleElement.InnerText, CultureInfo.InvariantCulture)
                : defaultDouble;
        }

        public static bool TryParseDouble(XmlElement doubleElement, out double result, double defaultDouble = 0.0)
        {
            if (doubleElement != null && double.TryParse(doubleElement.InnerText, out result))
            {
                return true;
            }

            result = defaultDouble;
            return false;
        }

        public static string ParseString(XmlElement stringElement, string defaultString = null)
        {
            if (defaultString == null)
                defaultString = string.Empty;

            return stringElement != null
                ? stringElement.InnerText
                : defaultString;
        }

        public static TimeSpan ParseTimeSpan(XmlElement timeSpanElement, TimeSpan defaultTimeSpan = default(TimeSpan))
        {
            return timeSpanElement != null
                ? TimeSpan.Parse(timeSpanElement.InnerText)
                : defaultTimeSpan;
        }

        public static bool TryParseTimeSpan(XmlElement timeSpanElement, out TimeSpan result,
            TimeSpan defaultTimeSpan = default(TimeSpan))
        {
            if (timeSpanElement != null && TimeSpan.TryParse(timeSpanElement.InnerText, out result))
            {
                return true;
            }

            result = defaultTimeSpan;
            return false;
        }

        public static XmlElement ToElement<T>(XmlDocument document, string name, T value)
        {
            var element = document.CreateElement(name);
            element.InnerText = value?.ToString();
            return element;
        }

        public static int CreateSetting(XmlDocument document, XmlElement parent, string name, Color color)
        {
            if (document != null)
            {
                var element = document.CreateElement(name);
                element.InnerText = color.ToArgb().ToString("X8");
                parent.AppendChild(element);
            }
            return color.GetHashCode();
        }

        public static int CreateSetting<T>(XmlDocument document, XmlElement parent, string name, T value)
        {
            if (document != null)
            {
                var element = document.CreateElement(name);
                element.InnerText = value?.ToString();
                parent.AppendChild(element);
            }
            return value != null ? value.GetHashCode() : 0;
        }

        public static int CreateSetting(XmlDocument document, XmlElement parent, string name, float value)
        {
            if (document != null)
            {
                var element = document.CreateElement(name);
                element.InnerText = value.ToString(CultureInfo.InvariantCulture);
                parent.AppendChild(element);
            }
            return value.GetHashCode();
        }

        public static int CreateSetting(XmlDocument document, XmlElement parent, string name, double value)
        {
            if (document != null)
            {
                var element = document.CreateElement(name);
                element.InnerText = value.ToString(CultureInfo.InvariantCulture);
                parent.AppendChild(element);
            }
            return value.GetHashCode();
        }

        public static XmlAttribute ToAttribute<T>(XmlDocument document, string name, T value)
        {
            var element = document.CreateAttribute(name);
            element.Value = value?.ToString();
            return element;
        }

        public static T ParseEnum<T>(XmlElement element, T defaultEnum = default(T))
        {
            return element != null
                ? (T)Enum.Parse(typeof(T), element.InnerText)
                : defaultEnum;
        }

        public static bool TryParseEnum<T>(XmlElement element, out T result, T defaultEnum = default(T)) where T : struct
        {
            if (element != null && Enum.TryParse(element.InnerText, out result))
            {
                return true;
            }

            result = defaultEnum;
            return false;
        }

        public static Version ParseVersion(XmlElement element)
        {
            return element != null
                ? Version.Parse(element.InnerText)
                : new Version(1, 0, 0, 0);
        }

        public static bool TryParseVersion(XmlElement element, out Version result)
        {
            if (element != null && Version.TryParse(element.InnerText, out result))
            {
                return true;
            }

            result = new Version(1, 0, 0, 0);
            return false;
        }

        public static Version ParseAttributeVersion(XmlElement element)
        {
            return element.HasAttribute("version")
                ? Version.Parse(element.GetAttribute("version"))
                : new Version(1, 0, 0, 0);
        }

        public static bool TryParseAttributeVersion(XmlElement element, out Version result)
        {
            if (element != null && element.HasAttribute("version")
                && Version.TryParse(element.GetAttribute("version"), out result))
            {
                return true;
            }

            result = new Version(1, 0, 0, 0);
            return false;
        }
    }
}
