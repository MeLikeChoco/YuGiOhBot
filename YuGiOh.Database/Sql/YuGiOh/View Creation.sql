create or replace view joined_cards
as 
	select 
		cards.*, 
		ar.name as archetypename, 
		s.name as supportname, 
		asu.name as antisupportname 
	from cards
	left join card_to_archetypes ca on ca.cardarchetypesid = cards.archetypes
	left join archetypes ar on ar.id = ca.archetypesid
	left join card_to_supports cs on cs.cardsupportsid = cards.supports
	left join supports s on s.id = cs.supportsid
	left join card_to_antisupports cas on cas.cardantisupportsid = cards.antisupports
	left join antisupports asu on asu.id = cas.antisupportsid;