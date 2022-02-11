create or replace function get_cards_contains(input varchar)
returns setof joined_cards
language plpgsql
as $$
begin

	input := '%' || input || '%';
	
	return query
		select * from joined_cards 
		where 
			name ~~* input or
			realname ~~* input 
		order by name;

end
$$;