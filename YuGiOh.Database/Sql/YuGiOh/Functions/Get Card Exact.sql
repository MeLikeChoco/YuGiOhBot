create or replace function get_card_exact(input varchar)
returns setof cards
language plpgsql
as $$
begin
	
	return query
		select * from cards
		where 
			name ~~* input or
			realname ~~* input
		order by name
		limit 1;

end
$$;