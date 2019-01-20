using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Svz.Common
{
    public class Book
    {
        public virtual int BookId { get; set; }
        public virtual string Title { get; set; }
        public virtual string Isbn { get; set; }
        public virtual string Ean { get; set; }
        public virtual string Category { get; set; }
        public virtual string Publisher { get; set; }
        public virtual string Authors { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual bool IsInStock { get; set; }
        public virtual string Description { get; set; }
        public virtual short PagesCount { get; set; }
        public virtual short YearPublished { get; set; }
        public virtual decimal FormatWidthCm { get; set; }
        public virtual decimal FormatHeightCm { get; set; }
        public virtual string Cover { get; set; }
    }

    public class BookMap : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> entity)
        {
            entity.ToTable("Book");
            entity.HasKey(x => x.BookId);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(500);
            entity.Property(t => t.Isbn).HasMaxLength(30);
            entity.Property(t => t.Ean).IsRequired().HasMaxLength(30);
            entity.Property(t => t.Category).HasMaxLength(255);
            entity.Property(t => t.Publisher).HasMaxLength(255);
            entity.Property(t => t.Authors).HasMaxLength(255);
            entity.Property(x => x.IsActive).IsRequired();
            entity.Property(x => x.IsInStock).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(3000).IsRequired();
            entity.Property(x => x.PagesCount).IsRequired();
            entity.Property(x => x.YearPublished).IsRequired();
            entity.Property(x => x.FormatWidthCm).IsRequired();
            entity.Property(x => x.FormatHeightCm).IsRequired();
            entity.Property(x => x.Cover).HasMaxLength(255);
        }
    }

    public class BookDbContext : DbContext
    {
        public BookDbContext(DbContextOptions<BookDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BookMap());
        }
    }
}
