create or replace function get_card_archetypes(id integer)
returns setof varchar
language plpgsql
as $$
#variable_conflict use_variable
begin

	return query
		select a.name from archetypes a
		inner join card_to_archetypes ca on ca.archetypesId = a.id
		inner join cards c on c.archetypes = ca.cardArchetypesId
		where c.archetypes = id;

end
$$;