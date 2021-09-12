create or replace procedure clear_tables()
language plpgsql
as $$
begin

	truncate table cards cascade;
	truncate table archetypes cascade;
	truncate table supports cascade;
	truncate table antisupports cascade;
	truncate table boosterpacks cascade;

end
$$;