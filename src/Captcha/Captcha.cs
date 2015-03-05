namespace Captcha
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Text;

    public class Captcha
    {
        readonly Random _random = new Random();

        private const int BulletPixelsWashPercent = 400;
        private const int BulletLightPixelsWashPercent = 100;
        private const int MaxSpacesInserted = 2;
        private const int MinFontSize = 82;
        private const int MaxFontSize = 100;
        private const int LetterCount = 3;
        private int _curveWidth = 2;
        private const string WaterMark = "elektrikas.tk";
        const string Spaces = "~. *-_";
        private const float WatermarkWash = .9f;
        private const int MaxRotateAngle = 15;
        private const string ValidLetters = "ERTYUIOPASDFGHJKLZCVBNM"; // some letters removed. Add your own

        protected int Height { get; set; }

        protected int Width { get; set; }

        public int Result { get; set; }

        public Image Image()
        {
            var str = Letters(); //Calculus();

            var randomFontSize = RandomFontSize();
            var randomFontFamily = RandomFontFamily();
            var randomFontStyle = RandomFontStyle();
            var foreFont = new Font(randomFontFamily, randomFontSize, randomFontStyle);
            var waterFont = new Font(randomFontFamily, randomFontSize - 20, randomFontStyle);
            var img = Draw(str, foreFont, waterFont, RandomDarkColor(), Color.White);

            img.Save("bbd.png");

            return img;
        }

        public static Color ChangeColorBrightness(Color color, float correctionFactor)
        {
            var red = (float)color.R;
            var green = (float)color.G;
            var blue = (float)color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }

        private Color RandomDarkColor()
        {
            var code = _random.Next(0x1000000) & 0x7F7F7F;
            var htmlCode = string.Format("#{0:x6}", code);

            return ColorTranslator.FromHtml(htmlCode);
        }

        private FontStyle RandomFontStyle()
        {
            switch (_random.Next(2))
            {
                case 0: return FontStyle.Bold;
                default: return FontStyle.Bold | FontStyle.Italic;
            }
        }

        private FontFamily RandomFontFamily()
        {
            switch (_random.Next(3))
            {
                case 0: return FontFamily.GenericMonospace;
                case 1: return FontFamily.GenericSansSerif;
                default: return FontFamily.GenericSerif;
            }
        }

        private float RandomFontSize()
        {
            return _random.Next(MinFontSize, MaxFontSize);
        }

        private string RandomSpaces()
        {
            return new string(Spaces[_random.Next(Spaces.Count())], _random.Next(1, MaxSpacesInserted));
        }

        private string Letters()
        {
            var sb = new StringBuilder(RandomSpaces());
            for (var i = 0; i < LetterCount; i++)
            {
                var val = ValidLetters[_random.Next(ValidLetters.Count())];
                sb.Append(val).Append(RandomSpaces());
            }

            return sb.ToString();
        }

/*
        private string Calculus()
        {
            double result;
            var n1 = .0;
            var n2 = .0;
            var act = 0;
            do
            {
                try
                {
                    n1 = _random.Next(0, 10);
                    n2 = _random.Next(0, 10);
                    act = _random.Next(0, 4);
                    switch (act)
                    {
                        case 0:
                            result = n1 + n2;
                            break;
                        case 1:
                            result = n1 - n2;
                            break;
                        case 2:
                            result = n1 * n2;
                            break;
                        case 3:
                            result = n1 / n2;
                            break;
                        default:
                            result = -1;
                            break;
                    }
                }
                catch
                {
                    result = -1;
                }

            } while (result < 0 || result > 10 || result != Math.Round(result));

            var retV = new StringBuilder().Append(RandomSpaces()).Append(n1).Append(RandomSpaces());

            switch (act)
            {
                case 0:
                    retV.Append("+");
                    break;
                case 1:
                    retV.Append("-");
                    break;
                case 2:
                    retV.Append("*");
                    break;
                case 3:
                    retV.Append(":");
                    break;
            }

            Result = (int)result;

            return retV.Append(RandomSpaces()).Append(n2).Append(RandomSpaces()).Append("=").ToString();
        }
*/

        private Image Draw(string text, Font foreFont, Font waterFont, Color textColor, Color backColor)
        {
            //first, create a dummy bitmap just to get a graphics object
            var img = new Bitmap(1, 1);
            var drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            var textSize = drawing.MeasureString(text, foreFont);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            Width = (int)(textSize.Width * 2);
            Height = (int)textSize.Height * 5;

            img = new Bitmap(Width, Height);

            drawing = Graphics.FromImage(img);

            drawing.TranslateTransform(25, textSize.Height * 2);
            drawing.RotateTransform(MaxRotateAngle - _random.Next(0, MaxRotateAngle * 2 + 1));

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text

            var waterBrush = new SolidBrush(ChangeColorBrightness(textColor, WatermarkWash));

            drawing.DrawString(WaterMark, waterFont, waterBrush, 0, 0);

            var textBrush = new SolidBrush(textColor);

            var light = ChangeColorBrightness(textColor, .7f);

            // <text color thin curves> 
            
            DrawRandomCurve(textColor, drawing, textSize);

            // </text color thin curves>

            // string by itself:
            drawing.DrawString(text, foreFont, textBrush, 0, 0);

            // <thick background curves thru text>

            _curveWidth += 2;
            DrawRandomCurve(backColor, drawing, textSize);

            // </thick background curves thru text>

            // light dots thru all:
            DrawRandomDots(img, textSize, textColor, light, BulletLightPixelsWashPercent);

            // grid
            DrawRandomGrid(drawing, textSize, textColor);

            // background color dots:
            DrawRandomDots(img, textSize, textColor, backColor, BulletPixelsWashPercent);

            // backgroud color grid:
            DrawRandomGrid(drawing, textSize, backColor);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            return CropWhiteSpace(img);
        }

        private void DrawRandomCurve(Color textColor, Graphics drawing, SizeF textSize)
        {
            var brush = new SolidBrush(textColor);

            var pen = new Pen(brush, _curveWidth);

            var height = (int)(textSize.Height);
            var width = (int)textSize.Width;

            var points = new[]
                              {
                                  new Point(0, 0), 
                                  new Point(0, height), 
                                  new Point(width, 0), 
                                  new Point(0, height/2), 
                                  new Point(width/2, 0), 
                                  new Point(width/2, height/2), 
                                  new Point(width, height/2),
                                  new Point(width/2, height),
                                  new Point(width, height)
                              };
            var rndPoints = points.OrderBy(x => _random.Next()).ToArray();
            
            drawing.DrawClosedCurve(pen, rndPoints);

            /*drawing.DrawBezier(pen, rndPoints[0], rndPoints[1], rndPoints[2], rndPoints[3]);
            drawing.DrawBezier(pen, rndPoints[3], rndPoints[4], rndPoints[5], rndPoints[6]);
            drawing.DrawBezier(pen, rndPoints[6], rndPoints[7], rndPoints[8], rndPoints[0]);*/
        }

        private void DrawRandomDots(Bitmap img, SizeF textSize, Color hitColor, Color color, int washPercent)
        {
            var totalTargetPixels = 0;

            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    var pixel = img.GetPixel(x, y);

                    if (pixel == color) continue;
                    if (pixel != hitColor) continue;

                    totalTargetPixels++;
                }
            }

            //washPercent *= 100; 

            var hitCount = 1;
            do
            {
                var x = _random.Next(0, Width);
                var y = _random.Next(0, Height);
                var pixel = img.GetPixel(x, y);

                //if (pixel == color) continue;
                if (pixel != hitColor) continue;

                img.SetPixel(x, y, color);
                hitCount++;

            } while (totalTargetPixels / hitCount * 100 > washPercent);
        }

        private void DrawRandomGrid(Graphics drawing, SizeF textSize, Color backColor)
        {
            var grid = _random.Next(8, 24);

            var brush = new SolidBrush(backColor);
            var pen = new Pen(brush, .1f);

            for (var i = grid; i < textSize.Height; i += grid)
            {
                drawing.DrawLine(pen, 0, i, textSize.Width, i);
            }

            for (var i = grid; i < textSize.Width; i += grid)
            {
                drawing.DrawLine(pen, i, 0, i, textSize.Height);
            }
        }

        public static Bitmap CropWhiteSpace(Bitmap bmp)
        {
            /////// http://stackoverflow.com/questions/248141/remove-surrounding-whitespace-from-an-image

            var w = bmp.Width;
            var h = bmp.Height;

            Func<int, bool> allWhiteRow = row =>
            {
                for (var i = 0; i < w; ++i)
                {
                    if (bmp.GetPixel(i, row).R != 255) return false;
                }

                return true;
            };

            Func<int, bool> allWhiteColumn = col =>
            {
                for (var i = 0; i < h; ++i)
                {
                    if (bmp.GetPixel(col, i).R != 255) return false;
                }

                return true;
            };

            var topmost = 0;
            for (var row = 0; row < h; ++row)
            {
                if (allWhiteRow(row))
                {
                    topmost = row;
                }
                else
                {
                    break;
                }
            }

            var bottommost = 0;
            for (var row = h - 1; row >= 0; --row)
            {
                if (allWhiteRow(row))
                {
                    bottommost = row;
                }
                else
                {
                    break;
                }
            }

            int leftmost = 0, rightmost = 0;
            for (var col = 0; col < w; ++col)
            {
                if (allWhiteColumn(col))
                {
                    leftmost = col;
                }
                else
                {
                    break;
                }
            }

            for (var col = w - 1; col >= 0; --col)
            {
                if (allWhiteColumn(col))
                {
                    rightmost = col;
                }
                else
                {
                    break;
                }
            }

            if (rightmost == 0)
            {
                rightmost = w; // As reached left
            }

            if (bottommost == 0)
            {
                bottommost = h; // As reached top.
            }

            var croppedWidth = rightmost - leftmost;
            var croppedHeight = bottommost - topmost;

            if (croppedWidth == 0) // No border on left or right
            {
                leftmost = 0;
                croppedWidth = w;
            }

            if (croppedHeight == 0) // No border on top or bottom
            {
                topmost = 0;
                croppedHeight = h;
            }

            try
            {
                var target = new Bitmap(croppedWidth + 2, croppedHeight + 2);
                using (var g = Graphics.FromImage(target))
                {
                    g.Clear(Color.White);
                    g.DrawImage(bmp,
                      new RectangleF(1, 1, croppedWidth + 1, croppedHeight + 1),
                      new RectangleF(leftmost, topmost, croppedWidth, croppedHeight),
                      GraphicsUnit.Pixel);
                }

                return target;
            }
            catch (Exception)
            {
                return bmp;
            }
        }
    }
}
