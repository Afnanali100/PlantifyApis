using Microsoft.EntityFrameworkCore;
using PlantifyApp.Core.Interfaces;
using PlantifyApp.Core.Models;
using PlantifyApp.Repository.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlantifyApp.Repository.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly Identity.IdentityConnection  dbcontext;

        public GenericRepository(IdentityConnection dbcontext)
        {
            this.dbcontext = dbcontext;
        }

        public async Task Add(T entity)
        {
            try
            {
                await dbcontext.Set<T>().AddAsync(entity);
                await dbcontext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine("An error occurred while saving changes:");
                Console.WriteLine(ex.ToString());
                throw; // Re-throw the exception to propagate it up the call stack
            }
        }



        public async Task Update(T entity)
        {
            dbcontext.Set<T>().Update(entity);
            await dbcontext.SaveChangesAsync();
        }

        public async Task Delete(T entity)
        {
            dbcontext.Set<T>().Remove(entity);
            await dbcontext.SaveChangesAsync();
        }


        public async Task<IReadOnlyList<T>> GetAllAsync()
        {

            if (typeof(T) == typeof(Posts))
            {
                var result = await dbcontext.Posts
                .Include(p => p.Comments)
                .Include(p => p.Likes)
                .ToListAsync();

                // Map the Post entities to PostDto objects
                var posts = result.Select(post => new Posts
                {
                    post_id = post.post_id,
                    user_id = post.user_id,
                    description = post.description,
                    image_name = post.image_name,
                    video_name = post.video_name,
                    creation_date = post.creation_date,
                    Comments = post.Comments.Select(c => new Comments
                    {
                        comment_id = c.comment_id,
                        user_id = c.user_id,
                        post_id = post.post_id,
                        content = c.content,
                        creation_date = c.creation_date
                    }).ToList(),
                    Likes = post.Likes.Select(c => new Likes
                    {
                        like_id = c.like_id,
                        user_id = c.user_id,
                        post_id = post.post_id,
                        creation_date = c.creation_date
                    }).ToList(),
                    LikesCount = post.Likes.Count() // Count of likes
                }).ToList();

                return posts.Cast<T>().ToList().AsReadOnly();
            }
            return await dbcontext.Set<T>().ToListAsync();
        }


        public async Task<IReadOnlyList<Posts>> GetAllPostsForSpecifcUser(string user_id)
        {
            var result = await dbcontext.Posts
                .Include(p => p.Comments)
                .Include(p => p.Likes)
                .Where(x => x.user_id == user_id)
                .ToListAsync();

            // Map the Post entities to PostDto objects
            var posts = result.Select(post => new Posts
            {
                post_id = post.post_id,
                user_id = post.user_id,
                description = post.description,
                image_name = post.image_name,
                video_name = post.video_name,
                creation_date = post.creation_date,
                Comments = post.Comments.Select(c => new Comments
                {
                    comment_id = c.comment_id,
                    user_id = c.user_id,
                    post_id=post.post_id,
                    content = c.content,
                    creation_date = c.creation_date
                }).ToList(),
                Likes = post.Likes.Select(c => new Likes
                {
                    like_id = c.like_id,
                    user_id = c.user_id,
                    post_id=post.post_id,
                    creation_date = c.creation_date
                }).ToList(),
                LikesCount = post.Likes.Count() // Count of likes
            }).ToList();

            return posts.AsReadOnly();
        }


        public async Task DeleteByPostIdAsync(int post_id)
        {
            if (typeof(T) == typeof(Comments))
            {
                // Use Where to filter the comments by post_id
                var comments = await dbcontext.Comments.Where(c => c.post_id == post_id).ToListAsync();

                // Remove the filtered comments
                dbcontext.Comments.RemoveRange(comments);

                // Save changes to the database
                await dbcontext.SaveChangesAsync();
            }
           else if (typeof(T) == typeof(Likes))
            {
                // Use Where to filter the comments by post_id
                var likes = await dbcontext.likes.Where(c => c.post_id == post_id).ToListAsync();

                // Remove the filtered comments
                dbcontext.likes.RemoveRange(likes);

                // Save changes to the database
                await dbcontext.SaveChangesAsync();
            }
            else
            {
                // Handle other types or throw an exception if T is not supported
                throw new NotSupportedException($"The type {typeof(T)} is not supported by this method.");
            }
        }





        public async Task<T> GetByIdAsync(int id)
        {
            
            return await dbcontext.Set<T>().FindAsync(id);
        }

        public async Task<IReadOnlyList<Plants>> GetAllPlantsDetailsForSpecifcUser(string user_id)
        {
            var result = await dbcontext.Plants
                .Where(x => x.user_id == user_id)
                .ToListAsync();

            return result;
        }


        public async Task<Likes> GetByUserAndPostIdAsync(string userId, int postId)
        {
            return await dbcontext.likes.FirstOrDefaultAsync(l => l.user_id == userId && l.post_id == postId);
        }

        public async Task DeleteByUserIdAsync(string user_id)
        {

            if (typeof(T) == typeof(Posts))
            {
                // Use Where to filter the comments by post_id
                var posts = await dbcontext.Posts.Where(c => c.user_id == user_id).ToListAsync();

                // Remove the filtered comments
                dbcontext.Posts.RemoveRange(posts);

                // Save changes to the database
                await dbcontext.SaveChangesAsync();
            }

           else if (typeof(T) == typeof(Comments))
            {
                // Use Where to filter the comments by post_id
                var comments = await dbcontext.Comments.Where(c => c.user_id == user_id).ToListAsync();

                // Remove the filtered comments
                dbcontext.Comments.RemoveRange(comments);

                // Save changes to the database
                await dbcontext.SaveChangesAsync();
            }
            else if (typeof(T) == typeof(Likes))
            {
                // Use Where to filter the comments by post_id
                var likes = await dbcontext.likes.Where(c => c.user_id == user_id).ToListAsync();

                // Remove the filtered comments
                dbcontext.likes.RemoveRange(likes);

                // Save changes to the database
                await dbcontext.SaveChangesAsync();
            }
            else
            {
                // Handle other types or throw an exception if T is not supported
                throw new NotSupportedException($"The type {typeof(T)} is not supported by this method.");
            }
        }
    }
 
}
