using System;
using System.Collections.Generic;
using System.Linq;

namespace API.Models
{
    public class PostRepository : IPostRepository
    {
        private readonly PostContext _context;

        public PostRepository(PostContext context)
        {
            _context = context;
        }

        public IEnumerable<PostItem> GetAll()
        {
            return _context.PostItems.ToList();
        }

        public void Add(PostItem item)
        {
            _context.PostItems.Add(item);
            _context.SaveChanges();
        }

        public PostItem Find(int id)
        {
            return _context.PostItems.FirstOrDefault(t => t.Id == id);
        }

        public void Remove(int id)
        {
            var entity = _context.PostItems.First(t => t.Id == id);
            _context.PostItems.Remove(entity);
            _context.SaveChanges();
        }

        public void Update(PostItem item)
        {
            _context.PostItems.Update(item);
            _context.SaveChanges();
        }
    }
}