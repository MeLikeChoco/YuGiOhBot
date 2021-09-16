create or replace function get_card_supports(id integer)
returns setof varchar
language plpgsql
as $$
#variable_conflict use_variable
begin

	return query
		select a.name from supports a
		inner join card_to_supports ca on ca.supportsId = a.id
		inner join cards c on c.supports = ca.cardsupportsId
		where c.supports = id;

end
$$;