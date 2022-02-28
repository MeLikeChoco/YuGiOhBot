create function upsert_translation(cardid integer, language character varying, name character varying,
                                   lore character varying) returns integer
    language plpgsql
as
$$
declare
    result integer;
BEGIN

    insert into translations(cardid, language, name, lore)
    values (cardid, language, name, lore)
    on conflict on constraint cardid_language_pair_unique
        do update
        set name = excluded.name,
            lore = excluded.lore
    where translations.cardid = excluded.cardid
      and translations.language = excluded.language
      and (translations.name != excluded.name or translations.lore != excluded.lore)
    returning id into result;

    return result;

END;
$$;