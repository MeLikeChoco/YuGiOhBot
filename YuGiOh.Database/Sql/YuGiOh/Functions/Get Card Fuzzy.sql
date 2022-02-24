create function get_card_fuzzy(input character varying) returns SETOF joined_cards
    language plpgsql
as
$$
BEGIN

    return query
        select *
        from joined_cards
                 inner join
             (
                 select id
                 from cards
                 order by levenshtein(name, input) asc,
                          name asc
                 limit 1
             ) absurdly_long_name_because_it_doesnt_matter
             using (id);

END;
$$;