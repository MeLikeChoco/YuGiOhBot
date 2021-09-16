create or replace function search_cards(input varchar)
returns setof varchar
language plpgsql
as $$
begin

	-- the union order matters or it cannot union due to type mismatch

	return query
		select * from (
			select get_cards_contains(input)
			union			
			select name from get_card_exact(input)
		) as cards
		order by 1 asc;

end
$$;