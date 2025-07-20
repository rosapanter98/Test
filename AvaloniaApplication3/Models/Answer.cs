using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvaloniaApplication3.Models
{
    public partial class Answer : ObservableObject
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }

        // Foreign key
        public int QuestionId { get; set; }

        // Navigation
        public Question Question { get; set; } = null!;

        [NotMapped]
        [ObservableProperty]
        private bool isSelected;

        [NotMapped]
        [ObservableProperty]
        private bool? isUserCorrect;
    }
}
