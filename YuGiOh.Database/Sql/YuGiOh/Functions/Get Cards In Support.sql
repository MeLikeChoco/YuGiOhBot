create or replace function get_cards_in_support(input varchar)
returns setof cards
as $$
begin
	
	return query
		select c.* from cards c
		inner join card_to_supports cs on cs.cardsupportsid = c.supports
		inner join supports s on s.id = cs.supportsid
		where s.name ~~* input
		order by c.name asc;

end;
$$
language plpgsql;