delete from cards
where
  -- YuGiOh.Common.Test
    name !~ 'Raidraptor - Revolution Falcon - Air Raid|D/D/D Deviser King Deus Machinex|Astrograph Sorcerer' and
    id not in
    (
        (
            select id from cards
            where name ~* 'book'
            order by random()
            limit 5
        )
        union all
        (
            select id from cards
            where name ~* 'ghost'
            order by random()
            limit 5
        )
        union all
        (
            select id from cards
            where name ~* 'dragon'
            order by random()
            limit 5
        )
    ) and
    name !~* all('{black,assault,the,rain}') and
    name !~* all('{blue,dragon,white,eyes}') and
    id not in
    (
        select id from cards
        where name ~* all('{le,an,at,l,te}')
        order by random()
        limit 5
    ) and
    name !~ 'Awakening of the Possessed - Nefariouser Archfiend|Carpiponica, Mystical Beast of the Forest|Karakuri Steel Shogun mdl 00X "Bureido"' and
    id not in
    (
        with temp as
                 (
                     select cards.id, a.name as archetypeName from cards
                                                                       inner join card_to_archetypes cta on cards.archetypes = cta.cardarchetypesid
                                                                       inner join archetypes a on a.id = cta.archetypesid
                 )
            (
                select id from temp
                where archetypeName ~* 'elemental hero'
                order by random()
                limit 5
            )
        union all
        (
            select id from temp
            where archetypeName ~* 'Infernity'
            order by random()
            limit 5
        )
        union all
        (
            select id from temp
            where archetypeName ~* 'SyNcHrOn'
            order by random()
            limit 5
        )
        union all
        (
            select id from temp
            where archetypeName ~* 'genex'
            order by random()
            limit 5
        )
        union all
        (
            select id from temp
            where archetypeName ~* 'WaRrioR'
            order by random()
            limit 5
        )
        union all
        (
            select id from temp
            where archetypeName ~* 'drAiN'
            order by random()
            limit 5
        )
    ) and
    id not in
    (
        with temp as
                 (
                     select cards.id, s.name as supportName from cards
                                                                     inner join card_to_supports cts on cards.supports = cts.cardsupportsid
                                                                     inner join supports s on cts.supportsid = s.id
                 )
            (
                select id from temp
                where supportName ~* 'dark'
                order by random()
                limit 5
            )
        union all
        (
            select id from temp
            where supportName ~* 'cYbErSe'
            order by random()
            limit 5
        )
        union all
        (
            select id from temp
            where supportName ~* 'Die Roll'
            order by random()
            limit 5
        )
        union all
        (
            select id from temp
            where supportName ~* 'FIEND'
            order by random()
            limit 5
        )
        union all
        (
            select id from temp
            where supportName ~* 'Fusion Material'
            order by random()
            limit 5
        )
        union all
        (
            select id from temp
            where supportName ~* 'number 39: utopia'
            order by random()
            limit 5
        )
    ) and
    id not in
    (
        with temp as
                 (
                     select cards.id, a2.name as antisupportName from cards
                                                                          inner join public.card_to_antisupports c on cards.antisupports = c.cardantisupportsid
                                                                          inner join public.antisupports a2 on a2.id = c.antisupportsid
                 )
            (
                select id from temp
                where antisupportName ~* 'Synchro Monster'
                order by random()
                limit 5
            )
        union all
        (
            select id from temp
            where antisupportName ~* 'flIp moNsTeR'
            order by random()
            limit 5
        )
        union all
        (
            select id from temp
            where antisupportName ~* 'equip card'
            order by random()
            limit 5
        )
        union all
        (
            select id from temp
            where antisupportName ~* 'kaiju'
            order by random()
            limit 5
        )
        union all
        (
            select id from temp
            where antisupportName ~* 'Dinosaur'
            order by random()
            limit 5
        )
        union all
        (
            select id from temp
            where antisupportName ~* 'fISh'
            order by random()
            limit 5
        )
    ) and
    name !~~* 'number|PeRFoRmApAL|dragon' and
    passcode != '11759079' and
    name !~* 'Crystal Girl' and
    id not in
    (
        (
            (
                select id from cards
                where ocgstatus = 'Forbidden'
                order by random()
                limit 5
            )
            union all
            (
                select id from cards
                where ocgstatus = 'Limited'
                order by random()
                limit 5
            )
            union all
            (
                select id from cards
                where ocgstatus = 'Semi-Limited'
                order by random()
                limit 5
            )
        )
        union all
        (
            (
                select id from cards
                where tcgadvstatus = 'Forbidden'
                order by random()
                limit 5
            )
            union all
            (
                select id from cards
                where tcgadvstatus = 'Limited'
                order by random()
                limit 5
            )
            union all
            (
                select id from cards
                where tcgadvstatus = 'Semi-Limited'
                order by random()
                limit 5
            )
        )
    )
returning *
;

delete from boosterpacks
where
    id not in
    (
        with temp as
                 (
                     select id, name from boosterpacks
                 )
            (
                select id from temp
                where name ~* 'Duelist'
                order by random()
                limit 5
            )
        union all
        (
            select id from temp
            where name ~* 'tOUrNaMenT'
            order by random()
            limit 5
        )
        union all
        (
            select id from temp
            where name ~* 'premium'
            order by random()
            limit 5
        )
    )
returning *
;

delete from anime_cards
where
    id not in
    (
        (
            select id from anime_cards
            where name ~* 'dragon'
            order by random()
            limit 5
        )
        union all
        (
            select id from anime_cards
            where name ~* 'harpie'
            order by random()
            limit 5
        )
        union all
        (
            select id from anime_cards
            where name ~* 'magician'
            order by random()
            limit 5
        )
    ) and
    id not in
    (
        (
            select id from anime_cards
            where name ~* 'number 1'
            order by random()
            limit 5
        )
        union all
        (
            select id from anime_cards
            where name ~* 'Red'
            order by random()
            limit 5
        )
        union all
        (
            select id from anime_cards
            where name ~* 'plANeT'
            order by random()
            limit 5
        )
    ) and
    id not in
    (
        select id from anime_cards tablesample bernoulli(0.5)
    )
returning *
;