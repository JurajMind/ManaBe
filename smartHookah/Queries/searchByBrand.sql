declare @query varchar(25)

set @query = '"*'+@sp+ '*"'
SELECT  P.Id ,P.AccName Name,B.DisplayName Brand,P.[Discriminator] 'Type', CASE WHEN  O.PersonId = @personId  Then 1 ELSE 0 END  as Owned 
FROM PipeAccesory P 
	JOIN Brand B On (P.BrandName = B.Name) 
	left JOIN  OwnPipeAccesories O On ( O.PipeAccesoryId = P.Id And O.PersonId = @personId)
	Where (Contains(B.DisplayName,@query)) and P.[Discriminator] = upper(@type) Order by B.DisplayName DESC, P.AccName
	OFFSET ((@PageNumber - 1) * @RowspPage) ROWS
FETCH NEXT @RowspPage ROWS ONLY
