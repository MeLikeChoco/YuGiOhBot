create or replace function get_cards_starts_with(input varchar)
returns setof joined_cards
language plpgsql
as $$
begin

	input := input || '%';
	
	return query
		select * from joined_cards
		where
			name ilike input or
			realname ilike input
		order by name asc;

end
$$;