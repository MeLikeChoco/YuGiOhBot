create or replace function get_boosterpacks_autocomplete(input varchar)
    returns setof varchar
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
        from boosterpacks
        where name ilike contains
        order by case
                     when name ilike starts_with then 0
                     else 1
                     end,
                 name
        limit 25;

end;
$$;