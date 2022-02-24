create procedure insert_antisupport_relation(cardname character varying, archetype character varying)
    language plpgsql
as
$$
BEGIN

    insert into card_to_antisupports values
        (
            (select antisupports from cards where name ilike cardname),
            (select * from insert_or_get_antisupport(archetype))
        )
    on conflict do nothing;

END;
$$;