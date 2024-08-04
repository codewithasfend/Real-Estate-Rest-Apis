using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.Data;
using RealEstateApi.Models;
using System.Linq;
using System.Security.Claims;

namespace RealEstateApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertiesController : ControllerBase
    {
        private ApiDbContext _dbContext;
        public PropertiesController(ApiDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("PropertyList")]
        [Authorize]
        public IActionResult GetProperties(int categoryId)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var result = _dbContext.Properties.Where(c => c.CategoryId == categoryId)
                 .Select(p => new
                 {
                     p.Id,
                     p.Name,
                     p.Address,
                     p.Price,
                     ImageUrl = $"{baseUrl}/{p.ImageUrl}"

                 });

            if (!result.Any())
            {
                return NotFound();
            }

            return Ok(result);

        }


        [HttpGet("PropertyDetail")]
        [Authorize]
        public IActionResult GetPropertyDetail(int id)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var user = _dbContext.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null) return NotFound();

            var propertyResult = _dbContext.Properties.Find(id);

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

            if (propertyResult != null)
            {
                var result = _dbContext.Properties.Where(p => p.Id == id)
                 .Select(p => new
                 {
                     p.Id,
                     p.Name,
                     p.Detail,
                     p.Address,
                     p.Price,
                     ImageUrl = $"{baseUrl}/{p.ImageUrl}",
                     p.User.Phone,
                 }).FirstOrDefault();
                return Ok(result);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("TrendingProperties")]
        [Authorize]
        public IActionResult GetTrendingProperties()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

            var result = _dbContext.Properties.Where(c => c.IsTrending == true)
              .Select(p => new
              {
                  p.Id,
                  p.Name,
                  p.Address,
                  p.Price,
                  ImageUrl = $"{baseUrl}/{p.ImageUrl}",
                  p.IsTrending
              });

            if (!result.Any())
            {
                return NotFound();
            }
            return Ok(result);

        }

        [HttpGet("SearchProperties")]
        [Authorize]
        public IActionResult GetSearchProperties(string address)
        {
            var propertiesResult = _dbContext.Properties.Select(p => new { p.Id, p.Name, p.Address }).Where(p => p.Address.Contains(address));
            if (propertiesResult == null)
            {
                return NotFound();
            }

            return Ok(propertiesResult);
        }


        [HttpPost]
        [Authorize]
        public IActionResult Post([FromBody] Property property)
        {
            if (property == null)
            {
                return NoContent();
            }
            else
            {
                var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var user = _dbContext.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user == null) return NotFound();
                property.IsTrending = false;
                property.UserId = user.Id;
                _dbContext.Properties.Add(property);
                _dbContext.SaveChanges();
                return StatusCode(StatusCodes.Status201Created);
            }
        }

    }
}
