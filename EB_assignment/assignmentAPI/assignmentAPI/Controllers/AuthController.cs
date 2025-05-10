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

namespace assignmentAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {

        FirebaseAuthProvider auth;
        FirestoreDb db;
        string credentials;
        public AuthController()
        {
            auth = new FirebaseAuthProvider(new FirebaseConfig(Environment.GetEnvironmentVariable("FirebaseMathApp")));
            credentials = Environment.GetEnvironmentVariable("google_appli_creds");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentials);
            db = FirestoreDb.Create(Environment.GetEnvironmentVariable("firestoreID"));
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterModel regi)
        {
            try
            {
                await auth.CreateUserWithEmailAndPasswordAsync(regi.email, regi.password);

                var fbAuthLink = await auth.SignInWithEmailAndPasswordAsync(regi.email, regi.password);
                string currentUserId = fbAuthLink.User.LocalId;

                if (currentUserId != null)
                {
                    await SaveUserData(currentUserId, regi.firstName, regi.lastName, regi.userRole);

                    return Ok(new AuthResponse(currentUserId));
                }
            }
            catch (FirebaseAuthException ex)
            {
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseErrorModel>(ex.ResponseData);
                return Unauthorized(firebaseEx.error.code + " - " + firebaseEx.error.message);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
            return View();

        }

        private async Task SaveUserData(string uid, string firstName, string lastName, string userRole)
        {
            DocumentReference docRef = db.Collection("Users").Document(uid);
            Dictionary<string, object> user = new Dictionary<string, object>
    {
        { "firstName", firstName },
        { "lastName", lastName },
        { "userRole", userRole }
    };
       await docRef.SetAsync(user);
        
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
