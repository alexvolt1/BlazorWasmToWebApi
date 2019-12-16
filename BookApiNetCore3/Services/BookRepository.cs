using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookApiNetCore3.Data;
using BookApiNetCore3.Models;

namespace BookApiNetCore3.Services
{
    public class BookRepository : IBookRepository
    {
        private ApplicationDbContext _ApplicationDbContext;

        public BookRepository(ApplicationDbContext ApplicationDbContext)
        {
            _ApplicationDbContext = ApplicationDbContext;
        }

        public bool BookExists(int bookId)
        {
            return _ApplicationDbContext.Books.Any(b => b.Id == bookId);
        }

        public bool BookExists(string bookIsbn)
        {
            return _ApplicationDbContext.Books.Any(b => b.Isbn == bookIsbn);
        }

        public bool CreateBook(List<int> authorsId, List<int> categoriesId, Book book)
        {
            var authors = _ApplicationDbContext.Authors.Where(a => authorsId.Contains(a.Id)).ToList();
            var categories = _ApplicationDbContext.Categories.Where(c => categoriesId.Contains(c.Id)).ToList();

            foreach(var author in authors)
            {
                var bookAuthor = new BookAuthor()
                {
                    Author = author,
                    Book = book
                };
                _ApplicationDbContext.Add(bookAuthor);
            }

            foreach (var category in categories)
            {
                var bookCategory = new BookCategory()
                {
                    Category = category,
                    Book = book
                };
                _ApplicationDbContext.Add(bookCategory);
            }

            _ApplicationDbContext.Add(book);

            return Save();
        }

        public bool DeleteBook(Book book)
        {
            _ApplicationDbContext.Remove(book);
            return Save();
        }

        public Book GetBook(int bookId)
        {
            return _ApplicationDbContext.Books.Where(b => b.Id == bookId).FirstOrDefault();
        }

        public Book GetBook(string bookIsbn)
        {
            return _ApplicationDbContext.Books.Where(b => b.Isbn == bookIsbn).FirstOrDefault();
        }

        public decimal GetBookRating(int bookId)
        {
            var reviews = _ApplicationDbContext.Reviews.Where(r => r.Book.Id == bookId);

            if (reviews.Count() <= 0)
                return 0;

            return ((decimal)reviews.Sum(r => r.Rating) / reviews.Count());
        }

        public ICollection<Book> GetBooks()
        {
            return _ApplicationDbContext.Books.OrderBy(b => b.Title).ToList();
        }

        public bool IsDuplicateIsbn(int bookId, string bookIsbn)
        {
            var book = _ApplicationDbContext.Books.Where(b => b.Isbn.Trim().ToUpper() == bookIsbn.Trim().ToUpper() 
                                                && b.Id != bookId).FirstOrDefault();

            return book == null ? false : true;
        }

        public bool Save()
        {
            var saved = _ApplicationDbContext.SaveChanges();
            return saved >= 0 ? true : false;
        }

        public bool UpdateBook(List<int> authorsId, List<int> categoriesId, Book book)
        {
            var authors = _ApplicationDbContext.Authors.Where(a => authorsId.Contains(a.Id)).ToList();
            var categories = _ApplicationDbContext.Categories.Where(c => categoriesId.Contains(c.Id)).ToList();

            var bookAuthorsToDelete = _ApplicationDbContext.BookAuthors.Where(b=> b.BookId == book.Id);
            var bookCategoriesToDelete = _ApplicationDbContext.BookCategories.Where(b => b.BookId == book.Id);

            _ApplicationDbContext.RemoveRange(bookAuthorsToDelete);
            _ApplicationDbContext.RemoveRange(bookCategoriesToDelete);

            foreach (var author in authors)
            {
                var bookAuthor = new BookAuthor()
                {
                    Author = author,
                    Book = book
                };
                _ApplicationDbContext.Add(bookAuthor);
            }

            foreach (var category in categories)
            {
                var bookCategory = new BookCategory()
                {
                    Category = category,
                    Book = book
                };
                _ApplicationDbContext.Add(bookCategory);
            }

            _ApplicationDbContext.Update(book);

            return Save();
        }
    }
}
