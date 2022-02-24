create or replace view joined_cards
            (id, name, realname, cardtype, property, types, attribute, materials, lore, archetypes, supports,
             antisupports, link, linkarrows, atk, def, level, pendulumscale, rank, tcgexists, ocgexists, img, url,
             passcode, ocgstatus, tcgadvstatus, tcgtrnstatus, cardtrivia, archetypename, supportname, antisupportname,
             translationid, cardid, language, translationname, translationlore)
as
    SELECT cards.*,
           ar.name AS archetypename,
           s.name AS supportname,
           asu.name AS antisupportname,
           tr.id AS translationid,
           tr.cardid,
           tr.language,
           tr.name AS translationname,
           tr.lore AS translationlore
    FROM cards
             LEFT JOIN card_to_archetypes ca ON ca.cardarchetypesid = cards.archetypes
             LEFT JOIN archetypes ar ON ar.id = ca.archetypesid
             LEFT JOIN card_to_supports cs ON cs.cardsupportsid = cards.supports
             LEFT JOIN supports s ON s.id = cs.supportsid
             LEFT JOIN card_to_antisupports cas ON cas.cardantisupportsid = cards.antisupports
             LEFT JOIN antisupports asu ON asu.id = cas.antisupportsid
             LEFT JOIN translations tr ON tr.cardid = cards.id;


create or replace view joined_boosterpacks
            (id, name, dates, cards, url, tcgexists, ocgexists, boosterpackdatename, boosterpackdate,
             boosterpackcardname, boosterpackrarity)
as
    SELECT b.*,
           bd.name AS boosterpackdatename,
           bd.date AS boosterpackdate,
           bc.name AS boosterpackcardname,
           br.name AS boosterpackrarity
    FROM boosterpacks b
             JOIN boosterpack_cards bc ON bc.boosterpackcardsid = b.cards
             JOIN boosterpack_rarities br ON br.boosterpackraritiesid = bc.rarities
             JOIN boosterpack_dates bd ON bd.boosterpackdatesid = b.dates;