create or replace function search_cards(
	input varchar,
	parameterized_input varchar
)
returns setof cards
language plpgsql
as $$
begin

	return query
		select * from get_card_exact(input)
		union
		select * from get_cards_contains(parameterized_input)
		order by name asc;

end
$$;