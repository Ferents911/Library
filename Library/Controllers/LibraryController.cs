using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Library;
using Library.Models.LibraryData;
using System.Web;
using System.Web.Http;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using Library.Models.ApiData;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
using HttpPutAttribute = Microsoft.AspNetCore.Mvc.HttpPutAttribute;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using HttpDeleteAttribute = Microsoft.AspNetCore.Mvc.HttpDeleteAttribute;
using Newtonsoft.Json;
using FromBodyAttribute = Microsoft.AspNetCore.Mvc.FromBodyAttribute;

namespace Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibraryController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public LibraryController(ApplicationContext context)
        {
            _context = context;
        }

        [Route("getbooks")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks([FromBody] PagingParameterModel pagingparametermodel)
        {
            var books = (from book in _context.Books.OrderBy(b => b.Name) select book).AsQueryable();
  
            int count = books.Count();

            int CurrentPage = pagingparametermodel.pageNumber;
  
            int PageSize = pagingparametermodel.pageSize;

            int TotalCount = count;

            int TotalPages = (int)Math.Ceiling(count / (double)PageSize);
            
            var items = books.Skip((CurrentPage - 1) * PageSize).Take(PageSize);

            var previousPage = CurrentPage > 1 ? "Yes" : "No";

            var nextPage = CurrentPage < TotalPages ? "Yes" : "No";
 
            var paginationMetadata = new
            {
                totalCount = TotalCount,
                pageSize = PageSize,
                currentPage = CurrentPage,
                totalPages = TotalPages,
                previousPage,
                nextPage
            };

            HttpContext.Response.Headers.Add("Paging-Headers", JsonConvert.SerializeObject(paginationMetadata));

            return await items.ToListAsync();

        }

        [Route("getbook/{id}")]
        [HttpGet]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            return book;
        }

        [Route("editbook/{id}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditBook(int id, Book book)
        {
            if (id != book.Id)
            {
                return BadRequest();
            }

            _context.Entry(book).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        [Route("addbook/{id}")]
        [HttpPost]
        public async Task<ActionResult<Book>> AddBook(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBook", new { id = book.Id }, book);
        }

        [Route("removebook/{id}")]
        [HttpDelete]
        public async Task<ActionResult<Book>> RemoveBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return book;
        }

        // requestedAuthor - here example of bodies for next api endpoints
        //{
        //    "Name": "AuthorC",
        //    "BornYear": 1889
        //}


        // adding existing author from db to book author list (id - book id ) 
        [Route("books/{id}/addnewauthor")]
        [HttpPut]
        public async Task<ActionResult<IEnumerable<Author>>> AddNewAuthorToBook(int id, [FromBody] Author requestedAuthor)
        {
            var requestedName = requestedAuthor.Name.ToLower();
            var book = await _context.Books.FindAsync(id);
            var author = _context.Authors.Where(a => a.Name.ToLower().Equals(requestedName) && a.BornYear.Equals(requestedAuthor.BornYear)).FirstOrDefault();
            if (id != book.Id || author == null)
            {
                return new JsonResult("Make shure that current book or author exists in library!");
            }

            BookAuthor bookAuthor = new BookAuthor
            {
                BookId = id,
                AuthorId = author.Id
            };

            book.BookAuthors.Add(bookAuthor);

            _context.Entry(book).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();

        }

        [Route("getbooksbyauthor")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetAllBooksByAuthorName([FromBody] Author requestedAuthor)
        {
            var requestedName = requestedAuthor.Name.ToLower();
            if (string.IsNullOrWhiteSpace(requestedName) || requestedAuthor.BornYear <= 0)
            {
                return new JsonResult("invalid entry data!");
            }

            var author = await _context.Authors.Include(a => a.BookAuthors).ThenInclude(ba => ba.Book).Where(a => a.Name.ToLower().Equals(requestedName) && a.BornYear.Equals(requestedAuthor.BornYear)).FirstOrDefaultAsync();
           
            var books = author.BookAuthors.Select(ba => ba.Book).AsEnumerable();


            if (books.Count() == 0)
            {
                return NotFound();
            }

            return books.ToList();

        }

        [Route("getauthors")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Author>>> GetAuthors([FromBody] PagingParameterModel pagingParameterModel)
        {
            var authors = (from author in _context.Authors.OrderBy(b => b.Name) select author).AsQueryable();

            int count = authors.Count();

            int CurrentPage = pagingParameterModel.pageNumber;

            int PageSize = pagingParameterModel.pageSize;

            int TotalCount = count;

            int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

            var items = authors.Skip((CurrentPage - 1) * PageSize).Take(PageSize);

            var previousPage = CurrentPage > 1 ? "Yes" : "No";

            var nextPage = CurrentPage < TotalPages ? "Yes" : "No";

            var paginationMetadata = new
            {
                totalCount = TotalCount,
                pageSize = PageSize,
                currentPage = CurrentPage,
                totalPages = TotalPages,
                previousPage,
                nextPage
            };

            HttpContext.Response.Headers.Add("Paging-Headers", JsonConvert.SerializeObject(paginationMetadata));

            return await items.ToListAsync();

        }

        [Route("getauthor/{id}")]
        [HttpGet]
        public async Task<ActionResult<Author>> GetAuthor(int id)
        {
            var author = await _context.Authors.FindAsync(id);

            if (author == null)
            {
                return NotFound();
            }

            return author;
        }

        [Route("editauthor/{id}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditAuthor(int id, Author author)
        {
            if (id != author.Id)
            {
                return BadRequest();
            }

            _context.Entry(author).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AuthorExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        [Route("addauthor/{id}")]
        [HttpPost]
        public async Task<ActionResult<Author>> AddAuthor(Author author)
        {
            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAuthor", new { id = author.Id }, author);
        }

        [Route("removeauthor/{id}")]
        [HttpDelete]
        public async Task<ActionResult<Author>> RemoveAuthor(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();

            return author;
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(b => b.Id == id);
        }

        private bool AuthorExists(int id)
        {
            return _context.Authors.Any(a => a.Id == id);
        }
    }
}
