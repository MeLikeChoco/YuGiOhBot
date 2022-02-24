create procedure insert_archetype_relation(cardname character varying, archetype character varying)
    language plpgsql
as
$$
BEGIN

    insert into card_to_archetypes values
        (
            (select archetypes from cards where name ilike cardname),
            (select * from insert_or_get_archetype(archetype))
        )
    on conflict do nothing;

END;
$$;