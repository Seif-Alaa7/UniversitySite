using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Security.Claims;

using HelwanUniversity.ViewModels.ChatVMs;
using Data.Repository.IRepository;
using Models;

[ApiController]
[Route("api/[controller]")] 
public class ChatbotApiController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IStudentRepository _studentRepository;

    // هذا هو الـ URL الخاص بخدمة الشات بوت (في Python)
    // **مهم جداً:** تأكد من أن هذا الـ URL هو نفس العنوان الذي ستشغل عليه خدمة Python Chatbot
    // مثلاً: http://localhost:5001/chat إذا كان خادم Python يعمل على منفذ 5001 والـ Endpoint هو /chat
    private readonly string _aiChatbotApiUrl = "http://localhost:5001/chat";

    public ChatbotApiController(
        IHttpClientFactory httpClientFactory,
        IStudentRepository studentRepository)
    {
        _httpClientFactory = httpClientFactory;
        _studentRepository = studentRepository;
    }

    [HttpPost("SendMessage")]
    public async Task<IActionResult> SendMessage([FromBody] ChatMessageViewModel chatMessage)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User is not authenticated. Please log in to use the the chatbot.");
        }

        var student = _studentRepository.GetByUserId(userId);
        if (student == null)
        {
            return NotFound("Student profile not found for the authenticated user.");
        }
        var aiPayload = new
        {
            user_id = userId,
            student_id = student.Id,
            student_name = student.Name,
            student_department_id = student.DepartmentId,
            message = chatMessage.Message
        };

        var jsonPayload = JsonConvert.SerializeObject(aiPayload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var httpClient = _httpClientFactory.CreateClient();
        try
        {
            var aiResponse = await httpClient.PostAsync(_aiChatbotApiUrl, content);

            if (aiResponse.IsSuccessStatusCode)
            {
                var responseContent = await aiResponse.Content.ReadAsStringAsync();
                var chatResponse = JsonConvert.DeserializeObject<ChatResponseViewModel>(responseContent);
                return Ok(chatResponse);
            }
            else
            {
                var errorContent = await aiResponse.Content.ReadAsStringAsync();
                return StatusCode((int)aiResponse.StatusCode, $"AI Chatbot Error: {errorContent}");
            }
        }
        catch (HttpRequestException e)
        {
            return StatusCode(500, $"Error communicating with AI Chatbot: {e.Message}");
        }
    }
}