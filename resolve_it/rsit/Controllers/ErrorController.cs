using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace rsit.Controllers
{
    public class ErrorController : Controller
    {
        [Route("/Error")]
        [AllowAnonymous]
        public IActionResult Error(int statusCode)
        {
            string title;
            string message;

            switch (statusCode)
            {
                case 400:
                    title = "Bad Request";
                    message = "The request could not be understood by the server.";
                    break;

                case 401:
                    title = "Unauthorized";
                    message = "You need to be logged in to access this resource.";
                    break;

                case 403:
                    title = "Access Denied";
                    message = "You do not have permission to access this page.";
                    break;

                case 404:
                    title = "Page Not Found";
                    message = "Sorry, the page you are looking for could not be found.";
                    break;

                case 405:
                    title = "Method Not Allowed";
                    message = "The requested HTTP method is not allowed for this resource.";
                    break;

                case 500:
                    title = "Internal Server Error";
                    message = "Something went wrong on our server. Please try again later.";
                    break;

                case 502:
                    title = "Bad Gateway";
                    message = "The server received an invalid response from another server.";
                    break;

                case 503:
                    title = "Service Unavailable";
                    message = "The service is temporarily unavailable. Please try again later.";
                    break;

                default:
                    title = "Something Went Wrong";
                    message = "An unexpected error occurred while processing your request.";
                    break;
            }

            ViewBag.StatusCode = statusCode;
            ViewBag.Title = title;
            ViewBag.Message = message;

            return View();
        }
    }
}