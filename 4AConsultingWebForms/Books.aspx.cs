using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using _4AConsultingWebForms.DataAccess;
using _4AConsultingWebForms.Models;
using _4AConsultingWebForms.Services;

namespace _4AConsultingWebForms
{
    public partial class Books : Page
    {
        private StoredProcedureService _service;

        protected void Page_Load(object sender, EventArgs e)
        {
            _service = new StoredProcedureService();

            if (!IsPostBack)
            {
                LoadBooks();
            }
        }

        private void LoadBooks()
        {
            try
            {
                var books = _service.GetAllBooks();
                gvBooks.DataSource = books;
                gvBooks.DataBind();
                HideErrorMessage();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error loading books list", ex);
                ShowErrorMessage("Не удалось загрузить список книг");
                gvBooks.DataSource = null;
                gvBooks.DataBind();
            }
        }

        private void ShowErrorMessage(string message)
        {
            lblErrorMessage.Text = message;
            lblErrorMessage.Visible = true;
        }

        private void HideErrorMessage()
        {
            lblErrorMessage.Visible = false;
            lblErrorMessage.Text = string.Empty;
        }


        protected void btnAddNew_Click(object sender, EventArgs e)
        {
            Response.Redirect("Book.aspx");
        }

        protected void gvBooks_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "View" || e.CommandName == "Edit")
            {
                string idString = e.CommandArgument?.ToString()?.Trim();
                
                if (string.IsNullOrWhiteSpace(idString) || 
                    !IsValidInteger(idString) ||
                    !int.TryParse(idString, out int bookId) || bookId <= 0)
                {
                    Logger.LogError($"Invalid book id in RowCommand (must be a positive integer): {e.CommandArgument}");
                    ShowErrorMessage("Неверный идентификатор книги");
                    return;
                }

                Response.Redirect($"Book.aspx?id={bookId}&mode={(e.CommandName == "View" ? "view" : "edit")}");
            }
        }

        protected void gvBooks_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.RowIndex >= gvBooks.DataKeys.Count)
                {
                    Logger.LogError($"Invalid row index in RowDeleting: {e.RowIndex}");
                    ShowErrorMessage("Неверный индекс строки");
                    return;
                }

                object keyValue = gvBooks.DataKeys[e.RowIndex].Value;
                string idString = keyValue?.ToString()?.Trim();
                
                if (string.IsNullOrWhiteSpace(idString) || 
                    !IsValidInteger(idString) ||
                    !int.TryParse(idString, out int bookId) || bookId <= 0)
                {
                    Logger.LogError($"Invalid book id in RowDeleting (must be a positive integer): {keyValue}");
                    ShowErrorMessage("Неверный идентификатор книги");
                    return;
                }

                _service = new StoredProcedureService();
                bool success = _service.DeleteBook(bookId);

                if (success)
                {
                    LoadBooks();
                }
                else
                {
                    ShowErrorMessage("Не удалось удалить книгу. Возможно, книга не существует.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error deleting book", ex);
                ShowErrorMessage("Не удалось удалить книгу");
            }
        }

        protected void gvBooks_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvBooks.PageIndex = e.NewPageIndex;
            LoadBooks();
        }

        protected string GetTableOfContentsPreview(object dataItem)
        {
            if (dataItem is Models.Book book && book.TableOfContents != null)
            {
                var htmlElement = book.TableOfContents.Element("TableOfContents");
                if (htmlElement != null)
                {
                    var htmlContent = htmlElement.Value;
                    var preview = htmlContent.Length > 50 ? htmlContent.Substring(0, 50) + "..." : htmlContent;
                    return preview;
                }
            }
            return "Нет данных";
        }

        protected string GetTableOfContentsXml(object dataItem)
        {
            if (dataItem is Models.Book book && book.TableOfContents != null)
            {
                return book.TableOfContents.ToString();
            }
            return string.Empty;
        }

        protected int GetSectionCount(object dataItem)
        {
            if (dataItem is Models.Book book && book.TableOfContents != null)
            {
                try
                {
                    var sections = book.TableOfContents.Descendants("Section");
                    return sections.Count();
                }
                catch
                {
                    return 0;
                }
            }
            return 0;
        }

        protected int GetSubSectionCount(object dataItem)
        {
            if (dataItem is Models.Book book && book.TableOfContents != null)
            {
                try
                {
                    var subSections = book.TableOfContents.Descendants("SubSection");
                    return subSections.Count();
                }
                catch
                {
                    return 0;
                }
            }
            return 0;
        }

        private bool IsValidInteger(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return Regex.IsMatch(value, @"^\d+$");
        }
    }
}
