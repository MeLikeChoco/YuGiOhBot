create function search_cards(input character varying) returns SETOF joined_cards
    language plpgsql
as
$$
declare
    contains varchar;
begin

    contains := '%' || input || '%';

    return query
        select * from joined_cards
        where
                name ilike contains or
                realname ilike contains
        order by
            case
                when name ilike input then 0
                else 1
                end,
            name
    ;

end
$$;