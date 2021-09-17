create or replace function insert_or_get_archetype(input varchar)
returns setof integer
as $$
begin

	return query
		with new_archetype as (
			insert into archetypes(name) values(input)
			on conflict do nothing
			returning id
		)
		select * from new_archetype
		union
		select id from archetypes where name = input;

end;
$$
language plpgsql;