using ZeroFormatter;

namespace Svz.Common
{
    [ZeroFormattable]
    public class BookView
    {
        [Index(0)] public virtual int BookId { get; set; }
        [Index(1)] public virtual string Title { get; set; }
        [Index(2)] public virtual string Isbn { get; set; }
        [Index(3)] public virtual string Ean { get; set; }
        [Index(4)] public virtual string Category { get; set; }
        [Index(5)] public virtual string Publisher { get; set; }
        [Index(6)] public virtual string Authors { get; set; }
        [Index(7)] public virtual bool IsActive { get; set; }
        [Index(8)] public virtual bool IsInStock { get; set; }
        [Index(9)] public virtual string Description { get; set; }
        [Index(10)] public virtual short PagesCount { get; set; }
        [Index(11)] public virtual short YearPublished { get; set; }
        [Index(12)] public virtual decimal FormatWidthCm { get; set; }
        [Index(13)] public virtual decimal FormatHeightCm { get; set; }
        [Index(14)] public virtual string Cover { get; set; }
    }
}