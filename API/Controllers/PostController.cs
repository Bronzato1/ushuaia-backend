using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using System.Text.RegularExpressions;
using System.IO.Compression;
using API.Models;

namespace API.Controllers
{
    [EnableCors("AureliaSPA")]
    [Route("api/[controller]")]
    public class PostController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public PostController(IHostingEnvironment hostingEnvironment, IPostRepository postItems)
        {
            _hostingEnvironment = hostingEnvironment;
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

            var matches = Regex.Matches(post.Content, "<img.+?src=[\"'](.+?)[\"'].+?>", RegexOptions.IgnoreCase); //.Groups[1].Value; 

            foreach (Match match in matches)
            {
                string urlPath = match.Groups[1].Value;
                string fileName = System.IO.Path.GetFileName(urlPath);
                string fullPath = System.IO.Path.GetFullPath("wwwroot/uploads/" + fileName);
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }

            PostItems.Remove(id);
            return new NoContentResult();
        }

        [Route("api/[controller]/DownloadZip")]
        [HttpGet("DownloadZip")]
        public IActionResult DownloadZip()
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            string uploadPath = System.IO.Path.Combine(webRootPath, "uploads");
            string zipName = string.Format("export-{0}.zip", DateTime.Now.ToString("yyyy-MM-dd"));
            string zipPath = System.IO.Path.Combine(webRootPath, "exports", zipName);

            if (System.IO.File.Exists(zipPath))
                System.IO.File.Delete(zipPath);

            ZipFile.CreateFromDirectory(uploadPath, zipPath);

            var stream = new System.IO.FileStream(zipPath, System.IO.FileMode.Open);
            return File(stream, "application/octetstream", zipName);
        }
    }
}
