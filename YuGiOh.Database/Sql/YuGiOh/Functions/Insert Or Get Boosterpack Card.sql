create function insert_or_get_boosterpack_card(_boosterpackcardsid integer, _name character varying) returns SETOF integer
    language plpgsql
as
$$
BEGIN

    return query
        with new_boosterpack_card as
                 (
                     insert into boosterpack_cards(boosterpackcardsid, name) values(_boosterpackcardsid, _name)
                         on conflict do nothing
                         returning id
                 )
        select * from new_boosterpack_card
        union
        select id from boosterpack_cards
        where
                boosterpackcardsid = _boosterpackcardsid and
                name = _name
    ;

END;
$$;