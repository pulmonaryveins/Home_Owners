using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeOwners.Models
{
    public class PollVote
    {
        public int Id { get; set; }

        [Required]
        public int PollId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public bool VoteValue { get; set; } // true = Yes, false = No

        public DateTime VoteDate { get; set; } = DateTime.Now;

        [ForeignKey("PollId")]
        public Poll Poll { get; set; }
    }
}