create or replace function search_cards(input varchar)
returns setof joined_cards
language plpgsql
as $$
begin

	return query
		(select * from get_card_exact(input))
		union
		(select * from get_cards_contains(input))
		order by name asc;

end
$$;

--create or replace function search_cards(input varchar)
--returns setof joined_cards
--language plpgsql
--as $$
--begin

--	return query
--		with cards as
--		(
--			select * from get_card_exact(input)
--			union
--			select * from get_cards_contains(input)
--		)
--		select * from cards
--		inner join
--			(
--				select distinct name
--				from cards
--				order by name
--				limit 25
--			) absurdly_long_temporary_name_because_it_doesnt_matter
--				using (name)
--		order by cards.name

--end
--$$;