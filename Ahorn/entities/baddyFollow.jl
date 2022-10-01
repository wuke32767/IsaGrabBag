module IsaGrabBagBaddyFollow

using ..Ahorn, Maple

@mapdef Entity "isaBag/baddyFollow" BadelineFriend(x::Integer, y::Integer, nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[])

const placements = Ahorn.PlacementDict(
	"Badeline Friend (IsaGrabBag)" => Ahorn.EntityPlacement(
		BadelineFriend,
		"rectangle"
	)
)

function Ahorn.selection(entity::BadelineFriend)
	x, y = Ahorn.position(entity)
	return Ahorn.getSpriteRectangle("isafriend/baddyAhorn.png", x, y)
	
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BadelineFriend, room::Maple.Room)
	Ahorn.drawSprite(ctx, "isafriend/baddyAhorn.png", 0, 0)
end

end