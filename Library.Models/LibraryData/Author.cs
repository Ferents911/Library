using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Models.LibraryData
{
    public class Author
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int BornYear { get; set; }

        public List<BookAuthor> BookAuthors { get; set; }

        public Author()
        {
            BookAuthors = new List<BookAuthor>();
        }
    }
}
