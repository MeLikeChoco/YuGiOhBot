create function get_random_monster() returns setof joined_cards
    language plpgsql
as
$$
    BEGIN 
        
        return query 
            select * from joined_cards
            inner join
            (
                select id from cards
                tablesample bernoulli(0.1)
                where cards.cardtype = 'Monster'
                order by random()
                limit 1
            ) absurdly_long_name_because_it_doesnt_matter
            using (id)
        ;
        
    end;
$$