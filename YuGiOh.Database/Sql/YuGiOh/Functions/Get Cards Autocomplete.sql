create or replace function get_cards_autocomplete(input varchar)
returns setof joined_cards
as $$
declare
	contains varchar;
	starts_with varchar;
begin

	contains := '%' || input || '%';
	starts_with := input || '%';

	return query
		with results as 
		(
			select * from joined_cards
			where name ilike contains
		)
		select results.* from results
		inner join
			(
				select id, name from results
				group by id, name
				order by
					case
						when name ilike starts_with then 0
						else 1
					end,
					name
				limit 25
			) absurdly_long_temporary_name_because_it_doesnt_matter
			using (id)
		order by
			case
				when results.name ilike starts_with then 0
				else 1
			end,
			name
		;

	-- double order by is needed because inner join and group by doesn't preserve order
	-- so we need the top n groups with cards that start with the input first and then cards that contain the input

end
$$
language plpgsql;