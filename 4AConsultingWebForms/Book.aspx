<%@ Page Title="Карточка книги" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Book.aspx.cs" Inherits="_4AConsultingWebForms.Book" ValidateRequest="false" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main>
        <div class="container-fluid">
            <h2 runat="server" id="pageTitle">Карточка книги</h2>
            
            <asp:Panel ID="pnlViewMode" runat="server" Visible="false">
                <div class="card">
                    <div class="card-body">
                        <div class="mb-3">
                            <asp:Label ID="lblViewMessage" runat="server" CssClass="alert" Visible="false" />
                        </div>
                        <div class="row mb-3">
                            <div class="col-md-2"><strong>ID:</strong></div>
                            <div class="col-md-10"><asp:Label ID="lblViewId" runat="server" /></div>
                        </div>
                        <div class="row mb-3">
                            <div class="col-md-2"><strong>Название:</strong></div>
                            <div class="col-md-10"><asp:Label ID="lblViewTitle" runat="server" /></div>
                        </div>
                        <div class="row mb-3">
                            <div class="col-md-2"><strong>Автор:</strong></div>
                            <div class="col-md-10"><asp:Label ID="lblViewAuthor" runat="server" /></div>
                        </div>
                        <div class="row mb-3">
                            <div class="col-md-2"><strong>Год издания:</strong></div>
                            <div class="col-md-10"><asp:Label ID="lblViewPublicationYear" runat="server" /></div>
                        </div>
                        <div class="row mb-3">
                            <div class="col-md-2"><strong>Описание:</strong></div>
                            <div class="col-md-10"><asp:Label ID="lblViewDescription" runat="server" /></div>
                        </div>
                        <div class="row mb-3">
                            <div class="col-md-2"><strong>Оглавление:</strong></div>
                            <div class="col-md-10">
                                <asp:Label ID="lblViewTableOfContents" runat="server" />
                            </div>
                        </div>
                        <div class="row mb-3">
                            <div class="col-md-2"><strong>Дата создания:</strong></div>
                            <div class="col-md-10"><asp:Label ID="lblViewCreatedDate" runat="server" /></div>
                        </div>
                        <div class="row mb-3">
                            <div class="col-md-2"><strong>Дата изменения:</strong></div>
                            <div class="col-md-10"><asp:Label ID="lblViewModifiedDate" runat="server" /></div>
                        </div>
                        <div class="mt-3">
                            <asp:Button ID="btnEditFromView" runat="server" Text="Редактировать" 
                                CssClass="btn btn-warning" OnClick="btnEditFromView_Click" />
                            <asp:Button ID="btnBackFromView" runat="server" Text="Назад к списку" 
                                CssClass="btn btn-secondary" OnClick="btnBack_Click" />
                        </div>
                    </div>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlEditMode" runat="server">
                <div class="card">
                    <div class="card-body">
                        <asp:HiddenField ID="hfBookId" runat="server" />
                        
                        <div class="mb-3">
                            <label for="txtTitle" class="form-label">Название <span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control" MaxLength="200" />
                            <asp:RequiredFieldValidator ID="rfvTitle" runat="server" 
                                ControlToValidate="txtTitle" 
                                ErrorMessage="Название обязательно для заполнения" 
                                CssClass="text-danger" Display="Dynamic" />
                        </div>

                        <div class="mb-3">
                            <label for="txtAuthor" class="form-label">Автор</label>
                            <asp:TextBox ID="txtAuthor" runat="server" CssClass="form-control" MaxLength="200" />
                        </div>

                        <div class="mb-3">
                            <label for="ddlPublicationYear" class="form-label">Год издания</label>
                            <asp:DropDownList ID="ddlPublicationYear" runat="server" CssClass="form-select">
                                <asp:ListItem Value="" Text="-- Выберите год --" Selected="True" />
                            </asp:DropDownList>
                        </div>

                        <div class="mb-3">
                            <label for="txtDescription" class="form-label">Описание</label>
                            <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" 
                                TextMode="MultiLine" Rows="4" MaxLength="1000" />
                        </div>

                        <div class="mb-3">
                            <label for="editorTableOfContents" class="form-label">Оглавление (HTML редактор)</label>
                            <textarea id="editorTableOfContents" name="editorTableOfContents" runat="server" class="form-control"></textarea>
                            <small class="form-text text-muted">Содержимое будет сохранено в XML поле</small>
                        </div>

                        <div class="mb-3">
                            <asp:Label ID="lblMessage" runat="server" CssClass="alert" Visible="false" />
                        </div>

                        <div class="mt-3">
                            <asp:Button ID="btnSave" runat="server" Text="Сохранить" 
                                CssClass="btn btn-primary" OnClick="btnSave_Click" 
                                OnClientClick="updateCKEditorContent(); return true;" />
                            <asp:Button ID="btnCancel" runat="server" Text="Отмена" 
                                CssClass="btn btn-secondary" OnClick="btnCancel_Click" CausesValidation="false" />
                        </div>
                    </div>
                </div>
            </asp:Panel>
        </div>
    </main>

    <!-- CKEditor (бесплатная версия 4.22.1 - последняя стабильная без лицензии) -->
    <script src="https://cdn.ckeditor.com/4.22.1/standard/ckeditor.js"></script>
    <script type="text/javascript">
        // Инициализация CKEditor после полной загрузки страницы
        (function() {
            var editorId = '<%= editorTableOfContents.ClientID %>';
            var initialized = false;
            
            function initializeCKEditor() {
                // Предотвращаем повторную инициализацию
                if (initialized) {
                    return;
                }
                
                var editorElement = document.getElementById(editorId);
                
                // Проверяем, что элемент существует
                if (!editorElement) {
                    return;
                }
                
                // Проверяем, что CKEditor загружен
                if (typeof CKEDITOR === 'undefined') {
                    return;
                }
                
                // Проверяем, не инициализирован ли уже редактор
                if (CKEDITOR.instances[editorId]) {
                    initialized = true;
                    return;
                }
                
                // Создаем экземпляр редактора
                try {
                    CKEDITOR.replace(editorId, {
                        height: 300,
                        toolbar: [
                            { name: 'basicstyles', items: ['Bold', 'Italic', 'Underline', 'Strike'] },
                            { name: 'paragraph', items: ['NumberedList', 'BulletedList', '-', 'Blockquote'] },
                            { name: 'links', items: ['Link', 'Unlink'] },
                            { name: 'styles', items: ['Format', 'Font', 'FontSize'] },
                            { name: 'colors', items: ['TextColor', 'BGColor'] },
                            { name: 'tools', items: ['Maximize', 'Source'] }
                        ]
                    });
                    initialized = true;
                } catch (e) {
                    // Игнорируем ошибки, если элемент уже обработан
                    console.warn('CKEditor инициализация:', e.message);
                }
            }
            
            // Пробуем инициализировать при загрузке DOM
            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', function() {
                    setTimeout(initializeCKEditor, 150);
                });
            } else {
                // DOM уже загружен
                setTimeout(initializeCKEditor, 150);
            }
            
            // Резервный вариант при полной загрузке страницы
            window.addEventListener('load', function() {
                if (!initialized) {
                    setTimeout(initializeCKEditor, 300);
                }
            });
        })();
        
        // Функция для обновления содержимого textarea из CKEditor перед отправкой формы
        function updateCKEditorContent() {
            var editorId = '<%= editorTableOfContents.ClientID %>';
            if (typeof CKEDITOR !== 'undefined' && CKEDITOR.instances[editorId]) {
                // Обновляем содержимое textarea из редактора
                CKEDITOR.instances[editorId].updateElement();
            }
        }
    </script>
</asp:Content>
