using PKHeX.Core;
using SkiaSharp;
using SkiaSharp.QrCode;

namespace PKHeX.Drawing.Misc;

public static class QREncode
{
    public static SKBitmap GenerateQRCode(DataMysteryGift mg) => GenerateQRCode(QRMessageUtil.GetMessage(mg));
    public static SKBitmap GenerateQRCode(PKM pk) => GenerateQRCode(QRMessageUtil.GetMessage(pk));

    public static SKBitmap GenerateQRCode7(PK7 pk7, int box = 0, int slot = 0, int copies = 1)
    {
        byte[] data = QR7.GenerateQRData(pk7, box, slot, copies);
        var msg = QRMessageUtil.GetMessage(data);
        return GenerateQRCode(msg, ppm: 4);
    }

    private static SKBitmap GenerateQRCode(string msg, int ppm = 4)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(msg, ECCLevel.Q);
        SKBitmap qr = new(ppm * data.ModuleMatrix.Count * ppm / 2, ppm * data.ModuleMatrix.Count * ppm / 2);
        using SKCanvas canvas = new(qr);
        canvas.Render(data, new(0, 0, qr.Width, qr.Height), SKColors.White, SKColors.Black);
        canvas.Flush();
        return qr;
    }
}
