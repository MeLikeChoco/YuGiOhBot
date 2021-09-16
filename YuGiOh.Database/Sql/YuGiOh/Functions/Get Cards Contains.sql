create or replace function get_cards_contains(input varchar)
returns setof varchar
as $func$
begin
	
	return query
		select name from cards where name ~* input or realname ~* input order by name;

end;
$func$
language plpgsql;