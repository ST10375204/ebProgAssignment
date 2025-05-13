using System.Text;
using FirebaseAdmin.Auth;
using assignment.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using assignment.Models;

namespace assignment.Controllers
{
    public class DbController : Controller
    {
        private static HttpClient httpClient = new()
        {
            BaseAddress = new Uri("http://localhost:5263/api/Db/"),
        };

        //products
        public async Task<IActionResult> ListProducts()
        {
            var response = await httpClient.GetAsync("GetAllProducts");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonString);
                TempData["ToastrMessage"] = "Fetched all products!";
                TempData["ToastrType"] = "success";

                return View(users);
            }

            TempData["ToastrMessage"] = "Unable to fetch products";
            TempData["ToastrType"] = "error";
            return View(new List<Dictionary<string, object>>());
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UploadProduct(itemModel item, IFormFile image)
        {
            using (var form = new MultipartFormDataContent())
            {
                var jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include
                };

                var jsonString = JsonConvert.SerializeObject(item, jsonSettings);
                var stringContent = new StringContent(jsonString, Encoding.UTF8);

                //explanation
                //when sending the json req, the null stuff is getting omitted
                //null value handling is suppsoed to deal with it but for some reason its not, so we 
                //now are converting it to a string, then to a plain text then to the req
                form.Add(stringContent, "model");

                if (image != null && image.Length > 0)
                {
                    var imageContent = new StreamContent(image.OpenReadStream());
                    imageContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
                    form.Add(imageContent, "image", image.FileName);
                }

                var response = await httpClient.PostAsync("UploadProduct", form);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    TempData["ToastrMessage"] = "Upload successful!";
                    TempData["ToastrType"] = "success";
                    return View("AddProduct");
                }
                else
                {
                    TempData["ToastrMessage"] = "Upload unsuccessful!";
                    TempData["ToastrType"] = "error";
                    return View("AddProduct");
                }
            }

        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(string itemId)
        {

            if (itemId == null)
            {
                TempData["ToastrMessage"] = "Product ID is null.";
                TempData["ToastrType"] = "error";
                return RedirectToAction("ListProducts", "Db");
            }
            var response = await httpClient.DeleteAsync($"DeleteProduct?itemId={itemId}");

            if (response.IsSuccessStatusCode)
            {
                TempData["ToastrMessage"] = "Product deleted successfully!";
                TempData["ToastrType"] = "success";
            }
            else
            {
                TempData["ToastrMessage"] = "Failed to delete Product.";
                TempData["ToastrType"] = "error";
            }
            return RedirectToAction("ListProducts");


        }

        [HttpGet]
        public async Task<IActionResult> UpdateProduct(int itemId)
        {

            var response = await httpClient.GetAsync($"GetProductForUpdate?itemId={itemId}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ToastrMessage"] = "Failed to load product.";
                TempData["ToastrType"] = "error";
                return RedirectToAction("ListProducts");
            }


            var json = await response.Content.ReadAsStringAsync();
            var itemModel = JsonConvert.DeserializeObject<itemModel>(json);
            return View(itemModel);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateProduct(itemModel item)
        {
            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include
            };

            var jsonString = JsonConvert.SerializeObject(item, jsonSettings);
            var stringContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"UpdateProduct", stringContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["ToastrMessage"] = "Product updated successfully!";
                TempData["ToastrType"] = "success";
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync();
                TempData["ToastrMessage"] = "Update failed";
                TempData["ToastrType"] = "error";
            }

            return RedirectToAction("UpdateProduct", new { itemId = item.itemId });
        }
        [HttpGet]
        public async Task<IActionResult> MyProducts()
        {
            var userId = HttpContext.Session.GetString("currentUser");
            var response = await httpClient.GetAsync(
                $"GetProductsByUser?userId={userId}"
            );

            if (!response.IsSuccessStatusCode)
            {
                TempData["ToastrMessage"] = "Products not found.";
                TempData["ToastrType"] = "error";
                return RedirectToAction("Index");
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var products = JsonConvert.DeserializeObject<List<itemModel>>(jsonString)
                            ?? new List<itemModel>();

            return View(products); 
        }

        //users

        public async Task<IActionResult> ListUsers()
        {
            var response = await httpClient.GetAsync("GetAllUsers");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonString);
                TempData["ToastrMessage"] = "Fetched all users!";
                TempData["ToastrType"] = "success";

                return View(users);
            }

            TempData["ToastrMessage"] = "Unable to fetch users";
            TempData["ToastrType"] = "error";
            return View(new List<Dictionary<string, object>>());
        }

        [HttpGet]
        public async Task<IActionResult> UserProfile(string userId)
        {
            var response = await httpClient.GetAsync($"GetUserDetails?userId={userId}");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var userData = JsonConvert.DeserializeObject<RegisterModel>(jsonString);
                return View(userData);
            }
            else
            {
                TempData["ToastrMessage"] = "User not found.";
                TempData["ToastrType"] = "error";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserProfile(RegisterModel model)
        {

            var response = await httpClient.PostAsync(
                $"UpdateUser?userId={HttpContext.Session.GetString("currentUser")}&lastName={model.lastName}&firstName={model.firstName}", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["ToastrMessage"] = "Updated profile!";
                TempData["ToastrType"] = "success";
                return RedirectToAction("UserProfile", new { userId = HttpContext.Session.GetString("currentUser") });
            }
            else
            {
                TempData["ToastrMessage"] = "Unable to update profile";
                TempData["ToastrType"] = "error";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var response = await httpClient.DeleteAsync($"DeleteUser?userId={userId}");

            if (response.IsSuccessStatusCode)
            {
                TempData["ToastrMessage"] = "User deleted successfully!";
                TempData["ToastrType"] = "success";
            }
            else
            {
                TempData["ToastrMessage"] = "Failed to delete user.";
                TempData["ToastrType"] = "error";
            }

            return RedirectToAction("ListUsers", "Db");
        }


    }
}