select * from (
select stocksymbol,
date,
previousDate,
datediff(day, previousdate, date) as dateDifference,
price,
previousprice,
100 * (previousPrice - price) / price as pricedifference
from
(
   select
   stocksymbol,
   convert(datetime, date) as [date],
   lag(convert(datetime, date), 1, null) over ( partition by stocksymbol order by date) as previousdate,
price,
   lag(price, 1, null) over ( partition by stocksymbol order by date) as previousPrice

   from stockprice
   where stocksymbol like 'S%'
) x
--order by date

) Y
where pricedifference > 10 or dateDifference > 10
order by stocksymbol, date
