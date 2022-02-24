create procedure delete_archetype_relation(cardname character varying, archetype character varying)
    language plpgsql
as
$$
BEGIN

    delete from card_to_archetypes
    where
            cardarchetypesid = (select archetypes from cards where name ilike cardname) and
            archetypesid = (select id from archetypes where name ilike archetype);

END;
$$;