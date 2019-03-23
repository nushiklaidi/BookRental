using BookRental.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using static BookRental.Models.BookRent;

namespace BookRental.ViewModel
{
    public class BookRentalViewModel
    {
        
        public int Id { get; set; }

        //Book Details
        public int BookId { get; set; }
        
        public string ISBN { get; set; }
        
        public string Title { get; set; }
        
        public string Author { get; set; }
        
        public string Description { get; set; }
        
        [DataType(DataType.ImageUrl)]
        public string ImageUrl { get; set; }
        
        [Range(0, 1000)]
        public int Availability { get; set; }
        
        [DataType(DataType.Currency)]
        public double Price { get; set; }

        [Display(Name = "Date Added")]
        [DisplayFormat(DataFormatString = "{0: MMM dd yyyy}")]
        public DateTime? DateAdded { get; set; }
        
        public int GenreId { get; set; }

        public Genre Genre { get; set; }

        [Display(Name = "Publication Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0: MMM dd yyyy}")]
        public DateTime PublicationDate { get; set; }
        
        public int Pages { get; set; }

        [Display(Name = "Product Dimensions")]
        public string ProductDimensions { get; set; }
        
        public string Publisher { get; set; }



        //BookRent Details
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0: MMM dd yyyy}")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Actual End Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0: MMM dd yyyy}")]
        public DateTime? ActualEndDate { get; set; }

        [Display(Name = "Schedule End Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0: MMM dd yyyy}")]
        public DateTime? ScheduleEndDate { get; set; }

        [Display(Name = "Additional Charge")]
        public double? AdditionalCharge { get; set; }

        [Display(Name = "Rental Price")]
        public double? RentalPrice { get; set; }
        
        public string RentalDuration { get; set; }

        public string Status { get; set; }

        public double RentalPriceOneMonth { get; set; }

        public double RentalPriceSixMonth { get; set; }
        

        //User Details
        public string UserId { get; set; }
        
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        
        public string Name {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        [Display(Name = "Birth Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0: MMM dd yyyy}")]
        public DateTime? BirthDate { get; set; }
    }
}