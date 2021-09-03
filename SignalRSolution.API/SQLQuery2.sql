select tarih,[1],[2],[3],[4],[5]  FROM 
(select [City],[Count],Cast([CovidDate] as date) as tarih FROM Covids) as covidT
PIVOT
(SUM(Count) for City IN([1],[2],[3],[4],[5])) as ptable
order by tarih asc