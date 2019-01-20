using Nest;

namespace Svz.Common
{
    public class BookSearchView
    {
        [Keyword] public int BookId { get; set; }

        [Text] public string Title { get; set; }

        [Keyword] public string Isbn { get; set; }

        [Keyword] public string Ean { get; set; }

        [Keyword] public string Category { get; set; }

        [Keyword] public string Publisher { get; set; }

        [Keyword] public string Authors { get; set; }

        [Keyword] public bool IsActive { get; set; }

        [Keyword] public bool IsInStock { get; set; }

        [Text] public string Description { get; set; }

        [Keyword] public short PagesCount { get; set; }

        [Keyword] public short YearPublished { get; set; }

        [Keyword] public decimal FormatWidthCm { get; set; }

        [Keyword] public decimal FormatHeightCm { get; set; }

        [Keyword] public string Cover { get; set; }
    }
}