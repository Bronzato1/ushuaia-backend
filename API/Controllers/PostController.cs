using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json.Linq;
using API.Models;

namespace API.Controllers
{
    [EnableCors("AureliaSPA")]
    [Route("api/[controller]")]
    public class PostController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly PostContext _postcontext;

        public PostController(IHostingEnvironment hostingEnvironment, PostContext postContext, IPostRepository postItems)
        {
            _hostingEnvironment = hostingEnvironment;
            _postcontext = postContext;
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

            if (post.Content != null)
            {
                var matches = Regex.Matches(post.Content, "<img.+?src=[\"'](.+?)[\"'].+?>", RegexOptions.IgnoreCase); //.Groups[1].Value; 
                foreach (Match match in matches)
                {
                    string urlPath = match.Groups[1].Value;
                    string fileName = System.IO.Path.GetFileName(urlPath);
                    string fullPath = System.IO.Path.GetFullPath("wwwroot/uploads/" + fileName);
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }
            }
            
            PostItems.Remove(id);
            return new NoContentResult();
        }

        [HttpGet("DownloadZip")]
        public IActionResult DownloadZip()
        {
            string webrootPath = _hostingEnvironment.WebRootPath;
            string uploadPath = System.IO.Path.Combine(webrootPath, "uploads");
            string jsonFileName = "export.json";
            string jsonFilePath = System.IO.Path.Combine(webrootPath + @"\", jsonFileName);
            string zipFileName = string.Format("export-{0}.zip", DateTime.Now.ToString("yyyy-MM-dd"));
            string zipFilePath = System.IO.Path.Combine(webrootPath + @"\", zipFileName);

            DirectoryInfo di = new System.IO.DirectoryInfo(webrootPath);
            foreach (var file in di.EnumerateFiles("export*.*")) { file.Delete(); }

            Newtonsoft.Json.Linq.JArray json = new JArray(
                PostItems.GetAll().Select(p => new JObject
                {
                    { "Title", p.Title},
                    { "Creation", p.Creation},
                    { "Content", p.Content}
                })
            );

            System.IO.File.WriteAllText(jsonFilePath, json.ToString());

            ZipFile.CreateFromDirectory(uploadPath, zipFilePath);

            using (ZipArchive archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Update))
            {
                archive.CreateEntryFromFile(jsonFilePath, jsonFileName);
            }

            var fileStream = new System.IO.FileStream(zipFilePath, System.IO.FileMode.Open);
            var memoryStream = new System.IO.MemoryStream();
            fileStream.CopyTo(memoryStream);
            fileStream.Close();
            memoryStream.Position = 0;

            foreach (var file in di.EnumerateFiles("export*.*")) { file.Delete(); }

            return File(memoryStream, "application/octetstream", zipFileName);
        }

        [HttpPost("UploadZip")]
        public async Task<IActionResult> UploadZip(IFormFile file)
        {
            long size = file.Length;

            string webrootPath = _hostingEnvironment.WebRootPath;
            string appRoot = Environment.CurrentDirectory;
            string uploadPath = System.IO.Path.Combine(webrootPath, "uploads");
            string jsonFilePath = System.IO.Path.Combine(uploadPath + @"\", @"export.json");
            string importFilePath = System.IO.Path.Combine(appRoot + @"\", @"App_Data\import.zip");

            if (file.Length == 0) throw new Exception("Le fichier est vide");
            if (file.FileName.EndsWith(".zip") == false) throw new Exception("Le type du fichier n'est pas valide");

            using (var stream = new System.IO.FileStream(importFilePath, System.IO.FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            using (ZipArchive archive = ZipFile.OpenRead(importFilePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string fileName = Path.Combine(uploadPath, entry.Name);
                    if (!System.IO.File.Exists(fileName))
                    {

                        entry.ExtractToFile(fileName);
                    }
                }
            }

            string json = System.IO.File.ReadAllText(jsonFilePath);

            var elements = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(json).ToObject<List<JObject>>();

            foreach(var elm in elements) 
            {
                Console.WriteLine("t = " + elm["Title"]);
                Console.WriteLine("l = " + elm["Content"]);
                Console.WriteLine("c = " + elm["Creation"]);

                PostItem postItem = new PostItem
                {
                    Title = (string)elm["Title"],
                    Content = (string)elm["Content"],
                    Creation = (DateTime)elm["Creation"]
                };

                PostItems.Add(postItem);
            }
            
            if (System.IO.File.Exists(jsonFilePath))
                System.IO.File.Delete(jsonFilePath);

            if (System.IO.File.Exists(importFilePath))
                System.IO.File.Delete(importFilePath);

            return Ok(new { count = elements.Count });
        }

        [HttpGet("ClearAllData")]
        public IActionResult ClearAllData() {
            var rowsAffectedCount = _postcontext.Database.ExecuteSqlCommand("delete from PostItems");
            return Ok(new { count = rowsAffectedCount });
        }
    }
}
