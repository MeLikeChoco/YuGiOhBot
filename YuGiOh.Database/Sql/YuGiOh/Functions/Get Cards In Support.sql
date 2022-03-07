create function get_cards_in_support(input character varying) returns SETOF joined_cards
    language plpgsql
as
$$
begin

    return query
        select *
        from joined_cards
        where supportname ilike input
        order by name;

end;
$$;