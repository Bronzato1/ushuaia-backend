using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using API.Models;

namespace API.Controllers
{
    [EnableCors("AureliaSPA")]
    [Route("api/[controller]")]
    public class PostController : Controller
    {
        public PostController(IPostRepository postItems)
        {
            PostItems = postItems;
        }

        public IPostRepository PostItems { get; set; }

        [HttpGet]
        public IEnumerable<PostItem> GetAll()
        {
            return PostItems.GetAll();
        }

        [HttpGet("{id}", Name = "GetPost")]
        public IActionResult GetById(int id)
        {
            var item = PostItems.Find(id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        [HttpPost]
        public IActionResult Create([FromBody] PostItem item)
        {
            if (item == null)
            {
                return BadRequest();
            }
            PostItems.Add(item);
            return CreatedAtRoute("GetPost", new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] PostItem item)
        {
            if (item == null || item.Id != id)
            {
                return BadRequest();
            }

            var post = PostItems.Find(id);
            if (post == null)
            {
                return NotFound();
            }

            post.Title = item.Title;
            post.Content = item.Content;
            post.Creation = item.Creation;

            PostItems.Update(post);
            return new NoContentResult();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var post = PostItems.Find(id);
            if (post == null)
            {
                return NotFound();
            }

            PostItems.Remove(id);
            return new NoContentResult();
        }
    }
}
