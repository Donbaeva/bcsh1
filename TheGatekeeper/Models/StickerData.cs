namespace TheGatekeeper.Models
{
    public enum StickerType
    {
        YellowPostIt,
        PinkSticker,
        RedAlert,
        BlueMemo
    }

    public class StickerData
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public StickerType StickerType { get; set; }
    }
}