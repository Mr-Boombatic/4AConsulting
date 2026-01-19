using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Xml;
using System.Xml.Linq;
using _4AConsultingWebForms.Models;
using _4AConsultingWebForms.Services;

namespace _4AConsultingWebForms.DataAccess
{
    public class StoredProcedureService
    {
        private string GetConnectionString()
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
            return connectionString;
        }

        public List<Models.Book> GetAllBooks()
        {
            var books = new List<Models.Book>();
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                using (var command = new SqlCommand("sp_GetAllRecords", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var book = new Models.Book
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Author = reader.IsDBNull(reader.GetOrdinal("Author")) ? null : reader.GetString(reader.GetOrdinal("Author")),
                                PublicationYear = reader.IsDBNull(reader.GetOrdinal("PublicationYear")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("PublicationYear")),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                ModifiedDate = reader.IsDBNull(reader.GetOrdinal("ModifiedDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("ModifiedDate"))
                            };

                            int tableOfContentsOrdinal = reader.GetOrdinal("TableOfContents");
                            if (!reader.IsDBNull(tableOfContentsOrdinal))
                            {
                                    var sqlXml = reader.GetSqlXml(tableOfContentsOrdinal);
                                    if (!sqlXml.IsNull)
                                        book.TableOfContents = XDocument.Load(sqlXml.CreateReader());
                            }

                            books.Add(book);
                        }
                    }
                }
            }
            return books;
        }

        public Models.Book GetBookById(int id)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                using (var command = new SqlCommand("sp_GetRecordById", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var book = new Models.Book
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Author = reader.IsDBNull(reader.GetOrdinal("Author")) ? null : reader.GetString(reader.GetOrdinal("Author")),
                                PublicationYear = reader.IsDBNull(reader.GetOrdinal("PublicationYear")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("PublicationYear")),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                ModifiedDate = reader.IsDBNull(reader.GetOrdinal("ModifiedDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("ModifiedDate"))
                            };

                            int tableOfContentsOrdinal = reader.GetOrdinal("TableOfContents");
                            if (!reader.IsDBNull(tableOfContentsOrdinal))
                            {
                                var sqlXml = reader.GetSqlXml(tableOfContentsOrdinal);
                                if (!sqlXml.IsNull)
                                    book.TableOfContents = XDocument.Load(sqlXml.CreateReader());
                            }

                            return book;
                        }
                    }
                }
            }
            return null;
        }

        public int CreateBook(Models.Book book)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                using (var command = new SqlCommand("sp_CreateRecord", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Title", book.Title);
                    command.Parameters.AddWithValue("@Author", (object)book.Author ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PublicationYear", (object)book.PublicationYear ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Description", (object)book.Description ?? DBNull.Value);
                    
                    var xmlParam = new SqlParameter("@TableOfContents", SqlDbType.Xml);
                    if (book.TableOfContents == null)
                    {
                        xmlParam.Value = DBNull.Value;
                    }
                    else
                    {
                        try
                        {
                            string xmlString = book.GetTableOfContentsXml();
                            if (!string.IsNullOrWhiteSpace(xmlString))
                            {
                                XDocument.Parse(xmlString);
                            }
                            xmlParam.Value = xmlString;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("Invalid XML in TableOfContents", ex);
                            throw new InvalidOperationException("Invalid XML in TableOfContents", ex);
                        }
                    }
                    command.Parameters.Add(xmlParam);
                    
                    var idParam = new SqlParameter("@Id", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    command.Parameters.Add(idParam);
                    
                    connection.Open();
                    command.ExecuteNonQuery();
                    
                    return (int)idParam.Value;
                }
            }
        }

        public bool UpdateBook(Models.Book book)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                using (var command = new SqlCommand("sp_UpdateRecord", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", book.Id);
                    command.Parameters.AddWithValue("@Title", book.Title);
                    command.Parameters.AddWithValue("@Author", (object)book.Author ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PublicationYear", (object)book.PublicationYear ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Description", (object)book.Description ?? DBNull.Value);
                    
                    var xmlParam = new SqlParameter("@TableOfContents", SqlDbType.Xml);
                    if (book.TableOfContents == null)
                    {
                        xmlParam.Value = DBNull.Value;
                    }
                    else
                    {
                        try
                        {
                            string xmlString = book.GetTableOfContentsXml();
                            if (!string.IsNullOrWhiteSpace(xmlString))
                            {
                                XDocument.Parse(xmlString);
                            }
                            xmlParam.Value = xmlString;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("Invalid XML in TableOfContents", ex);
                            throw new InvalidOperationException("Invalid XML in TableOfContents", ex);
                        }
                    }
                    command.Parameters.Add(xmlParam);
                    
                    var returnParam = new SqlParameter("@ReturnValue", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    command.Parameters.Add(returnParam);
                    
                    connection.Open();
                    command.ExecuteNonQuery();
                    
                    int rowsAffected = (int)returnParam.Value;
                    return rowsAffected > 0;
                }
            }
        }

        public bool DeleteBook(int id)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                using (var command = new SqlCommand("sp_DeleteRecord", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);
                    
                    var returnParam = new SqlParameter("@ReturnValue", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    command.Parameters.Add(returnParam);
                    
                    connection.Open();
                    command.ExecuteNonQuery();
                    
                    int rowsAffected = (int)returnParam.Value;
                    return rowsAffected > 0;
                }
            }
        }
    }
}
