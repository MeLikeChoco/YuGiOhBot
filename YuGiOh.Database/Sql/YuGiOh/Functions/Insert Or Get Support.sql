create or replace function insert_or_get_support(input varchar)
returns setof integer
as $$
begin

	return query
		with new_support as (
			insert into supports(name) values(input)
			on conflict do nothing
			returning id
		)
		select * from new_support
		union
		select id from supports where name = input;

end;
$$
language plpgsql;