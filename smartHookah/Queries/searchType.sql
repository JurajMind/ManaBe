declare @query varchar(25)

set @query = '"*'+@sp+ '*"'
SELECT  P.Id ,P.AccName Name,B.DisplayName Brand,P.[Discriminator] 'Type'
FROM PipeAccesory P 
	JOIN Brand B On (P.BrandName = B.Name)
	Where (Contains(P.AccName,@query) OR Contains(B.DisplayName,@query)) and P.[Discriminator] = upper(@type) Order by B.DisplayName DESC, P.AccName
	OFFSET ((@PageNumber - 1) * @RowspPage) ROWS
FETCH NEXT @RowspPage ROWS ONLY
