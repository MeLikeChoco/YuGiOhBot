create or replace function get_card_with_id(input varchar)
returns setof cards
as $func$
begin
	
	return query
		select * from cards where id = input;

end;
$func$
language plpgsql;