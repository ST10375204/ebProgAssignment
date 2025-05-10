using System.Text;
using FirebaseAdmin.Auth;
using assignment.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace assignment.Controllers
{
    public class AuthController : Controller
    {
        
        private static HttpClient httpClient = new()
        {
            BaseAddress = new Uri("http://localhost:5299/"),
        };

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(LoginModel login)
        {
            StringContent jsonContent = new(JsonConvert.SerializeObject(login), Encoding.UTF8,"application/json"); 
            HttpResponseMessage response = await httpClient.PostAsync("api/Auth/Register", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                AuthResponse? deserialisedResponse = JsonConvert.DeserializeObject<AuthResponse>(jsonResponse);
                
                HttpContext.Session.SetString("currentUser", deserialisedResponse.Token);
                return View("Login");                
            } else
            {
                ViewBag.Result = "An error has occurred";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel login)
        {
            StringContent jsonContent = new(JsonConvert.SerializeObject(login), Encoding.UTF8,"application/json"); 
            HttpResponseMessage response = await httpClient.PostAsync("api/Auth/Login", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                AuthResponse? deserialisedResponse = JsonConvert.DeserializeObject<AuthResponse>(jsonResponse);
                
                HttpContext.Session.SetString("currentUser", deserialisedResponse.Token);
                return View("Home");                
            } else
            {
                ViewBag.Result = "An error has occurred";
                return View();
            }            
        }

        [HttpGet]
        public IActionResult LogOut()
        {
            HttpContext.Session.Remove("currentUser");
            return RedirectToAction("Login");
        }
        

    }
}