using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Interfaces;

namespace UrlShortener.Controllers
{
    [ApiController]
    public class RedirectController : ControllerBase
    {
        private readonly IUrlService _urlService;

        public RedirectController(IUrlService urlService)
        {
            _urlService = urlService;
        }

        [HttpGet("/{shortCode:regex(^[[a-zA-Z0-9]]+$)}")]
        public async Task<IActionResult> RedirectToOriginal(string shortCode)
        {
            if (string.Equals(shortCode, "swagger", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(shortCode, "api", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            var originalUrl = await _urlService.GetOriginalUrlAsync(shortCode);

            if (string.IsNullOrEmpty(originalUrl))
            {
                return NotFound(new
                {
                    message = "Short URL not found."
                });
            }

            return Redirect(originalUrl);
        }
    }
}
