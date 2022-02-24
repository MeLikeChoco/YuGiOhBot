create function get_random_card() returns SETOF joined_cards
    language plpgsql
as
$$
BEGIN

    return query
        select * from joined_cards
                          inner join
                      (
                          select id from cards
                                             tablesample bernoulli(1)
                          order by random()
                          limit 1
                      ) absurdly_long_name_because_it_doesnt_matter
                      using (id)
    ;

END;
$$;