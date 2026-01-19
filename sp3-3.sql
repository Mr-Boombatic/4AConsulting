use library
go

drop procedure if exists sp_getallrecords
drop procedure if exists sp_getrecordbyid
drop procedure if exists sp_createrecord
drop procedure if exists sp_updaterecord
drop procedure if exists sp_deleterecord
go

drop table if exists books
go

create table books (
    id int identity(1,1) not null primary key,
    title nvarchar(200) not null,
    author nvarchar(200) null,
    publicationyear int null,
    description nvarchar(1000) null,
    tableofcontents xml null,
    createddate datetime not null,
    modifieddate datetime null
)
go

create procedure sp_getallrecords
as
begin
    set nocount on;

    select
        id,
        title,
        author,
        publicationyear,
        description,
        tableofcontents,
        createddate,
        modifieddate
    from books
    order by createddate desc
end
go

create procedure sp_getrecordbyid
    @id int
as
begin
    set nocount on;

    select
        id,
        title,
        author,
        publicationyear,
        description,
        tableofcontents,
        createddate,
        modifieddate
    from books
    where id = @id
end
go

create procedure sp_createrecord
    @title nvarchar(200),
    @author nvarchar(200) = null,
    @publicationyear int = null,
    @description nvarchar(1000) = null,
    @tableofcontents xml = null,
    @id int output
as
begin
    set nocount on;

    insert into books (
        title,
        author,
        publicationyear,
        description,
        tableofcontents,
        createddate
    )
    values (
        @title,
        @author,
        @publicationyear,
        @description,
        @tableofcontents,
        getdate()
    );

    set @id = scope_identity();
end
go

create procedure sp_updaterecord
    @id int,
    @title nvarchar(200),
    @author nvarchar(200) = null,
    @publicationyear int = null,
    @description nvarchar(1000) = null,
    @tableofcontents xml = null
as
begin
    set nocount on;

    update books
    set
        title = @title,
        author = @author,
        publicationyear = @publicationyear,
        description = @description,
        tableofcontents = @tableofcontents,
        modifieddate = getdate()
    where id = @id;

    return @@rowcount;
end
go

create procedure sp_deleterecord
    @id int
as
begin
    set nocount on;

    delete from books
    where id = @id;

    return @@rowcount;
end
go
