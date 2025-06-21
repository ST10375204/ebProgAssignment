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
using assignmentAPI.Controllers;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace assignmentAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {

        FirebaseAuthProvider auth;
        private readonly IConfiguration _config;
      
        private readonly AssignmentDbContext _context;
        public AuthController(IConfiguration config, AssignmentDbContext context)
        {
            _config = config;
            _context = context;
          auth = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyDchoNrZSGuyLr_TmXimP6nDPZ2Dg0Zx7c"));
        }


        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterModel regi)
        {

            try
            {
                string currentUserId = await CreateFirebaseUserAndGetUid(regi.email, regi.password);

                if (currentUserId != null)
                {
                    await SaveUserData(currentUserId, regi);

                    return Ok(new AuthResponse(currentUserId));
                }
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }

            return View();
        }

        private async Task SaveUserData(string uid, RegisterModel regi)
        {
            var newUser = new Models.User
            {
                UserId = uid,
                FirstName = regi.firstName,
                LastName = regi.lastName,
                UserRole = regi.userRole
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
        }


        public async Task<string> CreateFirebaseUserAndGetUid(string email, string password)
        {
            var client = new RestClient("https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=" + Environment.GetEnvironmentVariable("FirebaseMathApp"));

            var request = new RestRequest("", Method.Post);
            request.AddHeader("Content-Type", "application/json");

            var body = new
            {
                email = email,
                password = password,
                returnSecureToken = true
            };

            request.AddJsonBody(body);

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                throw new Exception("Failed to create user: " + response.Content);
            }

            // Log the raw response content for debugging purposes
            Console.WriteLine(response.Content);  // Or use your logging mechanism

            dynamic result = JObject.Parse(response.Content);
            string uid = result.localId;

            return uid;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginModel login)
        {
            try
            {
                var fbAuthLink = await auth.SignInWithEmailAndPasswordAsync(login.Email, login.Password);
                string currentUserId = fbAuthLink.User.LocalId;
                
                if (currentUserId != null)
                {
                    return Ok(new AuthResponse(currentUserId));
                }

            }
            catch (FirebaseAuthException ex)
            {
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseErrorModel>(ex.ResponseData);
                AuthLogger.Instance.LogError(firebaseEx.error.message + " - User: " + login.Email + " - IP: " + HttpContext.Connection.RemoteIpAddress
                    + " - Browser: " + Request.Headers.UserAgent);
                return Unauthorized(firebaseEx.error.code + " - " + firebaseEx.error.message);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }

            return View();
        }


        [HttpPost("Logout")]
        public IActionResult LogOut()
        {
            return Ok();
        }



    }
}
