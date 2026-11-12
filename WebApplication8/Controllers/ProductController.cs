using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication8.Models;

namespace WebApplication8.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private static readonly List<Product> _products = new()
        {
            new Product { Id = 1, Name = "Notebook", Price = 45m },
            new Product { Id = 2, Name = "Pen", Price = 12.5m }
        };
        private static int _nextId = 3;

        // GET is open: no token needed (200)
        [HttpGet]
        public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;

            var items = _products
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                data = items,
                meta = new { page, pageSize, total = _products.Count }
            });
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            return product == null ? NotFound(new { message = "Product not found." }) : Ok(product);
        }

        // Admin only: 401 with no token, 403 for a normal User, 201 for Admin
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Create(Product input)
        {
            input.Id = _nextId++;
            _products.Add(input);
            return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, Product input)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound(new { message = "Product not found." });

            product.Name = input.Name;
            product.Price = input.Price;
            return Ok(product);
        }

        // Admin only: 204 on success
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound(new { message = "Product not found." });

            _products.Remove(product);
            return NoContent();
        }
    }
}