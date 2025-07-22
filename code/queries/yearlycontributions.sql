select 
    sum(ReceiptAmountGbp) amount, 
    case 
        when month(date) < 4 or (month(date) = 4 and day(date) <= 5) then 
            concat(year(date) - 1, '/', year(date))
        else 
            concat(year(date), '/', year(date) + 1)
    end as tax_year
from CashStatementItem
where Accountcode = 'GSEJ-SIPP' and (CashStatementItemType = 'Contribution' or CashStatementItemType = 'TaxRelief')
group by case 
    when month(date) < 4 or (month(date) = 4 and day(date) <= 5) then 
        concat(year(date) - 1, '/', year(date))
    else 
        concat(year(date), '/', year(date) + 1)
end
order by tax_year