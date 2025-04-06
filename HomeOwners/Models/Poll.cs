using System.ComponentModel.DataAnnotations;

namespace HomeOwners.Models
{
    public class Poll
    {
        public int PollId { get; set; }

        [Required(ErrorMessage = "Question is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Question must be between 5 and 200 characters")]
        public string Question { get; set; }

        [Required(ErrorMessage = "At least two options are required")]
        [MinLength(2, ErrorMessage = "At least two options are required")]
        public List<PollOption> Options { get; set; } = new List<PollOption>();

        public bool IsActive { get; set; }
    }


    public class PollOption
    {
        public int PollOptionId { get; set; }

        [Required(ErrorMessage = "Option text is required")]
        public string OptionText { get; set; }

        public int Votes { get; set; }
        public int PollId { get; set; }
        public Poll Poll { get; set; }
    }
}
