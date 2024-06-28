using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlantifyApp.Apis.Dtos;
using PlantifyApp.Core.Models;
using System.Security.Claims;
using PlantifyApp.Apis.Errors;
using PlantifyApp.Services;
using PlantifyApp.Core.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;

namespace PlantifyApp.Apis.Controllers
{

    public class DashboardController : ApiBaseController
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ITokenService tokenService;
        private readonly IMapper mapper;
        private readonly IGenericRepository<ApplicationUser> userRepo;
        private readonly IGenericRepository<Comments> commentRepo;
        private readonly IGenericRepository<Likes> likeRepo;
        private readonly IGenericRepository<Posts> postRepo;
        private readonly IGenericRepository<Contactus> contactusRepo;

        public DashboardController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, ITokenService tokenService, IMapper mapper, IGenericRepository<ApplicationUser> userRepo, IGenericRepository<Comments> commentRepo, IGenericRepository<Likes> likeRepo, IGenericRepository<Posts> postRepo, IGenericRepository<Contactus> contactusRepo)
        {
            UserManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
            this.tokenService = tokenService;
            this.mapper = mapper;
            this.userRepo = userRepo;
            this.commentRepo = commentRepo;
            this.likeRepo = likeRepo;
            this.postRepo = postRepo;
            this.contactusRepo = contactusRepo;
        }

        public UserManager<ApplicationUser> UserManager { get; }

        [HttpPost("admin")]
        public async Task<ActionResult> Login(string Email, string Password)
        {
            var user = await UserManager.FindByEmailAsync(Email);
            if (user is null) return Unauthorized(new ApiErrorResponde(401));
            if (UserManager.IsInRoleAsync(user, "Admin").Result)
            {
                var result = await signInManager.CheckPasswordSignInAsync(user, Password, false);
                if (!result.Succeeded) { return Unauthorized(new ApiErrorResponde(401, "Wrong Access Credientials")); }
                var request = HttpContext.Request;
                var requestUrl = $"{request.Scheme}://{request.Host}/Assest/User_images";
                string image = null;
                if (!string.IsNullOrEmpty(user.Image_name))
                    image = $"{requestUrl}/{user.Image_name}";
                return Ok(new
                {
                    Id = user.Id,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    Image_name = image,
                    Token = await tokenService.CreateToken(user, UserManager)
                });
            }
            return Unauthorized(new ApiErrorResponde(401, "You are not authorized"));
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("get-all-user-list")]
        public async Task<ActionResult> GetAllUsers(int pageNumber = 1)
        {
            int pageSize = 10;
            var users = await userRepo.GetAllAsync();
            if (users != null)
            {
                var userdto = mapper.Map<IReadOnlyList<ApplicationUser>, IReadOnlyList<UserDto>>(users);
                foreach (var user in userdto)
                {
                    if (user.Image_name != null)
                    {
                        var request = HttpContext.Request;
                        var requestUrl = $"{request.Scheme}://{request.Host}/Assest/User_images";
                        string image = null;
                        if (!string.IsNullOrEmpty(user.Image_name))
                            image = $"{requestUrl}/{user.Image_name}";
                        user.Image_name = image;
                    }
                }
                // Filter out non-admin users
                var Users = userdto.Where(u => u.Role!= "Admin").ToList();

                // Apply pagination
                var pagedUsers = Users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                return Ok(pagedUsers);
            }
            return NotFound(new ApiErrorResponde(404, "There is no user"));

        }



        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("get-all-admin-list")]
        public async Task<ActionResult> GetAllAdmin(int pageNumber = 1)
        {
            int pageSize = 10;
            var users = await userRepo.GetAllAsync();
            if (users != null)
            {
                var userdto = mapper.Map<IReadOnlyList<ApplicationUser>, IReadOnlyList<UserDto>>(users);
                foreach (var user in userdto)
                {
                    if (user.Image_name != null)
                    {
                        var request = HttpContext.Request;
                        var requestUrl = $"{request.Scheme}://{request.Host}/Assest/User_images";
                        string image = null;
                        if (!string.IsNullOrEmpty(user.Image_name))
                            image = $"{requestUrl}/{user.Image_name}";
                        user.Image_name = image;
                    }
                }
                // Filter out non-admin users
                var Users = userdto.Where(u => u.Role == "Admin").ToList();

                // Apply pagination
                var pagedUsers = Users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                return Ok(pagedUsers);
            }
            return NotFound(new ApiErrorResponde(404, "There is no user"));

        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("add-new-user")]
        public async Task<ActionResult> AddNewUser(string name, string email, string password, string role, IFormFile? image)
        {
            // Validate role
            if (!await roleManager.RoleExistsAsync(role) || role == "Admin")
            {
                return BadRequest(new ApiErrorResponde(400, "Invalid role."));
            }

            // Check if user already exists
            var existingUser = await UserManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                return BadRequest(new ApiErrorResponde(400, "User with this email already exists."));
            }
            var existingUsername = await UserManager.FindByNameAsync(name);
            if (existingUsername != null)
            {
                return BadRequest(new ApiErrorResponde(400, "User with this name already exists."));
            }

            // Create new user
            var user = new ApplicationUser
            {
                Email = email,
                DisplayName = name,
                Role = role,
                UserName = email.Split('@')[0],
                created_date=DateTime.Now

            };

            var result = await UserManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiErrorResponde(400, "Failed to create user."));
            }

            // Assign role to user
            result = await UserManager.AddToRoleAsync(user, role);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiErrorResponde(400, "Failed to assign role to user."));
            }

            // Handle image upload
            if (image != null && image.Length > 0)
            {
                try
                {
                    string guid = Guid.NewGuid().ToString();
                    string fileExtension = Path.GetExtension(image.FileName);
                    string newFileName = $"{guid}_{Path.GetFileNameWithoutExtension(image.FileName)}{fileExtension}";

                    string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "Assest");
                    string imagePath = Path.Combine(rootPath, "User_images", newFileName);

                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    user.Image_name = newFileName;
                    await UserManager.UpdateAsync(user);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"An error occurred while uploading the image: {ex.Message}");
                }
            }

            return Ok(new
            {
                message = "User created successfully!",
                statusCode = 200
            });
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("update-user")]
        public async Task<ActionResult> UpdateUser(string id, string? name, string? email, string? role, IFormFile? image)
        {
            var user = await UserManager.FindByIdAsync(id);
            if (user == null )
            {
                return NotFound(new ApiErrorResponde(404, "User not found."));
            }
            if(user.Role == "Admin")
            {
                return NotFound(new ApiErrorResponde(404, "User not found."));
            }
            // Check if user already exists
            if (!string.IsNullOrEmpty(email))
            {
                var existingUser = await UserManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    return BadRequest(new ApiErrorResponde(400, "User with this email already exists."));
                }
            }
           if(!string.IsNullOrEmpty(name))
            {
                var existingUsername = await UserManager.FindByNameAsync(name);
                if (existingUsername != null)
                {
                    return BadRequest(new ApiErrorResponde(400, "User with this name already exists."));
                }
            }
          

            if (!await roleManager.RoleExistsAsync(role)|| role=="Admin")
            {
                return BadRequest(new ApiErrorResponde(400, "Invalid role."));
            }
            if (!string.IsNullOrEmpty(name))
            {
                user.DisplayName = name;
            }
            if (!string.IsNullOrEmpty(email))
            {
                user.Email = email;
                user.UserName = email.Split('@')[0];

            }
            var currentRoles = await UserManager.GetRolesAsync(user);
            if(currentRoles!=null && currentRoles.FirstOrDefault() != user.Role)
            {
                currentRoles = new List<string> { user.Role };
            }
            if (!string.IsNullOrEmpty(role))
            {
                user.Role = role;
            }
          var resu=await UserManager.AddToRoleAsync(user, role);

            if (!resu.Succeeded)
            {
                return BadRequest(new ApiErrorResponde(400, "Failed to update user Role."));
            }
            var result = await UserManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiErrorResponde(400, "Failed to update user."));
            }

            var removeRolesResult = await UserManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeRolesResult.Succeeded)
            {
                return BadRequest(new ApiErrorResponde(400, "Failed to remove existing roles."));
            }

            var addRoleResult = await UserManager.AddToRoleAsync(user, role);
            if (!addRoleResult.Succeeded)
            {
                return BadRequest(new ApiErrorResponde(400, "Failed to assign new role."));
            }

            if (image != null && image.Length > 0)
            {
                try
                {
                   
                    string guid = Guid.NewGuid().ToString();
                    string fileExtension = Path.GetExtension(image.FileName);
                    string newFileName = $"{guid}_{Path.GetFileNameWithoutExtension(image.FileName)}{fileExtension}";

                    string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "Assest");
                    string imagePath = Path.Combine(rootPath, "User_images", newFileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    try
                    {
                        if (user.Image_name != null)
                        {
                            string oldimagpath = Path.Combine(rootPath, "User_images", user.Image_name);

                            System.IO.File.Delete(oldimagpath);
                        }
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new ApiErrorResponde(500, "internal server error"));
                    }

                    user.Image_name = newFileName;
                   var res= await UserManager.UpdateAsync(user);
                    if (!res.Succeeded)
                    {
                        return BadRequest(new ApiErrorResponde(400, "Failed to updating User."));
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"An error occurred while uploading the image: {ex.Message}");
                }
            }

            return Ok(new { message = "User updated successfully!", statusCode = 200 });
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("delete-user")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            var user = await UserManager.FindByIdAsync(id);
            if (user == null  )
            {
                return NotFound(new ApiErrorResponde(404, "User not found."));
            }
            if(user.Role == "Admin")
            {
                return NotFound(new ApiErrorResponde(404, "User not found."));
            }

                 
            await postRepo.DeleteByUserIdAsync(id);
            await commentRepo.DeleteByUserIdAsync(id);
            await likeRepo.DeleteByUserIdAsync(id);
            var result = await UserManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiErrorResponde(400, "Failed to delete user."));
            }

            return Ok(new { message = "User deleted successfully!", statusCode = 200 });
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("create-admin")]
        public async Task<ActionResult> CreateAdmin(string name, string email, string password, IFormFile? image)
        {
            var existingUser = await UserManager.FindByEmailAsync(email);
            if (existingUser != null )
            {
                return BadRequest(new ApiErrorResponde(400, "Admin with this email already exists."));
            }

            var user = new ApplicationUser
            {
                UserName = email.Split('@')[0],
                Email = email,
                DisplayName = name,
                Role = "Admin",
                created_date=DateTime.Now
            };

            var result = await UserManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiErrorResponde(400, "Failed to create admin user."));
            }

            result = await UserManager.AddToRoleAsync(user, "Admin");
            if (!result.Succeeded)
            {
                return BadRequest(new ApiErrorResponde(400, "Failed to assign admin role to user."));
            }

            if (image != null && image.Length > 0)
            {
                try
                {
                    string guid = Guid.NewGuid().ToString();
                    string fileExtension = Path.GetExtension(image.FileName);
                    string newFileName = $"{guid}_{Path.GetFileNameWithoutExtension(image.FileName)}{fileExtension}";

                    string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "Assest");
                    string imagePath = Path.Combine(rootPath, "User_images", newFileName);

                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    user.Image_name = newFileName;
                    await UserManager.UpdateAsync(user);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"An error occurred while uploading the image: {ex.Message}");
                }
            }

            return Ok(new { message = "Admin created successfully!", statusCode = 200 });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("update-admin")]
        public async Task<ActionResult> UpdateAdmin(string id, string? name, string? email, IFormFile? image)
        {
            var user = await UserManager.FindByIdAsync(id);
            if (user == null )
            {
                return NotFound(new ApiErrorResponde(404, "Admin not found."));
            }
            if (user.Role != "Admin")
            {
                return NotFound(new ApiErrorResponde(404, "Admin not found."));

            }
            // Check if user already exists
            if (!string.IsNullOrEmpty(email))
            {
                var existingUser = await UserManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    return BadRequest(new ApiErrorResponde(400, "Admin with this email already exists."));
                }
            }
            if (!string.IsNullOrEmpty(name))
            {
                var existingUsername = await UserManager.FindByNameAsync(name);
                if (existingUsername != null)
                {
                    return BadRequest(new ApiErrorResponde(400, "Admin with this name already exists."));
                }
            }

            if (!string.IsNullOrEmpty(name))
            {
                user.DisplayName = name;
            }
            if (!string.IsNullOrEmpty(email))
            {
                user.Email = email;
                user.UserName = email.Split('@')[0];

            }

            var result = await UserManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiErrorResponde(400, "Failed to update user."));
            }

        

            if (image != null && image.Length > 0)
            {
                try
                {

                    string guid = Guid.NewGuid().ToString();
                    string fileExtension = Path.GetExtension(image.FileName);
                    string newFileName = $"{guid}_{Path.GetFileNameWithoutExtension(image.FileName)}{fileExtension}";

                    string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "Assest");
                    string imagePath = Path.Combine(rootPath, "User_images", newFileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    try
                    {
                        if (user.Image_name != null)
                        {
                            string oldimagpath = Path.Combine(rootPath, "User_images", user.Image_name);

                            System.IO.File.Delete(oldimagpath);
                        }
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new ApiErrorResponde(500, "internal server error"));
                    }

                    user.Image_name = newFileName;
                    var res = await UserManager.UpdateAsync(user);
                    if (!res.Succeeded)
                    {
                        return BadRequest(new ApiErrorResponde(400, "Failed to updating Admin."));
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"An error occurred while uploading the image: {ex.Message}");
                }
            }

            return Ok(new { message = "Admin updated successfully!", statusCode = 200 });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("delete-admin")]
        public async Task<ActionResult> DeleteAdmin(string id)
        {
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ApiErrorResponde(404, "Admin not found."));
            }
            if(!await UserManager.IsInRoleAsync(user, "Admin"))
            {
                return NotFound(new ApiErrorResponde(404, "Admin not found."));
            }
            await postRepo.DeleteByUserIdAsync(id);
            await commentRepo.DeleteByUserIdAsync(id);
            await likeRepo.DeleteByUserIdAsync(id);
            var result = await UserManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiErrorResponde(400, "Failed to delete admin."));
            }

            return Ok(new { message = "Admin deleted successfully!", statusCode = 200 });
        }



        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("get-user-details")]
        public async Task<ActionResult> GetUserDetails(string id)
        {
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {

                return NotFound(new ApiErrorResponde(404, "User not found."));
            }
            if (user.Role == "Admin")
            {
                return NotFound(new ApiErrorResponde(404, "User not found."));
            }

            var userDto = mapper.Map<ApplicationUser, UserDto>(user);
            if (user.Image_name != null)
            {
                var request = HttpContext.Request;
                var requestUrl = $"{request.Scheme}://{request.Host}/Assest/User_images";
                string image = $"{requestUrl}/{user.Image_name}";
                userDto.Image_name = image;
            }

            return Ok(userDto);
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("get-admin-details")]
        public async Task<ActionResult> GetAdminDetails(string id)
        {
            var user = await UserManager.FindByIdAsync(id);
            if (user == null )
            {
                return NotFound(new ApiErrorResponde(404, "Admin not found."));
            }
            if(!await UserManager.IsInRoleAsync(user, "Admin"))
            {
                return NotFound(new ApiErrorResponde(404, "Admin not found."));
            }

            var userDto = mapper.Map<ApplicationUser, UserDto>(user);
            if (user.Image_name != null)
            {
                var request = HttpContext.Request;
                var requestUrl = $"{request.Scheme}://{request.Host}/Assest/User_images";
                string image = $"{requestUrl}/{user.Image_name}";
                userDto.Image_name = image;
            }

            return Ok(userDto);
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("get-user-counts-by-role")]
        public async Task<ActionResult> GetUserCountsByRole()
        {
            var roles = new[] { "User", "Agricultural engineer", "Botanist", "Expert","Admin" };

            var userCountsByRole = new Dictionary<string, int>();

            foreach (var role in roles)
            {
                var usersInRole = await UserManager.GetUsersInRoleAsync(role);
                userCountsByRole[role] = usersInRole.Count;
            }

            return Ok(userCountsByRole);
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("user-counts-over-time")]
        public async Task<ActionResult> GetUserCountsOverTime(string period = "monthly")
        {
            try
            {
                var users = await userRepo.GetAllAsync(); // Assuming this method retrieves all users from the repository

                var userCountsOverTime = new Dictionary<string, int>();

                // Example logic: count users based on creation date for different time periods
                switch (period.ToLower())
                {
                    case "daily":
                        userCountsOverTime = CountUsersByPeriod(users, user => user.created_date?.Date.ToShortDateString());
                        break;
                    case "monthly":
                        userCountsOverTime = CountUsersByPeriod(users, user => user.created_date?.ToString("MM-yyyy"));
                        break;
                    case "yearly":
                        userCountsOverTime = CountUsersByPeriod(users, user => user.created_date?.Year.ToString());
                        break;
                    default:
                        return BadRequest(new ApiErrorResponde(400, "Invalid period. Supported periods: daily, monthly, yearly"));
                }

                return Ok(userCountsOverTime);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to retrieve user counts over time: {ex.Message}");
            }
        }

        private Dictionary<string, int> CountUsersByPeriod(IEnumerable<ApplicationUser> users, Func<ApplicationUser, string> periodSelector)
        {
            var userCounts = new Dictionary<string, int>();

            foreach (var user in users)
            {
                var period = periodSelector(user);

                if (userCounts.ContainsKey(period))
                {
                    userCounts[period]++;
                }
                else
                {
                    userCounts[period] = 1;
                }
            }

            return userCounts;
        }



        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("contactus/all")]
        public async Task<ActionResult<IEnumerable<ContactusDto>>> GetAllContactUsMessages()
        {
            var messages = await contactusRepo.GetAllAsync();
            var messageDtos = mapper.Map<IEnumerable<Contactus>, IEnumerable<ContactusDto>>(messages);
            return Ok(messageDtos);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("contactus/update")]
        public async Task<ActionResult> UpdateContactUsMessage(int id ,int is_replied)
        {
            var message = await contactusRepo.GetByIdAsync(id);
            if (message == null)
            {
                return NotFound(new ApiErrorResponde(404, "Message not found."));
            }
            if(is_replied==1)
            message.is_replied = true;
            else
             message.is_replied = false;

            await contactusRepo.Update(message);



            return Ok(new
            {
                message = "Message status updated successfully.",
                statusCode = 200
            });
        }



        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("signout")]
        public async Task<ActionResult> SignOut()
        {
            await signInManager.SignOutAsync();
            return Ok(new { message = "Signed out successfully!", statusCode = 200 });
        }

    }
}
