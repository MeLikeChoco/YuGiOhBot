create procedure insert_support_relation(cardname character varying, support character varying)
    language plpgsql
as
$$
BEGIN

    insert into card_to_antisupports values
        (
            (select antisupports from cards where name ilike cardname),
            (select * from insert_or_get_support(support))
        )
    on conflict do nothing;

END;
$$;