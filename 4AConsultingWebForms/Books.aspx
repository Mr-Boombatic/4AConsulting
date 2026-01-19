<%@ Page Title="Список книг" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Books.aspx.cs" Inherits="_4AConsultingWebForms.Books" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main>
        <div class="container-fluid">
            <h2>Список книг</h2>
            
            <div class="mb-3">
                <asp:Button ID="btnAddNew" runat="server" Text="Добавить новую книгу" 
                    CssClass="btn btn-primary" OnClick="btnAddNew_Click" />
            </div>

            <asp:Label ID="lblErrorMessage" runat="server" CssClass="alert alert-danger" Visible="false" />

            <asp:GridView ID="gvBooks" runat="server" 
                CssClass="table table-striped table-bordered" 
                AutoGenerateColumns="False"
                OnRowCommand="gvBooks_RowCommand"
                OnRowDeleting="gvBooks_RowDeleting"
                DataKeyNames="Id"
                AllowPaging="True"
                PageSize="10"
                OnPageIndexChanging="gvBooks_PageIndexChanging">
                <Columns>
                    <asp:BoundField DataField="Id" HeaderText="ID" ItemStyle-Width="50px" />
                    <asp:BoundField DataField="Title" HeaderText="Название" />
                    <asp:BoundField DataField="Author" HeaderText="Автор" />
                    <asp:BoundField DataField="PublicationYear" HeaderText="Год издания" ItemStyle-Width="100px" />
                    <asp:BoundField DataField="Description" HeaderText="Описание" ItemStyle-Width="300px" />
                    <asp:TemplateField HeaderText="Разделов" ItemStyle-Width="80px" ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:Label ID="lblSectionCount" runat="server" Text='<%# GetSectionCount(Container.DataItem) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Подразделов" ItemStyle-Width="100px" ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:Label ID="lblSubSectionCount" runat="server" Text='<%# GetSubSectionCount(Container.DataItem) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="CreatedDate" HeaderText="Дата создания" 
                        DataFormatString="{0:dd.MM.yyyy HH:mm}" />
                    <asp:TemplateField HeaderText="Действия" ItemStyle-Width="200px">
                        <ItemTemplate>
                            <div class="d-flex flex-column">
                                <asp:Button ID="btnView" runat="server" Text="Просмотр" 
                                    CssClass="btn btn-sm btn-info mb-2" 
                                    CommandName="View" 
                                    CommandArgument='<%# Eval("Id") %>' />
                                <asp:Button ID="btnEdit" runat="server" Text="Редактировать" 
                                    CssClass="btn btn-sm btn-warning mb-2" 
                                    CommandName="Edit" 
                                    CommandArgument='<%# Eval("Id") %>' />
                                <asp:Button ID="btnDelete" runat="server" Text="Удалить" 
                                    CssClass="btn btn-sm btn-danger" 
                                    CommandName="Delete" 
                                    CommandArgument='<%# Eval("Id") %>' 
                                    OnClientClick="return confirm('Вы уверены, что хотите удалить эту книгу?');" />
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <PagerSettings Mode="NumericFirstLast" />
                <PagerStyle CssClass="pagination" />
            </asp:GridView>
        </div>
    </main>
</asp:Content>
