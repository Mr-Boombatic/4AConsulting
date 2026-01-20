if object_id(' T', 'U') is not null
    drop table T;

create table T(
    N nvarchar(50) not null,
    S decimal(18,2) not null,
    constraint PK_T primary key clustered (N asc),
    constraint CK_T_S_Positive check (S >= 0)
);


if object_id(' sp_TransferMoney', 'P') is not null
    drop procedure sp_TransferMoney;

insert into T (N, S) values 
    ('ACC001', 10000.00),
    ('ACC002', 5000.00),
    ('ACC003', 15000.00),
    ('ACC004', 7500.50),
    ('ACC005', 25000.00);
go

create procedure sp_TransferMoney
    @N1 nvarchar(50),
    @N2 nvarchar(50),
    @S decimal(18,2),
    @ResultCode int output
as
begin
    set nocount on;
    set @ResultCode = 0;
    
    declare @CurrentBalance1 decimal(18,2);
    declare @CurrentBalance2 decimal(18,2);
    declare @Account1Exists bit = 0;
    declare @Account2Exists bit = 0;
    declare @ErrorMessage nvarchar(500);
    declare @ErrorSeverity int;
    declare @ErrorState int;
    
    if @S <= 0
    begin
        set @ResultCode = 3;
        raiserror('Сумма перевода должна быть больше нуля', 16, 1);
        return;
    end
    
    if @N1 = @N2
    begin
        set @ResultCode = 3;
        raiserror('Номера счетов не должны совпадать', 16, 1);
        return;
    end
    
    begin transaction;
    
    begin try
        select 
            @CurrentBalance1 = S,
            @Account1Exists = 1
        from T with (updlock, rowlock)
        where N = @N1;
        
        select 
            @CurrentBalance2 = S,
            @Account2Exists = 1
        from T with (updlock, rowlock)
        where N = @N2;
        
        if @Account1Exists = 0 or @Account2Exists = 0
        begin
            set @ResultCode = 2;
            rollback transaction;
            raiserror('Один из счетов не найден', 16, 1);
            return;
        end
        
        if @CurrentBalance1 < @S
        begin
            set @ResultCode = 1;
            rollback transaction;
            set @ErrorMessage = 
                'Недостаточно средств на счете ' + @N1 + 
                '. Текущий баланс: ' + cast(@CurrentBalance1 as nvarchar(20)) + 
                ', требуется: ' + cast(@S as nvarchar(20));
            raiserror(@ErrorMessage, 16, 1);
            return;
        end
        
        update T
        set S = S - @S
        where N = @N1;
        
        if @@ROWCOUNT = 0
        begin
            set @ResultCode = 3;
            rollback transaction;
            raiserror('Ошибка при списании средств со счета %s', 16, 1, @N1);
            return;
        end
        
        update T
        set S = S + @S
        where N = @N2;
        
        if @@ROWCOUNT = 0
        begin
            set @ResultCode = 3;
            rollback transaction;
            raiserror('Ошибка при зачислении средств на счет %s', 16, 1, @N2);
            return;
        end
        
        commit transaction;
        set @ResultCode = 0;
        
    end try
    begin catch
        if @@TRANCOUNT > 0
        begin
            rollback transaction;
        end
        
        set @ResultCode = 3;
        set @ErrorMessage = error_message();
        set @ErrorSeverity = error_severity();
        set @ErrorState = error_state();
        
        raiserror(@ErrorMessage, @ErrorSeverity, @ErrorState);
    end catch
end
go

select N as AccountNumber, S as Balance from  T order by N;

declare @ResultCode int;
exec  sp_TransferMoney
    @N1 = 'ACC001',
    @N2 = 'ACC002',
    @S = 333.33,
    @ResultCode = @ResultCode output;

select @ResultCode as ResultCode;

select N as AccountNumber, S as Balance from  T order by N;
go