create or replace function get_cards_in_archetype(input varchar)
returns setof varchar
as $$
begin
	
	return query
		select c.name from cards c
		inner join card_to_archetypes ca on ca.cardarchetypesid = c.archetypes
		inner join archetypes a on a.id = ca.archetypesid
		where a.name ~~* input;

end;
$$
language plpgsql;