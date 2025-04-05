
using System.Collections.Generic;

namespace HomeOwners.ViewModels
{
    public class PollViewModel
    {
        public int PollId { get; set; }
        public string? Question { get; set; }
        public List<PollOptionViewModel>? Options { get; set; }
        public bool IsActive { get; set; }
    }

    public class PollOptionViewModel
    {
        public int PollOptionId { get; set; }
        public string? OptionText { get; set; }
        public int Votes { get; set; }
    }
}
