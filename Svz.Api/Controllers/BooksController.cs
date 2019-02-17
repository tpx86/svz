using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Svz.Common;

namespace Svz.Api.Controllers
{
    [Route("api/books")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly BookService _service;

        public BooksController()
        {
            _service = new BookService();
        }

        [HttpGet("ping")]
        public ActionResult<string> Ping()
        {
            return Ok("pong");
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookView>>> GetList()
        {
            return Ok(await _service.GetBooks());
        }

        [HttpGet("{bookId}")]
        public ActionResult<BookView> GetById(int bookId)
        {
            if (bookId <= 0)
                return BadRequest("Invalid bookId");

            var resp = _service.GetById(bookId);
            HttpContext.Response.Headers.Add("X-From-Redis", resp.FromCache.ToString());
            
            return Ok(resp.Value);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<BookView>>> Search([FromQuery] string query)
        {
            if (string.IsNullOrEmpty(query) || query.Length > 100)
                return BadRequest("Invalid query");

            return Ok(await _service.Search(query));
        }
    }
}