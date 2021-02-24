using System;
using System.ComponentModel.DataAnnotations;

namespace FavoriteMovies.Models
{
    public class Like
    {
        [Key]
        public int LikeId { get; set; }
        public int UserId { get; set; } //connects us to one user
        public User UserWhoLikes { get; set;} //references the actual object
        public int MovieId { get; set; } //connects us to one movie
        public Movie LikedMovie { get; set;} // references the actual object
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}