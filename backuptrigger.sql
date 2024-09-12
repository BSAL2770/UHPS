drop trigger if exists autostatus
go
create trigger autostatus on trackingrecords
after insert, update
as begin
        declare @tracknum int;
        declare @dest int;
        declare @out int;
        declare @stat int;
        select @tracknum=tracknum, @out=destination, @dest=addressid from inserted,Packages where Packages.Id = inserted.TrackNum;
        if @out is null
            select @stat = 0
        else
            begin
                if @out = @dest
                    select @stat = 2
                else
                    select @stat = 1
            end
        update Packages set status = @stat where Id=@tracknum
end


--trigger: automatically set the cost of a package
drop trigger if exists cost
            go
            create trigger cost on packages
            after insert, update
            as begin
                declare @total float;
                declare @W float;
                declare @exp int;
                declare @ident int;
                declare @depth float;
                declare @width float;
                declare @height float;
                declare @shipclass int;
                select @ident=Id from inserted;
                select @W = packages.Weight, @exp = packages.Express, @depth = packages.Depth, @width = packages.Width, @height = packages.Height from packages where packages.Id =@ident;
                declare @bound int;
                select @bound = Id from ShipmentClasses where ExpressCost >= 0;
                select @shipclass = (select top 1 id from ShipmentClasses where @height < ShipmentClasses.MaxHeight and @width < ShipmentClasses.MaxWidth and @depth < ShipmentClasses.MaxLength order by GroundCost)
                if @shipclass is null
                select @shipclass=(select top 1 id from ShipmentClasses order by GroundCost desc)

                    if @exp = 1
                    begin
                    select @total = ShipmentClasses.ExpressCost from ShipmentClasses where id=@shipclass;

                    end

                    if @exp = 0
                    begin
                    select @total = ShipmentClasses.GroundCost from ShipmentClasses where id=@shipclass;
                    end

                set @total = (@total * @W);	
                update packages set packages.ShipCost = @total, packages.classid=@shipclass where packages.Id = @ident;
            end
      

drop trigger if exists NewSale
go
create trigger NewSale on sales
after insert
as begin
    declare @ident int;
	declare @quant int;
	declare @pid int;
	declare @tot float;
    select @ident=Id, @quant = Quantity, @pid = ProductID from inserted;
	select @tot = products.UnitCost from products where products.Id = @pid;
	set @tot = @tot*@quant;
    update sales set sales.PurchaseDate = getdate() where sales.ID=@ident;
	update sales set sales.Total = @tot where sales.ID=@ident;
	update products set products.Stock = (products.Stock-@quant) where products.Id = @pid;
end

--update sale
drop trigger if exists UpdateSale
go
create trigger UpdateSale on sales
after update
as begin
declare @newQuant int;
declare @oldQuant int;
declare @newPID int;
declare @oldPID int;
declare @ident int;
declare @tot float;
declare @cost float;
select @ident=Id, @newQuant=Quantity, @newPID=ProductID from inserted;
select @oldPID=ProductID, @oldQuant=Quantity from deleted;
select @cost=UnitCost from products where products.ID = @newPID;
if ((@oldQuant != @newQuant) and (@newPID = @oldPID))
	begin
	update products set products.Stock = (products.Stock+(@oldQuant-@newQuant)) where products.Id = @newPID;
	end
if (@newPID != @oldPID)
	begin
	update products set products.Stock = (products.Stock+@oldQuant) where products.Id = @oldPID;
	update products set products.Stock = (products.Stock-@newQuant) where products.Id = @newPID;
	end
set @tot = @newQuant*@cost;
update sales set sales.Total = @tot where sales.ID = @ident;
update sales set sales.PurchaseDate=getdate() where sales.ID = @ident;

end


