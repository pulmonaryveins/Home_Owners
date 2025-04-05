using System.Collections.Generic;

namespace HomeOwners.Models
{
    public class Poll
    {
        public int PollId { get; set; }
        public string Question { get; set; }
        public virtual ICollection<PollOption> Options { get; set; }
        public bool IsActive { get; set; }
    }

    public class PollOption
    {
        public int PollOptionId { get; set; }
        public string OptionText { get; set; }
        public int Votes { get; set; }
        public int PollId { get; set; }
        public virtual Poll Poll { get; set; }
    }
}
