using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using PlantifyApp.Apis.Dtos;
using PlantifyApp.Apis.Errors;
using PlantifyApp.Core.Interfaces;
using PlantifyApp.Core.Models;
using System.Security.Claims;

namespace PlantifyApp.Apis.Controllers
{

    public class CommunityController : ApiBaseController
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IGenericRepository<Posts> postRepo;
        private readonly IMapper mapper;
        private readonly IGenericRepository<Comments> commentRepo;
        private readonly IGenericRepository<Likes> likeRepo;

        public CommunityController(UserManager<ApplicationUser> userManager, IGenericRepository<Posts> postRepo, IMapper mapper, IGenericRepository<Comments> commentRepo, IGenericRepository<Likes> likeRepo)
        {
            this.userManager = userManager;
            this.postRepo = postRepo;
            this.mapper = mapper;
            this.commentRepo = commentRepo;
            this.likeRepo = likeRepo;
        }

        private readonly string imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "Assest", "CommunityImages");
        private readonly string videosFolder = Path.Combine(Directory.GetCurrentDirectory(), "Assest", "CommunityVideos");

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("create-post")]
        public async Task<ActionResult> CreatePost(string? description, IFormFile? image, IFormFile? video)
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                if (!string.IsNullOrEmpty(email))
                {
                    var user = await userManager.FindByEmailAsync(email);

                    if (user != null)
                    {
                        string imageName = "";
                        string videoName = "";

                        // Process image file
                        if (image != null && image.Length > 0)
                        {
                            imageName = DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(image.FileName);
                            string imagePath = Path.Combine(imagesFolder, imageName);
                            using (var stream = new FileStream(imagePath, FileMode.Create))
                            {
                                await image.CopyToAsync(stream);
                            }
                            imageName = Path.GetFileName(imagePath); // Get only the filename without the path
                        }

                        // Process video file
                        if (video != null && video.Length > 0)
                        {
                            videoName = DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(video.FileName);
                            string videoPath = Path.Combine(videosFolder, videoName);
                            using (var stream = new FileStream(videoPath, FileMode.Create))
                            {
                                await video.CopyToAsync(stream);
                            }
                            videoName = Path.GetFileName(videoPath); // Get only the filename without the path
                        }

                        var postDto = new PostDto()
                        {
                            user_id = user.Id,
                            description = description,
                            image_name = imageName,
                            video_name = videoName,
                            creation_date = DateTime.Now
                        };

                        var post = mapper.Map<PostDto, Posts>(postDto);
                        await postRepo.Add(post);

                        return Ok(new
                        {
                            message = "Post Created Successfully",
                            statusCode = 200
                        });
                    }
                    else
                    {
                        return NotFound(new ApiErrorResponde(404, "This User does not exist"));
                    }
                }
                else
                {
                    return BadRequest(new ApiErrorResponde(400, "Email claim not found"));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponde(500, "Error occurred while creating the post"));
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("get-all-users-posts")]
        public async Task<ActionResult<IReadOnlyList<PostDto>>> GetAllPosts()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    var posts = await postRepo.GetAllAsync(); // Await the result here

                    if (posts != null && posts.Any()) // Check if there are any posts
                    {
                        // Map posts to PostDto if needed
                        var postDtos = mapper.Map<IReadOnlyList<Posts>, IReadOnlyList<PostDto>>(posts);

                        var requestUrl = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value;
                        foreach (var postDto in postDtos)
                        {
                            if (!string.IsNullOrEmpty(postDto.image_name))
                            {
                                postDto.image_name = requestUrl + "/Assest/CommunityImages/" + postDto.image_name;
                            }
                            if (!string.IsNullOrEmpty(postDto.video_name))
                            {
                                postDto.video_name = requestUrl + "/Assest/CommunityVideos/" + postDto.video_name;
                            }
                        }
                        return Ok(postDtos);
                    }
                    return NotFound(new ApiErrorResponde(404, "There are no posts available"));
                }
                return BadRequest(new ApiErrorResponde(400, "You are not authorized"));
            }
            return BadRequest(new ApiErrorResponde(400, "You are not authorized"));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("get-all-posts-for-specific-user")]
        public async Task<ActionResult<IReadOnlyList<PostDto>>> GetAllUserPosts()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    var posts = await postRepo.GetAllPostsForSpecifcUser(user.Id); // Await the result here

                    if (posts != null && posts.Any()) // Check if there are any posts
                    {
                        // Map posts to PostDto if needed
                        var postDtos = mapper.Map<IReadOnlyList<Posts>, IReadOnlyList<PostDto>>(posts);

                        var requestUrl = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value;
                        foreach (var postDto in postDtos)
                        {
                            if (!string.IsNullOrEmpty(postDto.image_name))
                            {
                                postDto.image_name = requestUrl + "/Assest/CommunityImages/" + postDto.image_name;
                            }
                            if (!string.IsNullOrEmpty(postDto.video_name))
                            {
                                postDto.video_name = requestUrl + "/Assest/CommunityVideos/" + postDto.video_name;

                            }
                        }
                        return Ok(postDtos);
                    }
                    return NotFound(new ApiErrorResponde(404, "There are no posts available"));
                }
                return BadRequest(new ApiErrorResponde(400, "You are not authorized"));
            }
            return BadRequest(new ApiErrorResponde(400, "You are not authorized"));
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("delete-user-post")]
        public async Task<ActionResult<IReadOnlyList<PostDto>>> DeletePost(int post_id)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    var post = await postRepo.GetByIdAsync(post_id);

                    if (post != null)
                    {
                        string imagename = "", videoname = "";
                        var requestUrl = Path.Combine(Directory.GetCurrentDirectory(), "Assest");

                        if (!string.IsNullOrEmpty(post.image_name))
                        {
                            imagename = Path.Combine(requestUrl, "CommunityImages", post.image_name);
                        }
                        if (!string.IsNullOrEmpty(post.video_name))
                        {
                            videoname = Path.Combine(requestUrl, "CommunityVideos", post.video_name);
                        }

                        try
                        {
                            if (System.IO.File.Exists(imagename))
                            {
                                System.IO.File.Delete(imagename);
                            }
                            if (System.IO.File.Exists(videoname))
                            {
                                System.IO.File.Delete(videoname);
                            }

                            // Delete the post from the repository after deleting the files
                            await postRepo.Delete(post);

                            return Ok(new
                            {
                                message = "Post Deleted Successfully!",
                                StatusCode = 200
                            });
                        }
                        catch (Exception ex)
                        {
                            // Log the exception and return an error response
                            return StatusCode(500, new ApiErrorResponde(500, "Internal Server Error: " + ex.Message));
                        }
                    }
                    return NotFound(new ApiErrorResponde(404, "This post is not available"));
                }
                return BadRequest(new ApiErrorResponde(400, "You are not authorized"));
            }
            return BadRequest(new ApiErrorResponde(400, "You are not authorized"));
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("update-user-post")]
        public async Task<ActionResult> UpdatePost(int post_id, string? description, IFormFile? image, IFormFile? video)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    var post = await postRepo.GetByIdAsync(post_id);

                    if (post != null)
                    {
                        // Check if the current user is the owner of the post
                        if (post.user_id == user.Id)
                        {
                            // Update the description if provided
                            if (!string.IsNullOrEmpty(description))
                            {
                                post.description = description;
                            }

                            // Update the image if provided
                            if (image != null)
                            {
                                // Generate a unique file name for the new image
                                var newImageName = DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(image.FileName);
                                // Save the new image file to the CommunityImages folder
                                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Assest", "CommunityImages", newImageName);
                                using (var stream = new FileStream(imagePath, FileMode.Create))
                                {
                                    await image.CopyToAsync(stream);
                                }
                                // Delete the old image file
                                if (!string.IsNullOrEmpty(post.image_name))
                                {
                                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "Assest", "CommunityImages", post.image_name);
                                    if (System.IO.File.Exists(oldImagePath))
                                    {
                                        System.IO.File.Delete(oldImagePath);
                                    }
                                }
                                // Update the image name in the post
                                post.image_name = newImageName;
                            }

                            // Update the video if provided (similar logic to image update)
                            if (video != null)
                            {
                                // Generate a unique file name for the new video
                                var newVideoName = DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(video.FileName);
                                var videoPath = Path.Combine(Directory.GetCurrentDirectory(), "Assest", "CommunityVideos", newVideoName);
                                using (var stream = new FileStream(videoPath, FileMode.Create))
                                {
                                    await video.CopyToAsync(stream);
                                }
                                if (!string.IsNullOrEmpty(post.video_name))
                                {
                                    var oldVideoPath = Path.Combine(Directory.GetCurrentDirectory(), "Assest", "CommunityVideos", post.video_name);
                                    if (System.IO.File.Exists(oldVideoPath))
                                    {
                                        System.IO.File.Delete(oldVideoPath);
                                    }
                                }
                                post.video_name = newVideoName;
                            }

                            // Update the post in the repository
                            await postRepo.Update(post);

                            return Ok(new
                            {
                                message = "Post Updated Successfully!",
                                StatusCode = 200
                            });
                        }
                        else
                        {
                            return BadRequest(new ApiErrorResponde(400, "You are not authorized to update this post"));
                        }
                    }
                    else
                    {
                        return NotFound(new ApiErrorResponde(404, "This post does not exist"));
                    }
                }
                else
                {
                    return BadRequest(new ApiErrorResponde(400, "You are not authorized"));
                }
            }
            else
            {
                return BadRequest(new ApiErrorResponde(400, "You are not authorized"));
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("create-comment")]
        public async Task<ActionResult> CreateComment(int post_id, string comment)
        {

            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    var post = await postRepo.GetByIdAsync(post_id);
                    if (post.user_id == user.Id)
                    {
                        var commentdto = new CommentDto
                        {
                            user_id = user.Id,
                            post_id = post.post_id,
                            content = comment,
                            creation_date = DateTime.Now,
                        };
                        var Comment =mapper.Map<CommentDto,Comments>(commentdto);
                        await commentRepo.Add(Comment);
                        return Ok(new
                        {
                            message = "Comment created Successfully!",
                            statusCode = 200
                        });

                    }
                    return BadRequest(new ApiErrorResponde(500, "internal server error"));

                }

                return BadRequest(new ApiErrorResponde(400, "You are not Authorized"));


            }
            return BadRequest(new ApiErrorResponde(400, "You are not Authorized"));

        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("delete-comment")]
        public async Task<ActionResult> DeleteComment(int post_id, int comment_id)
        {

            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    var post = await postRepo.GetByIdAsync(post_id);
                    if (post.user_id == user.Id)
                    {
                        var comment = await commentRepo.GetByIdAsync(comment_id);
                        if (comment != null)
                        {

                            await commentRepo.Delete(comment);
                            return Ok(new
                            {
                                message = "Comment Deleted Successfully!",
                                statusCode = 200
                            });
                        }
                        return NotFound(new ApiErrorResponde(404, "ThIs Comment is not Exist"));

                    }
                    return BadRequest(new ApiErrorResponde(500, "internal server error"));

                }

                return BadRequest(new ApiErrorResponde(400, "You are not Authorized"));


            }
            return BadRequest(new ApiErrorResponde(400, "You are not Authorized"));

        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("update-comment")]
        public async Task<ActionResult> UpdateComment(int post_id, int comment_id,string comment)
        {

            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    var post = await postRepo.GetByIdAsync(post_id);
                    if (post.user_id == user.Id)
                    {
                        var Comment = await commentRepo.GetByIdAsync(comment_id);
                        if (Comment != null)
                        {
                            Comment.content = comment;

                            await commentRepo.Update(Comment);
                            return Ok(new
                            {
                                message = "Comment Updated Successfully!",
                                statusCode = 200
                            });
                        }
                        return NotFound(new ApiErrorResponde(404, "ThIs Comment is not Exist"));

                    }
                    return BadRequest(new ApiErrorResponde(500, "internal server error"));

                }

                return BadRequest(new ApiErrorResponde(400, "You are not Authorized"));


            }
            return BadRequest(new ApiErrorResponde(400, "You are not Authorized"));

        }



        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("create-like")]
        public async Task<ActionResult> CreateLike(int post_id)
        {

            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    var post = await postRepo.GetByIdAsync(post_id);
                    if (post.user_id == user.Id)
                    {
                        var likedto = new LikeDto
                        {
                            user_id = user.Id,
                            post_id = post.post_id,
                            creation_date = DateTime.Now,
                        };
                        var Like = mapper.Map<LikeDto, Likes>(likedto);
                        await likeRepo.Add(Like);
                        return Ok(new
                        {
                            message = "Like created Successfully!",
                            statusCode = 200
                        });

                    }
                    return BadRequest(new ApiErrorResponde(500, "internal server error"));

                }

                return BadRequest(new ApiErrorResponde(400, "You are not Authorized"));


            }
            return BadRequest(new ApiErrorResponde(400, "You are not Authorized"));

        }



        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("delete-like")]
        public async Task<ActionResult> DeleteLike(int post_id,int like_id)
        {

            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    var post = await postRepo.GetByIdAsync(post_id);
                    if (post.user_id == user.Id)
                    {
                        var like =await likeRepo.GetByIdAsync(like_id);
                        if(like != null)
                        {
                          await likeRepo.Delete(like);

                            return Ok(new
                            {
                                message = "Like Deleted Successfully!",
                                statusCode = 200
                            });
                        }
                        return NotFound(new ApiErrorResponde(404, "this Like is not Exist"));

                    }
                    return BadRequest(new ApiErrorResponde(500, "internal server error"));

                }

                return BadRequest(new ApiErrorResponde(400, "You are not Authorized"));


            }
            return BadRequest(new ApiErrorResponde(400, "You are not Authorized"));

        }



    }
}