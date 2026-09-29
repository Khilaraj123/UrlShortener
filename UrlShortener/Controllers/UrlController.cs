using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.DTOs;
using UrlShortener.Interfaces;

namespace UrlShortener.Controllers
{
    [Route("api/urls")]
    [ApiController]
    public class UrlController : ControllerBase
    {
        private readonly IUrlService _urlService;
        private readonly string _baseUrl;

        public UrlController(IUrlService urlService, IConfiguration configuration)
        {
            _urlService = urlService;
            _baseUrl = (configuration["BaseUrl"] ?? "http://localhost:5062").TrimEnd('/');
        }

        [HttpPost]
        public async Task<IActionResult> Shorten([FromBody] ShortenUrlRequest? request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Url))
            {
                return BadRequest(new
                {
                    message = "URL cannot be empty."
                });
            }

            if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return BadRequest(new
                {
                    message = "Invalid URL format. Only HTTP and HTTPS URLs are allowed."
                });
            }

            // Prevent self-referencing / recursion loops
            if (Uri.TryCreate(_baseUrl, UriKind.Absolute, out var baseUri) &&
                string.Equals(uri.Host, baseUri.Host, StringComparison.OrdinalIgnoreCase) &&
                uri.Port == baseUri.Port)
            {
                return BadRequest(new
                {
                    message = "Cannot shorten URLs from this service."
                });
            }

            var result = await _urlService.ShortenUrlAsync(request.Url);

            var response = new ShortenUrlResponse(
                ShortUrl: $"{_baseUrl}/{result.ShortCode}",
                ShortCode: result.ShortCode,
                OriginalUrl: result.OriginalUrl
            );

            return Created(
                response.ShortUrl,
                response);
        }
    }
}
