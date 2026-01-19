using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using _4AConsultingMVC.DataAccess;
using _4AConsultingMVC.Models;
using _4AConsultingMVC.Services;

namespace _4AConsultingMVC.Controllers
{
    public class BooksController : Controller
    {
        private readonly IStoredProcedureService _service;

        public BooksController(IStoredProcedureService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            try
            {
                var books = _service.GetAllBooks();
                return View(books);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error loading books list", ex);
                ViewBag.ErrorMessage = "Не удалось загрузить список книг";
                return View(new List<Book>());
            }
        }

        public IActionResult Details(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid book id");
            }

            try
            {
                var book = _service.GetBookById(id);
                if (book == null)
                {
                    return NotFound();
                }

                var viewModel = new BookViewModel
                {
                    Id = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    PublicationYear = book.PublicationYear,
                    Description = book.Description,
                    TableOfContentsHtml = book.GetTableOfContentsHtml(),
                    CreatedDate = book.CreatedDate,
                    ModifiedDate = book.ModifiedDate
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error loading book", ex);
                ViewBag.ErrorMessage = "Не удалось загрузить книгу";
                return View();
            }
        }

        public IActionResult Create()
        {
            var viewModel = new BookViewModel();
            PopulateYearList(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BookViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                PopulateYearList(viewModel);
                return View(viewModel);
            }

            try
            {
                var book = new Book
                {
                    Title = viewModel.Title?.Trim(),
                    Author = viewModel.Author?.Trim(),
                    PublicationYear = viewModel.PublicationYear,
                    Description = viewModel.Description?.Trim(),
                    CreatedDate = DateTime.Now
                };

                book.SetTableOfContentsHtml(viewModel.TableOfContentsHtml);

                int newId = _service.CreateBook(book);
                if (newId > 0)
                {
                    return RedirectToAction(nameof(Details), new { id = newId });
                }
                else
                {
                    ModelState.AddModelError("", "Не удалось создать книгу");
                    PopulateYearList(viewModel);
                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error creating book", ex);
                ModelState.AddModelError("", "Не удалось создать книгу");
                PopulateYearList(viewModel);
                return View(viewModel);
            }
        }

        public IActionResult Edit(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid book id");
            }

            try
            {
                var book = _service.GetBookById(id);
                if (book == null)
                {
                    return NotFound();
                }

                var viewModel = new BookViewModel
                {
                    Id = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    PublicationYear = book.PublicationYear,
                    Description = book.Description,
                    TableOfContentsHtml = book.GetTableOfContentsHtml(),
                    CreatedDate = book.CreatedDate,
                    ModifiedDate = book.ModifiedDate
                };

                PopulateYearList(viewModel);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error loading book for edit", ex);
                ViewBag.ErrorMessage = "Не удалось загрузить книгу";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, BookViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                PopulateYearList(viewModel);
                return View(viewModel);
            }

            try
            {
                var book = new Book
                {
                    Id = id,
                    Title = viewModel.Title?.Trim(),
                    Author = viewModel.Author?.Trim(),
                    PublicationYear = viewModel.PublicationYear,
                    Description = viewModel.Description?.Trim(),
                    ModifiedDate = DateTime.Now
                };

                book.SetTableOfContentsHtml(viewModel.TableOfContentsHtml);

                bool success = _service.UpdateBook(book);
                if (success)
                {
                    return RedirectToAction(nameof(Details), new { id = id });
                }
                else
                {
                    ModelState.AddModelError("", "Не удалось обновить книгу");
                    PopulateYearList(viewModel);
                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error updating book", ex);
                ModelState.AddModelError("", "Не удалось обновить книгу");
                PopulateYearList(viewModel);
                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid book id");
            }

            try
            {
                bool success = _service.DeleteBook(id);
                if (success)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Не удалось удалить книгу. Возможно, книга не существует.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error deleting book", ex);
                TempData["ErrorMessage"] = "Не удалось удалить книгу";
                return RedirectToAction(nameof(Index));
            }
        }

        private void PopulateYearList(BookViewModel viewModel)
        {
            ViewBag.Years = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Выберите год --", Selected = !viewModel.PublicationYear.HasValue }
            };

            int currentYear = DateTime.Now.Year;
            int endYear = currentYear + 5;
            int startYear = 1000;

            for (int year = endYear; year >= startYear; year--)
            {
                ViewBag.Years.Add(new SelectListItem
                {
                    Value = year.ToString(),
                    Text = year.ToString(),
                    Selected = viewModel.PublicationYear.HasValue && viewModel.PublicationYear.Value == year
                });
            }
        }
    }
}
