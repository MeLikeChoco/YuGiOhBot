create or replace function get_archetypes_autocomplete(input varchar)
returns setof varchar
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
			select * from archetypes
			where name ilike contains
		)
		select results.name from results
		inner join
			(
				select * from results
				group by id, name
				order by
					case 
						when name ilike starts_with then 0
						else 1
					end,
					name
				limit 25
			) absurdly_long_name_because_it_doesnt_matter
			using (id)
		order by
			case 
				when results.name ilike starts_with then 0
				else 1
			end,
			name
		;

end
$$
language plpgsql;