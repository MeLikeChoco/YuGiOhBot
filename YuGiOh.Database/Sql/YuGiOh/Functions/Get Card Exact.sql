create or replace function get_card_exact(input varchar)
returns setof cards
as $func$
begin
	
	return query
		select * from cards where name ~~* input or realname ~~* input order by name;

end;
$func$
language plpgsql;