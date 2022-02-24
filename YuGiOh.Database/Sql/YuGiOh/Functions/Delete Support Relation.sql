create procedure delete_support_relation(cardname character varying, support character varying)
    language plpgsql
as
$$
BEGIN

    delete from card_to_supports
    where
            cardsupportsid = (select supports from cards where name ilike cardname) and
            supportsid = (select id from supports where name ilike support);

END;
$$;