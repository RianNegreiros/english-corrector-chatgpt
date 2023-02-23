using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using dotnet_core_chatgpt.Models;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_core_chatgpt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnglishCorrectorController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public EnglishCorrectorController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string text, [FromServices] IConfiguration configuration)
        {
            var sk = configuration.GetValue<string>("ChatGptSecretKey");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sk);

            if (string.IsNullOrEmpty(text))
            {
                return BadRequest("Text is null or empty");
            }

            var model = new ChatGptInputModel(text);

            var requestBody = JsonSerializer.Serialize(model);

            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("OpenAI API returned error");
            }

            var result = await response.Content.ReadFromJsonAsync<ChatGptViewModel>();

            if (result == null)
            {
                return BadRequest("OpenAI API returned null");
            }

            var prompt = result.choices.First();

            return Ok(prompt.text.Replace("\n", "").Replace("\t", ""));
        }
    }
}