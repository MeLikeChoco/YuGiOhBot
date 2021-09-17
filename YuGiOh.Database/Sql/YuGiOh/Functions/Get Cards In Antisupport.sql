create or replace function get_cards_in_antisupport(input varchar)
returns setof cards
as $$
begin
	
	return query
		select c.* from cards c
		inner join card_to_antisupports ca on ca.cardantisupportsid = c.antisupports
		inner join antisupports a on a.id = ca.antisupportsid
		where a.name ~~* input
		order by c.name asc;

end;
$$
language plpgsql;