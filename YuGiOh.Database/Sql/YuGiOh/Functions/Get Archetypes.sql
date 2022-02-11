create or replace function get_archetypes()
returns setof varchar
as $$
begin

	return query
		select name from archetypes;

end
$$ 
language plpgsql;