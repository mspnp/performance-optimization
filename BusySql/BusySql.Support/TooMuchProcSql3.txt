declare @Items varchar(8000);
declare @ItemList varchar(8000);
DECLARE @DelimIndex     INT;
DECLARE @Item   VARCHAR(8000);
set @ItemList = 'ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca,ca';
declare @Delimiter varchar(1);              
SET @Delimiter = ',';
SET @DelimIndex = CHARINDEX(@Delimiter, @ItemList, 0);
SET @Item = LTRIM(SUBSTRING(@ItemList, 0, @DelimIndex));
SET @Items = @Item;
WHILE (@DelimIndex != 0)
BEGIN
    SET @ItemList = SUBSTRING(@ItemList, @DelimIndex+1, LEN(@ItemList)-@DelimIndex)
	SET @DelimIndex = CHARINDEX(@Delimiter, @ItemList, 0)
	SET @Item = LTRIM(SUBSTRING(@ItemList, 0, @DelimIndex))
	Set @Item =Replace('ca','ca','cat')
	SET @Items=CONCAT(@Items,' ',@Item)	
END; -- End WHILE
SET @Item = LTRIM(SUBSTRING(@ItemList, 0, @DelimIndex))
SET @Items=CONCAT(@Items,' ',@Item);	
select  @Items as FormattedList;