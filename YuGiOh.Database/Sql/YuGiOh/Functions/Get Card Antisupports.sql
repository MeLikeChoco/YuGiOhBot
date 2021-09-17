create or replace function get_card_antisupports(id integer)
returns setof varchar
language plpgsql
as $$
#variable_conflict use_variable
begin

	return query
		select a.name from antisupports a
		inner join card_to_antisupports ca on ca.antisupportsId = a.id
		inner join cards c on c.antisupports = ca.cardantisupportsId
		where c.antisupports = id;

end
$$;