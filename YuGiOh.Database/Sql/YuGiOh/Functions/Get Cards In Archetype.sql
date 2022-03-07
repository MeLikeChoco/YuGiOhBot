create function get_cards_in_archetype(input character varying)
    returns SETOF joined_cards
    language plpgsql
as
$$
begin

    return query
        select *
        from joined_cards
        where archetypename ilike input
        order by name;

end
$$;