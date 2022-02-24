create function get_supports_autocomplete(input character varying) returns SETOF character varying
    language plpgsql
as
$$
declare
    contains varchar;
    starts_with varchar;
begin

    contains := '%' || input || '%';
    starts_with := input || '%';

    return query
        select name from supports
        where name ilike contains
        order by
            case
                when name ilike starts_with then 0
                else 1
                end,
            name
        limit 25;

END;
$$;
