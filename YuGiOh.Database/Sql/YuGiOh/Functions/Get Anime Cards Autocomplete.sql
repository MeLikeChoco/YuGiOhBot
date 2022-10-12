create function get_anime_cards_autocomplete(input character varying) returns SETOF character varying
    language plpgsql
as
$$
declare
    contains    varchar;
    starts_with varchar;
BEGIN

    contains := '%' || input || '%';
    starts_with := input || '%';

    return query
        select name
        from anime_cards
        where name ilike contains
        order by case
                     when name ilike starts_with then 0
                     else 1
                     end,
                 name
        limit 25;

END;
$$;