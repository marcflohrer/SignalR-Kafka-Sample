using Microsoft.AspNetCore.Mvc;

namespace StockProcessor.Controllers {
    [Route ("[controller]")]
    public class HomeController : Controller {
        [HttpGet]
        public IActionResult Error (int? statusCode = null) {
            if (statusCode.HasValue) {
                if (statusCode == 404 || statusCode == 500) {
                    var viewName = statusCode.ToString ();
                    return View (viewName);
                }
            }
            return View ();
        }
    }

}