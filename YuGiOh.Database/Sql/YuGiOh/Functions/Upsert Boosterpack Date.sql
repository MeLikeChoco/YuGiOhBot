create function upsert_boosterpack_date(boosterpackdatesid integer, name character varying, date character varying) returns integer
    language plpgsql
as
$$
declare
    result integer;
BEGIN

    insert into boosterpack_dates(boosterpackdatesid, name, date) values(boosterpackdatesid, name, date)
    on conflict on constraint boosterpack_dates_boosterpackid_name_unique_pair
        do
            update set
                       name = excluded.name,
                       date = excluded.date
    returning id into result;

    return result;

END;
$$;