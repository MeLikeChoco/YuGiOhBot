create or replace function insert_or_get_antisupport(input varchar)
returns setof integer
as $$
begin

	return query
		with new_antisupport as (
			insert into antisupports(name) values(input)
			on conflict do nothing
			returning id
		)
		select * from new_antisupport
		union
		select id from antisupports where name = input;

end;
$$
language plpgsql;