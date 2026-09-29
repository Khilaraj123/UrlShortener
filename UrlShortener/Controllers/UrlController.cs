using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.DTOs;
using UrlShortener.Interfaces;

namespace UrlShortener.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UrlController : ControllerBase
    {
        private readonly IUrlService _urlService;
        private readonly string _baseUrl;

        public UrlController(IUrlService urlService, IConfiguration configuration)
        {
            _urlService = urlService;
            _baseUrl = configuration["BaseUrl"] ?? "http://localhost:5000";
        }

        [HttpPost("api/urls")]
        public async Task<IActionResult> Shorten([FromBody] ShortenUrlRequest request)
        {
            if(!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
            {
                return BadRequest(new
                {
                    message = "Invalid URL format provided."
                });
            }

            var result = await _urlService.ShortenUrlAsync(request.Url);

            var response = new ShortenUrlResponse(
                ShortUrl: $"{_baseUrl}/{result.ShortCode}",
                ShortCode: result.ShortCode,
                OriginalUrl: result.OriginalUrl
            );

            return CreatedAtAction(nameof(RedirectToOriginal), new
            {
                shortCode = result.ShortCode
            }, response);
        }

        [HttpGet("{shortCode}")]
        public async Task<IActionResult> RedirectToOriginal(string shortCode)
        {
            var originalUrl = await _urlService.GetOriginalUrlAsync(shortCode);

            if (string.IsNullOrEmpty(originalUrl))
            {
                return NotFound(new
                {
                    message = "Short Url not found or expired"
                });
            }

            return Redirect(originalUrl);
        }
    }
}
