using System.ComponentModel.DataAnnotations;

namespace task_backend.Models
{
    public class UserTask
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        [MaxLength(256, ErrorMessage = "Task description cannot exceed 256 characters.")]
        public string TaskDescription { get; set; }
    }
}