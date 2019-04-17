using System.Collections.Generic;

namespace API.Models
{
    public interface IPostRepository
    {
        void Add(PostItem item);
        IEnumerable<PostItem> GetAll();
        PostItem Find(int id);
        void Remove(int id);
        void Update(PostItem item);
    }
}