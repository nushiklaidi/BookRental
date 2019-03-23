using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookRental.Models
{
    public class ThumbnailModel
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Descrition { get; set; }
        public string ImageUrl { get; set; }
        public string Link { get; set; }
    }
}