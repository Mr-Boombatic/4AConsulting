using System;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using _4AConsultingWebForms.DataAccess;
using _4AConsultingWebForms.Models;
using _4AConsultingWebForms.Services;

namespace _4AConsultingWebForms
{
    public partial class Book : Page
    {
        private StoredProcedureService _service;
        private int _bookId;
        private string _mode;
        
        private bool ValidationFailed
        {
            get { return ViewState["ValidationFailed"] != null && (bool)ViewState["ValidationFailed"]; }
            set { ViewState["ValidationFailed"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _service = new StoredProcedureService();

            string validationError = ValidateQueryParameters();
            if (!string.IsNullOrEmpty(validationError))
            {
                ValidationFailed = true;
                pnlViewMode.Visible = false;
                pnlEditMode.Visible = true;
                pageTitle.InnerText = "Ошибка";
                DisableEditForm();
                ShowMessage(validationError, false);
                return;
            }
            else
            {
                ValidationFailed = false;
            }

            if (!IsPostBack)
            {
                PopulateYearList();

                if (_bookId > 0)
                {
                    LoadBook(_bookId);
                }

                if (_mode == "view")
                {
                    pnlViewMode.Visible = true;
                    pnlEditMode.Visible = false;
                    pageTitle.InnerText = "Просмотр книги";
                }
                else
                {
                    pnlViewMode.Visible = false;
                    pnlEditMode.Visible = true;
                    pageTitle.InnerText = _bookId > 0 ? "Редактирование книги" : "Создание новой книги";
                }
            }
        }

        private string ValidateQueryParameters()
        {
            string idParam = Request.QueryString["id"];
            string modeParam = Request.QueryString["mode"];

            if (!string.IsNullOrWhiteSpace(idParam))
            {
                idParam = idParam.Trim();
                
                if (!IsValidInteger(idParam) || !int.TryParse(idParam, out _bookId) || _bookId <= 0)
                {
                    string errorMessage = $"Неверный параметр id. Ожидается целое положительное число, получено: '{idParam}'";
                    Logger.LogError($"Invalid id parameter (must be a positive integer): {idParam}");
                    return errorMessage;
                }
            }
            else
            {
                _bookId = 0;
            }

            if (!string.IsNullOrWhiteSpace(modeParam))
            {
                modeParam = modeParam.ToLower().Trim();
                if (modeParam != "view" && modeParam != "edit")
                {
                    string errorMessage = $"Неверный параметр mode. Допустимые значения: 'view' или 'edit', получено: '{modeParam}'";
                    Logger.LogError($"Invalid mode parameter: {modeParam}");
                    return errorMessage;
                }
                _mode = modeParam;
            }
            else
            {
                _mode = "edit";
            }

            if (_mode == "view" && _bookId <= 0)
            {
                string errorMessage = "Для режима просмотра (view) требуется указать корректный параметр id";
                Logger.LogError("Mode 'view' requires valid id parameter, but id is missing or invalid");
                return errorMessage;
            }

            if (_mode == "edit" && !string.IsNullOrWhiteSpace(idParam) && _bookId <= 0)
            {
                string errorMessage = $"Неверный параметр id для режима редактирования. Ожидается целое положительное число, получено: '{idParam}'";
                Logger.LogError($"Mode 'edit' with id parameter requires valid id, but got invalid value: {idParam}");
                return errorMessage;
            }

            return string.Empty;
        }

        private void LoadBook(int id)
        {
            try
            {
                var book = _service.GetBookById(id);
                if (book != null)
                {
                    hfBookId.Value = book.Id.ToString();

                    lblViewId.Text = book.Id.ToString();
                    lblViewTitle.Text = book.Title;
                    lblViewAuthor.Text = book.Author ?? "Не указан";
                    lblViewPublicationYear.Text = book.PublicationYear?.ToString() ?? "Не указан";
                    lblViewDescription.Text = book.Description;
                    lblViewCreatedDate.Text = book.CreatedDate.ToString("dd.MM.yyyy HH:mm");
                    lblViewModifiedDate.Text = book.ModifiedDate?.ToString("dd.MM.yyyy HH:mm") ?? "Не изменялась";

                    if (book.TableOfContents != null)
                    {
                        lblViewTableOfContents.Text = book.GetTableOfContentsHtml();
                    }
                    else
                    {
                        lblViewTableOfContents.Text = "Нет данных";
                    }

                    txtTitle.Text = book.Title;
                    txtAuthor.Text = book.Author ?? string.Empty;
                    
                    if (book.PublicationYear.HasValue)
                    {
                        var yearItem = ddlPublicationYear.Items.FindByValue(book.PublicationYear.Value.ToString());
                        if (yearItem != null)
                        {
                            ddlPublicationYear.SelectedValue = book.PublicationYear.Value.ToString();
                        }
                    }
                    else
                    {
                        ddlPublicationYear.SelectedIndex = 0;
                    }
                    
                    txtDescription.Text = book.Description;

                    if (book.TableOfContents != null)
                    {
                        editorTableOfContents.Value = book.GetTableOfContentsHtml();
                    }
                }
                else
                {
                    ShowMessage("Книга не найдена", false);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error loading book", ex);
                ShowMessage("Не удалось загрузить книгу", false);
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidationFailed)
            {
                DisableEditForm();
                ShowMessage("Сохранение невозможно из-за ошибки валидации параметров", false);
                return;
            }

            string validationError = ValidateQueryParameters();
            if (!string.IsNullOrEmpty(validationError))
            {
                ValidationFailed = true;
                DisableEditForm();
                ShowMessage(validationError, false);
                return;
            }

            if (!btnSave.Enabled || !btnSave.Visible)
            {
                ShowMessage("Сохранение заблокировано", false);
                return;
            }

            if (!Page.IsValid)
            {
                return;
            }

            try
            {
                var book = new Models.Book
                {
                    Title = txtTitle.Text.Trim(),
                    Author = txtAuthor.Text.Trim(),
                    PublicationYear = !string.IsNullOrWhiteSpace(ddlPublicationYear.SelectedValue) && int.TryParse(ddlPublicationYear.SelectedValue, out int year) ? (int?)year : null,
                    Description = txtDescription.Text.Trim()
                };

                string htmlContent = Request.Form[editorTableOfContents.UniqueID];

                book.SetTableOfContentsHtml(htmlContent);

                bool success;
                int bookIdToRedirect = 0;
                
                if (_bookId > 0)
                {
                    book.Id = _bookId;
                    book.ModifiedDate = DateTime.Now;
                    success = _service.UpdateBook(book);
                    if (success)
                    {
                        bookIdToRedirect = _bookId;
                    }
                }
                else
                {
                    book.CreatedDate = DateTime.Now;
                    int newId = _service.CreateBook(book);
                    success = newId > 0;
                    if (success)
                    {
                        bookIdToRedirect = newId;
                    }
                }
                
                if (success && bookIdToRedirect > 0)
                {
                    try
                    {
                        Response.Redirect($"Book.aspx?id={bookIdToRedirect}&mode=view", false);
                        Context.ApplicationInstance.CompleteRequest();
                        return;
                    }
                    catch (System.Threading.ThreadAbortException)
                    {
                        throw;
                    }
                }
                else
                {
                    ShowMessage(success ? "Книга успешно сохранена" : (_bookId > 0 ? "Не удалось обновить книгу" : "Не удалось создать книгу"), success);
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError("Error saving book", ex);
                ShowMessage(_bookId > 0 ? "Не удалось обновить книгу" : "Не удалось создать книгу", false);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("Books.aspx");
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("Books.aspx");
        }

        protected void btnEditFromView_Click(object sender, EventArgs e)
        {
            if (_bookId > 0)
            {
                Response.Redirect($"Book.aspx?id={_bookId}&mode=edit");
            }
            else
            {
                Response.Redirect("Books.aspx");
            }
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            string cssClass = isSuccess ? "alert alert-success" : "alert alert-danger";
            
            if (_mode == "view")
            {
                lblViewMessage.Text = message;
                lblViewMessage.CssClass = cssClass;
                lblViewMessage.Visible = true;
            }
            else
            {
                lblMessage.Text = message;
                lblMessage.CssClass = cssClass;
                lblMessage.Visible = true;
            }
        }

        private void DisableEditForm()
        {
            txtTitle.Enabled = false;
            txtTitle.ReadOnly = true;
            txtAuthor.Enabled = false;
            txtAuthor.ReadOnly = true;
            ddlPublicationYear.Enabled = false;
            txtDescription.Enabled = false;
            txtDescription.ReadOnly = true;
            editorTableOfContents.Disabled = true;
            btnSave.Enabled = false;
            btnSave.Visible = false;
            rfvTitle.Enabled = false;

            string script = @"
                <script type='text/javascript'>
                    (function() {
                        function disableForm() {
                            var saveButton = document.getElementById('" + btnSave.ClientID + @"');
                            if (saveButton) {
                                saveButton.disabled = true;
                                saveButton.style.display = 'none';
                                saveButton.onclick = function(e) { e.preventDefault(); e.stopPropagation(); return false; };
                            }
                        }
                        
                        if (document.readyState === 'loading') {
                            document.addEventListener('DOMContentLoaded', disableForm);
                        } else {
                            disableForm();
                        }
                        window.addEventListener('load', disableForm);
                    })();
                </script>";
            ClientScript.RegisterStartupScript(this.GetType(), "DisableSaveButton", script);
        }

        private bool IsValidInteger(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return Regex.IsMatch(value, @"^\d+$");
        }

        private void PopulateYearList()
        {
            ddlPublicationYear.Items.Clear();
            ddlPublicationYear.Items.Add(new ListItem("-- Выберите год --", ""));

            int currentYear = DateTime.Now.Year;
            int endYear = currentYear + 5;
            int startYear = 1000;

            for (int year = endYear; year >= startYear; year--)
            {
                ddlPublicationYear.Items.Add(new ListItem(year.ToString(), year.ToString()));
            }
        }
    }
}
