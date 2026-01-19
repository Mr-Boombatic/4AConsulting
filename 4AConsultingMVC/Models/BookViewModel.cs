using System.ComponentModel.DataAnnotations;

namespace _4AConsultingMVC.Models
{
    public class BookViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название обязательно для заполнения")]
        [Display(Name = "Название")]
        [StringLength(200)]
        public string Title { get; set; }

        [Display(Name = "Автор")]
        [StringLength(200)]
        public string Author { get; set; }

        [Display(Name = "Год издания")]
        public int? PublicationYear { get; set; }

        [Display(Name = "Описание")]
        [StringLength(1000)]
        public string Description { get; set; }

        [Display(Name = "Оглавление")]
        public string TableOfContentsHtml { get; set; }

        [Display(Name = "Дата создания")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Дата изменения")]
        public DateTime? ModifiedDate { get; set; }
    }
}
