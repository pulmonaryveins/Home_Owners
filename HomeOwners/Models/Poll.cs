// HomeOwners/Models/Poll.cs
using System.ComponentModel.DataAnnotations;

namespace HomeOwners.Models
{
    public class Poll
    {
        public int PollId { get; set; }

        [Required(ErrorMessage = "Question is required")]
        public string Question { get; set; }

        [Required(ErrorMessage = "At least two options are required")]
        public virtual List<PollOption> Options { get; set; } = new List<PollOption>();

        public bool IsActive { get; set; }
    }

    public class PollOption
    {
        public int PollOptionId { get; set; }

        [Required(ErrorMessage = "Option text is required")]
        public string OptionText { get; set; }

        public int Votes { get; set; }
        public int PollId { get; set; }
        public virtual Poll Poll { get; set; }
    }
}
