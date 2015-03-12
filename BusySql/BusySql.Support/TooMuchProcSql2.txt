declare @Items TABLE (Item VARCHAR(8000));
declare @ItemList varchar(8000);
DECLARE @DelimIndex     INT;
DECLARE @Item   VARCHAR(8000);
set @ItemList = 'car,moto,bicycle,hope,rio,grande,canal,juggle,money,jar,compose,film,actor,code,book,the art of hiding secrects,
                 the universe,brazil,amazon,forest,ama,love,determined,complecompose,film,actor,code,book,the art of hiding secrects,
				 the universe,brazil,amazon,forest,ama,love,determined,complecompose,film,actor,code,book,the art of hiding secrects,
				 the universe,brazil,amazon,forest,ama,love,determined,complecompose,film,actor,code,book,the art of hiding secrects,
				 the universe,brazil,amazon,forest,ama,love,determined,complecompose,film,actor,code,book,the art of hiding secrects,
				 the universe,brazil,amazon,forest,ama,love,determined,complecompose,film,actor,code,book,the art of hiding secrects,
				 the universe,brazil,amazon,forest,ama,love,determined,complecompose,film,actor,code,book,the art of hiding secrects,
				 the universe,brazil,amazon,forest,ama,love,determined,complecompose,film,actor,code,book,the art of hiding secrects,
				 the universe,brazil,amazon,forest,ama,love,determined,complecompose,film,actor,code,book,the art of hiding secrects,
				 the universe,brazil,amazon,forest,ama,love,determined,complecompose,film,actor,code,book,the art of hiding secrects,
				 the universe,brazil,amazon,forest,ama,love,determined,complecompose,film,actor,code,book,the art of hiding secrects,
				 the universe,brazil,amazon,forest,ama,love,determined,complecompose,film,actor,code,book,the art of hiding secrects,' ;			
declare @Delimiter varchar(1);              
SET @Delimiter = ',';
SET @DelimIndex = CHARINDEX(@Delimiter, @ItemList, 0);
WHILE (@DelimIndex != 0)
BEGIN
SET @Item = LTRIM(SUBSTRING(@ItemList, 0, @DelimIndex))
INSERT INTO @Items VALUES (@Item)
-- Set @ItemList = @ItemList minus one less item
SET @ItemList = SUBSTRING(@ItemList, @DelimIndex+1, LEN(@ItemList)-@DelimIndex)
SET @DelimIndex = CHARINDEX(@Delimiter, @ItemList, 0)
END; -- End WHILE
select * from @Items;