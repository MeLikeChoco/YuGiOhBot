create or replace function get_random_card()
returns setof cards
as $$
begin

	return query
		select * from cards
		tablesample bernoulli(1)
		order by random()
		limit 1;

end;
$$
language plpgsql;