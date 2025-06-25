using System.ComponentModel.DataAnnotations;

namespace HelwanUniversity.ViewModels.ChatVMs
{
    public class ChatMessageViewModel
    {
        [Required(ErrorMessage = "Message is required.")]
        public string Message { get; set; } = null!;
    }
}