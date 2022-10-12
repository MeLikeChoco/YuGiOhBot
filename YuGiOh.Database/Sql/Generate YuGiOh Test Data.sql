delete from cards
where
        name !~* 'Raidraptor - Revolution Falcon - Air Raid|D/D/D Deviser King Deus Machinex|Astrograph Sorcerer' and
        name !~* 'book|ghost|dragon' and
        name !~* all('{black,assault,the,rain}') and
        name !~* all('{blue,dragon,white,eyes}') and
        name !~* all('{le,an,at,l,te}') and
        name !~* 'Awakening of the Possessed - Nefariouser Archfiend|Carpiponica, Mystical Beast of the Forest|Karakuri Steel Shogun mdl 00X "Bureido"' and
        id in
        (
            select cards.id from cards
                                     inner join card_to_archetypes cta on cards.archetypes = cta.cardarchetypesid
                                     inner join archetypes a on a.id = cta.archetypesid
            where a.name !~* 'elemental hero|Infernity|SyNcHrOn'
        ) and
        id in
        (
            select cards.id from cards
                                     inner join card_to_supports cts on cards.supports = cts.cardsupportsid
                                     inner join supports s on s.id = cts.supportsid
            where s.name !~* 'dark|cYbErSe|Hamon, Lord of Striking Thunder'
        ) and
        id in
        (
            select cards.id from cards
                                     inner join card_to_antisupports c on cards.antisupports = c.cardantisupportsid
                                     inner join antisupports a2 on a2.id = c.antisupportsid
            where a2.name !~* 'Synchro Monster|flIp moNsTeR|equip card'
        ) and
        name !~* 'number|PeRFoRmApAL|dragon' and
        id not in
        (
            select id from cards tablesample bernoulli(0.17)
        )
;

delete from boosterpacks
where
        name !~* 'Duelist|tOUrNaMenT|premium'
;

delete from anime_cards
where
        name !~* 'dragon|harpie|magician' and
        name !~* 'number 1|Red|plANeT' and
        id not in
        (
            select id from anime_cards tablesample bernoulli(0.5)
        )
;

truncate errors;