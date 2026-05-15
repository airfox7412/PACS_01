using SkiaSharp;
using System;

namespace Api.Library;

internal class Captcha
{
    internal static SKRect MeasureText(string text, SKPaint paint)
    {
        SKRect rect = new SKRect();
        paint.MeasureText(text, ref rect);
        return rect;
    }

    [Obsolete]
    public static SKPaint CreatePaint()
    {
        return CreatePaint(SKColors.White, "Arial", 30, SKTypefaceStyle.Normal);
    }

    [Obsolete]
    public static SKPaint CreatePaint(SKColor color, string fontName, float fontSize, SKTypefaceStyle fontStyle)
    {
        SKTypeface font = SKTypeface.FromFamilyName(fontName, fontStyle);

        SKPaint paint = new SKPaint();

        paint.IsAntialias = true;
        paint.Color = color;
        // paint.StrokeCap = SKStrokeCap.Round;
        paint.Typeface = font;
        paint.TextSize = fontSize;

        return paint;
    }

    [Obsolete]
    internal static byte[] GetCaptcha(string captchaText)
    {
        byte[] imageBytes;

        int image2dX;
        int image2dY;

        int compensateDeepCharacters;

        using (SKPaint drawStyle = CreatePaint())
        {
            compensateDeepCharacters = (int)drawStyle.TextSize / 5;
            if (System.StringComparer.Ordinal.Equals(captchaText, captchaText.ToUpperInvariant()))
                compensateDeepCharacters = 0;

            var size = MeasureText(captchaText, drawStyle);
            image2dX = (int)size.Width + 20; 
            image2dY = (int)size.Height + 20 + compensateDeepCharacters;
        }

        using SKBitmap image2d = new SKBitmap(image2dX, image2dY, SKColorType.Bgra8888, SKAlphaType.Premul);
        using SKCanvas canvas = new SKCanvas(image2d);
        canvas.DrawColor(SKColors.Black); // Clear 

        using (SKPaint drawStyle = CreatePaint())
            canvas.DrawText(captchaText, 0 + 10,
                image2dY - 10 - compensateDeepCharacters, drawStyle);
        using SKImage img = SKImage.FromBitmap(image2d);
        using SKData p = img.Encode(SKEncodedImageFormat.Png, 100);
        imageBytes = p.ToArray();

        return imageBytes;
    }

}