using System.ComponentModel.DataAnnotations;

namespace UrlShortener.DTOs
{
    public record ShortenUrlRequest(
        [Required(ErrorMessage = "URL is required.")]
        string Url
    );
}
