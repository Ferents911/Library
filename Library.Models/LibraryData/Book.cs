using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Models.LibraryData
{
    public class Book
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int Year { get; set; }

        public int PagesCount { get; set; }

        public List<BookAuthor> BookAuthors { get; set; }

        public Book()
        {
            BookAuthors = new List<BookAuthor>();
        }
    }
}
