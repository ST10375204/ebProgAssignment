using Firebase.Auth;
using assignmentAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using assignmentAPI.Utils;
using Google.Cloud.Firestore;
using RestSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Google.Type;

namespace assignmentAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DbController : Controller
    {
        FirestoreDb db;
        string credentials;

        public DbController()
        {

            credentials = Environment.GetEnvironmentVariable("google_appli_creds");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentials);
            db = FirestoreDb.Create(Environment.GetEnvironmentVariable("firestoreID"));
        }



        [HttpPost("UploadProduct")]
        public async Task<IActionResult> UploadProduct([FromForm] itemModel model, IFormFile image)
        {

            var imgUrl = await SaveImage(image);


            model.imgUrl = imgUrl ?? "";
            //its nullable so even if a user doesnt select an image its all good

            Dictionary<string, object> item = new Dictionary<string, object>
        {
            { "itemName", model.itemName },
            { "itemCategory", model.itemCategory },
            { "itemDesc", model.itemDesc ?? "" },
            { "itemPrice", model.itemPrice ?? 0.0 },
            { "productionDate", model.productionDate ?? "" },
            { "imgUrl", model.imgUrl ?? "" },
            { "userID", model.userID ?? "" }
        };

            // Add to Firestore
            CollectionReference colRef = db.Collection("Products");
            DocumentReference docRef = await colRef.AddAsync(item);

            // Return success response with product ID
            return Ok(new { message = "Product uploaded successfully", productId = docRef.Id });

        }



        [HttpPost("TestImageUpload")] //test for the helper is dummy
        public async Task<IActionResult> TestImageUpload(IFormFile image)
        {
            var imgUrl = await SaveImage(image);

            if (string.IsNullOrEmpty(imgUrl))
                return StatusCode(500, "Failed to upload image.");

            return Ok(new { imageUrl = imgUrl });
        }

        public async Task<string> SaveImage(IFormFile image)
        {
            string imgUrl = string.Empty;

            try
            {

                if (image == null || image.Length == 0)
                    throw new ArgumentException("Image file is empty.");

                string clientId = Environment.GetEnvironmentVariable("imgurClientID");
                string clientSecret = Environment.GetEnvironmentVariable("imgurClientSecret");

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    throw new InvalidOperationException("Imgur Client ID or Client Secret is missing.");
                }


                string accessToken = await GetAccessToken();


                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new InvalidOperationException("Failed to retrieve access token.");
                }


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
                Console.WriteLine("Error uploading image: " + ex.Message); //will trigger if user doenst select an image but no big deal since its console
            }
            Console.WriteLine(imgUrl);
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

            RestResponse response = await client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                var jsonResponse = JObject.Parse(response.Content);
                Console.WriteLine("New Access Token: " + jsonResponse["access_token"]?.ToString());
                return jsonResponse["access_token"]?.ToString();
            }
            else
            {
                Console.WriteLine("Error getting access token: " + response.Content);
                return string.Empty;
            }
        }

        [HttpGet("GetAllProducts")]
        public async Task<IActionResult> GetAllProducts()
        {
            var productsRef = db.Collection("Products");
            var snapshot = await productsRef.GetSnapshotAsync();
            var products = snapshot.Documents.Select(doc => doc.ToDictionary());
            return Ok(products);
        }

        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var usersRef = db.Collection("Users");
            var snapshot = await usersRef.GetSnapshotAsync();
            var users = snapshot.Documents.Select(doc => doc.ToDictionary());

            return Ok(users);

        }

        [HttpGet("GetProductsByUser")]
        public async Task<IActionResult> GetProductsByUser(string userId)
        {
            var productsRef = db.Collection("Products");
            var query = productsRef.WhereEqualTo("userID", userId);
            var snapshot = await query.GetSnapshotAsync();
            var products = snapshot.Documents.Select(doc => doc.ToDictionary());
            return Ok(products);
        }

        [HttpGet("GetUserDetails")]
        public async Task<IActionResult> GetUserDetails(string userId)
        {
            var docRef = db.Collection("Users").Document(userId);
            var snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                var userData = snapshot.ToDictionary();
                userData.Add("userId", snapshot.Id);
                return Ok(userData);
            }
            else
            {
                return NotFound($"User with ID {userId} not found.");
            }
        }

        [HttpPost("UpdateUser")]
        public async Task<IActionResult> UpdateUser(string userId, string lastName, string firstName)
        {
            DocumentReference userRef = db.Collection("Users").Document(userId);

            await userRef.SetAsync(new Dictionary<string, object>
            {
                 { "firstName", firstName },
                 {"lastName", lastName}
            }, SetOptions.MergeAll);
            return Ok();
        }

        [HttpPost("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct(string itemId, string category, string productionDate, string name, string desc, double price)
        {
            DocumentReference itemRef = db.Collection("Products").Document(itemId);

            await itemRef.SetAsync(new Dictionary<string, object>
            {
                 { "itemName", name },
                 {"itemDesc", desc},
                 {"itemCategory", category},
                 {"productionDate", productionDate},
                 {"itemPrice", price}
            }, SetOptions.MergeAll);
            return Ok();
        }

        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            DocumentReference userRef = db.Collection("Users").Document(userId);
            await userRef.DeleteAsync();
            return Ok();
        }

        [HttpDelete("DeleteProduct")]
        public async Task<IActionResult> DeleteProduct(string itemId)
        {
            DocumentReference itemRef = db.Collection("Products").Document(itemId);
            await itemRef.DeleteAsync();
            return Ok();
        }



    }
}



