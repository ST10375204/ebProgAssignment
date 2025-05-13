using assignmentAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace assignmentAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DbController : Controller
    {
        private readonly AssignmentDbContext _context;

        public DbController(AssignmentDbContext context)
        {
            _context = context;
        }
        //  Products

        [HttpPost("UploadProduct")]
        public async Task<IActionResult> UploadProduct([FromForm] string model, IFormFile? image)
        {
            var dto = JsonConvert.DeserializeObject<itemModel>(model)!;
            var imgUrl = await SaveImage(image);
            dto.imgUrl = imgUrl ?? "";

            var entity = new Item
            {
                ItemName = dto.itemName,
                ItemCategory = dto.itemCategory,
                ItemDesc = dto.itemDesc,
                ItemPrice = dto.itemPrice,
                ProductionDate = dto.productionDate,
                ImgUrl = dto.imgUrl,
                UserId = dto.userID
            };

            _context.Items.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product uploaded successfully", productId = entity.ItemId });
        }

        [HttpGet("GetAllProducts")]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _context.Items
                .Select(i => new
                {
                    itemId = i.ItemId,
                    itemName = i.ItemName,
                    itemCategory = i.ItemCategory,
                    itemDesc = i.ItemDesc,
                    itemPrice = i.ItemPrice,
                    productionDate = i.ProductionDate,
                    imgUrl = i.ImgUrl,
                    userID = i.UserId
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("GetProductsByUser")]
        public async Task<IActionResult> GetProductsByUser(string userId)
        {
            var products = await _context.Items
                .Where(i => i.UserId == userId)
                .Select(i => new
                {
                    itemId = i.ItemId,
                    itemName = i.ItemName,
                    itemCategory = i.ItemCategory,
                    itemDesc = i.ItemDesc,
                    itemPrice = i.ItemPrice,
                    productionDate = i.ProductionDate,
                    imgUrl = i.ImgUrl,
                    userID = i.UserId
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("GetUserDetails")]
        public async Task<IActionResult> GetUserDetails(string userId)
        {
            var user = await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => new
                {
                    userId = u.UserId,
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    userRole = u.UserRole
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound($"User with ID {userId} not found.");

            return Ok(user);
        }

        [HttpPost("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct(int itemId,
                                                       string category,
                                                       string productionDate,
                                                       string name,
                                                       string desc,
                                                       double price)
        {
            var item = await _context.Items.FindAsync(itemId);
            if (item == null)
                return NotFound($"Product with ID {itemId} not found.");

            item.ItemCategory = category;
            item.ProductionDate = productionDate;
            item.ItemName = name;
            item.ItemDesc = desc;
            item.ItemPrice = price;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("DeleteProduct")]
        public async Task<IActionResult> DeleteProduct(int itemId)
        {
            var item = await _context.Items.FindAsync(itemId);
            if (item == null)
                return NotFound($"Product with ID {itemId} not found.");

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // Users

        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    userId = u.UserId,
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    userRole = u.UserRole
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("UpdateUser")]
        public async Task<IActionResult> UpdateUser(string userId, string lastName, string firstName)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound($"User with ID {userId} not found.");

            user.LastName = lastName;
            user.FirstName = firstName;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound($"User with ID {userId} not found.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("GetUserRole")]
        public async Task<IActionResult> GetUserRole(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("UserId cannot be null or empty.");

            var role = await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => u.UserRole)
                .FirstOrDefaultAsync();

            if (role == null)
                return NotFound($"User with ID {userId} not found.");

            return Ok(role);
        }

        //image helpers

        [HttpPost("TestImageUpload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> TestImageUpload(IFormFile image)
        {
            var imgUrl = await SaveImage(image);
            if (string.IsNullOrEmpty(imgUrl))
                return StatusCode(500, "Failed to upload image.");
            return Ok(new { imageUrl = imgUrl });
        }

       public async Task<string> SaveImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return string.Empty; // gracefully exit

            string imgUrl = string.Empty;

            try
            {
                string clientId = Environment.GetEnvironmentVariable("imgurClientID");
                string clientSecret = Environment.GetEnvironmentVariable("imgurClientSecret");

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                    throw new InvalidOperationException("Imgur Client ID or Client Secret is missing.");

                string accessToken = await GetAccessToken();

                if (string.IsNullOrEmpty(accessToken))
                    throw new InvalidOperationException("Failed to retrieve access token.");

                using (var ms = new MemoryStream())
                {
                    await image.CopyToAsync(ms);
                    byte[] imageBytes = ms.ToArray();

                    var client = new RestClient("https://api.imgur.com/3/image");
                    var request = new RestRequest();
                    request.Method = Method.Post;
                    request.AddHeader("Authorization", "Bearer " + accessToken);
                    request.AddParameter("image", Convert.ToBase64String(imageBytes));
                    request.AddParameter("title", "userImage");

                    RestResponse response = await client.ExecuteAsync(request);

                    if (response.IsSuccessful)
                    {
                        var jsonResponse = JObject.Parse(response.Content);
                        imgUrl = jsonResponse["data"]?["link"]?.ToString();
                    }
                    else
                    {
                        Console.WriteLine("Imgur Upload Failed: " + response.ErrorMessage);
                        throw new InvalidOperationException("Imgur upload failed: " + response.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error uploading image: " + ex.Message);

            }

            return imgUrl;
        }


        private async Task<string> GetAccessToken()
        {
            var client = new RestClient("https://api.imgur.com/oauth2/token");
            var request = new RestRequest();
            request.Method = Method.Post;

            request.AddParameter("refresh_token", Environment.GetEnvironmentVariable("imgurRefreshToken"));
            request.AddParameter("client_id", Environment.GetEnvironmentVariable("imgurClientID"));
            request.AddParameter("client_secret", Environment.GetEnvironmentVariable("imgurClientSecret"));
            request.AddParameter("grant_type", "refresh_token");

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful) return string.Empty;
            return JObject.Parse(response.Content!)["access_token"]!.ToString()!;
        }

    }
}
