create or replace function get_card_fuzzy(input varchar)
returns setof cards
as $func$
begin
	
	return query
		select * from cards order by levenshtein(name, input) asc, name asc limit 1;

end;
$func$
language plpgsql;