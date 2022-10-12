create function search_anime_cards(input character varying) returns SETOF anime_cards
    language plpgsql
as
$$
declare
    contains varchar;
begin

    contains := '%' || input || '%';

    return query
        select * from anime_cards
        where
            name ilike contains
        order by
            case
                when name ilike input then 0
                else 1
                end,
            name
    ;

end
$$;