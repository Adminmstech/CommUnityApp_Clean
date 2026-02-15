using CommUnityApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Net.Http;

namespace CommUnityApp.Areas.Community.Controllers
{
    [Area("Community")]

    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string userName, string password)
        {
           using var con = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                using var cmd = new SqlCommand("sp_CommunityLogin", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserName", userName);
                cmd.Parameters.AddWithValue("@Password", password);

                con.Open();

                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    long resultId = Convert.ToInt64(reader["ResultId"]);

                    if (resultId > 0)
                    {
                        HttpContext.Session.SetString(
                            "CommunityId",
                            reader["CommunityId"].ToString());

                        HttpContext.Session.SetString(
                            "CommunityName",
                            reader["CommunityName"].ToString());

                    return RedirectToAction(
                 "AddEvent",
                 "Home",
                 new { area = "Community" });
                }

             

                ViewBag.Error = reader["ResultMessage"].ToString();
                    return View();
                }

                ViewBag.Error = "Invalid username or password";
                return View();
            }
        public IActionResult CommunityLogin()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CommunityLogin(string userName, string password)
        {
            using var con = new SqlConnection(
                     _configuration.GetConnectionString("DefaultConnection"));

            using var cmd = new SqlCommand("sp_CommunityLogin", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UserName", userName);
            cmd.Parameters.AddWithValue("@Password", password);

            con.Open();

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                long resultId = Convert.ToInt64(reader["ResultId"]);

                if (resultId > 0)
                {
                    HttpContext.Session.SetString(
                        "CommunityId",
                        reader["CommunityId"].ToString());

                    HttpContext.Session.SetString(
                        "CommunityName",
                        reader["CommunityName"].ToString());

                    return RedirectToAction(
                 "AddEvent",
                 "Home",
                 new { area = "Community" });
                }



                ViewBag.Error = reader["ResultMessage"].ToString();
                return View();
            }

            ViewBag.Error = "Invalid username or password";
            return View();
        }
    }


    }

