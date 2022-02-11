create or replace function get_card_exact(input varchar)
returns setof joined_cards
language plpgsql
as $$
begin
	
	return query
		select * from joined_cards
		where 
			name ilike input or
			realname ilike input
		order by name;

end
$$;