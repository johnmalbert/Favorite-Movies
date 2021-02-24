using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FavoriteMovies.Models
{
    public class Movie
    {
        [Key]
        public int MovieId {get; set;}
        [Required(ErrorMessage="Please Enter a Title")]
        [MinLength(3,ErrorMessage="Length must be at least three characters.")]
        public string Title {get; set;}
        
        [Required]
        [MinLength(3,ErrorMessage="Length must be at least three characters.")]
        public string Starring {get; set;}
        
        [Required]
        [MinLength(10,ErrorMessage="Length must be at least ten characters.")]
        public string ImageUrl { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public User PostedBy { get; set; }
        public List<Like> Likes { get; set; } //must go through the middle man, Like.cs

    }
}