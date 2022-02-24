create procedure delete_antisupport_relation(cardname character varying, antisupport character varying)
    language plpgsql
as
$$
BEGIN

    delete from card_to_antisupports
    where
            cardantisupportsid = (select antisupports from cards where name ilike cardname) and
            antisupportsid = (select id from antisupports where name ilike antisupport);

END;
$$;